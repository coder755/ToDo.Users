using System.Collections.Generic;
using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.ECR;
using Amazon.CDK.AWS.ECS;
using Amazon.CDK.AWS.ElasticLoadBalancingV2;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Logs;
using Amazon.CDK.AWS.RDS;
using Amazon.CDK.AWS.SecretsManager;
using Constructs;
using Newtonsoft.Json;
using HealthCheck = Amazon.CDK.AWS.ElasticLoadBalancingV2.HealthCheck;
using Protocol = Amazon.CDK.AWS.EC2.Protocol;
using Secret = Amazon.CDK.AWS.SecretsManager.Secret;

namespace Infrastructure;

public class UsersStack : Stack
{
    private const string TodoCertArn =
        "arn:aws:acm:us-east-1:442042533215:certificate/40bc5552-6dca-4274-a2b7-c371785777e9";
    private const string region = "us-east-1";
    internal UsersStack(Construct scope, string stackId, string serviceId, IStackProps props) : base(scope, stackId, props)
    {
        var baseNamespace = "todo";
        var serviceNamespace = baseNamespace + "." + serviceId;
        var dashedServiceNamespace = baseNamespace + "-" + serviceId;

        var subnetPub = new SubnetConfiguration
        {
            CidrMask = 24,
            Name = dashedServiceNamespace + "-db-subnet-public",
            SubnetType = SubnetType.PUBLIC
        };
        var subnetPrivate = new SubnetConfiguration
        {
            CidrMask = 24,
            Name = dashedServiceNamespace + "-db-subnet-private",
            SubnetType = SubnetType.PRIVATE_ISOLATED
        };
        var availabilityZones = new string[] {region + "a", region + "b", region + "c"};
        var todoVpc = new Vpc(this, baseNamespace + ".VPC", new VpcProps
        {
            IpAddresses = IpAddresses.Cidr("10.0.0.0/16"),
            DefaultInstanceTenancy = DefaultInstanceTenancy.DEFAULT,
            SubnetConfiguration = new ISubnetConfiguration[]
            {
                subnetPub, subnetPrivate
            },
            AvailabilityZones = availabilityZones
        });
        
        // set up database
        var dbSecret = new Secret(this, serviceNamespace + ".secrete.db", new SecretProps
        {
            SecretName = "todoDbSecret",
            Description = "Credentials to the RDS instance",
            GenerateSecretString = new SecretStringGenerator
            {
                SecretStringTemplate = JsonConvert.SerializeObject(new Dictionary<string, string> { { "username", "coder" } }),
                GenerateStringKey = "password",
                ExcludeCharacters = "@/\\\" "
            }
        });

        // security groups
        var albSecurityGroup = new SecurityGroup(this, serviceNamespace + ".alb.sg", new SecurityGroupProps
        {
            AllowAllOutbound = true,
            SecurityGroupName = serviceNamespace + ".alb.securityGroup",
            Vpc = todoVpc
        });
        albSecurityGroup.AddIngressRule(Peer.AnyIpv4(), new Port(new PortProps
        {
            FromPort = 80, ToPort = 80, Protocol = Protocol.TCP, StringRepresentation = "80:80:TCP"
        }));
        albSecurityGroup.AddIngressRule(Peer.AnyIpv4(), new Port(new PortProps
        {
            FromPort = 443, ToPort = 443, Protocol = Protocol.TCP, StringRepresentation = "443:443:TCP"
        }));
        var fargateSecGroup = new SecurityGroup(this, serviceNamespace + ".fargateService.securityGroup", new SecurityGroupProps
        {
            AllowAllOutbound = true,
            SecurityGroupName = serviceNamespace + ".fargateService.securityGroup",
            Vpc = todoVpc
        });
        fargateSecGroup.AddIngressRule(Peer.AnyIpv4(), new Port(new PortProps
        {
            FromPort = 80, ToPort = 80, Protocol = Protocol.TCP, StringRepresentation = "80:80:TCP", 
        }));
        fargateSecGroup.AddIngressRule(albSecurityGroup, Port.Tcp(80), "Allow traffic from ALB to Fargate on port 80");
        fargateSecGroup.AddEgressRule(Peer.AnyIpv4(), Port.Tcp(3306), "Allow outbound traffic to MySQL on port 3306");
        
        var dbSecurityGroup = new SecurityGroup(this, serviceNamespace + ".securityGroup.db", new SecurityGroupProps
        {
            Vpc = todoVpc,
            SecurityGroupName = dashedServiceNamespace + "-db-securityGroup",
            AllowAllOutbound = true
        });
        dbSecurityGroup.AddIngressRule(fargateSecGroup, Port.Tcp(3306), "Allow Fargate to access db");
        
        var dbEngine = DatabaseInstanceEngine.Mysql(new MySqlInstanceEngineProps
        {
            Version = MysqlEngineVersion.VER_8_0_32
        });

        var database = new DatabaseInstance(this, serviceNamespace + ".db", new DatabaseInstanceProps
        {
            Vpc = todoVpc,
            VpcSubnets = new SubnetSelection { SubnetType = SubnetType.PUBLIC },
            InstanceType = InstanceType.Of(InstanceClass.T3, InstanceSize.MICRO),
            AllocatedStorage = 20,
            Engine = dbEngine,
            InstanceIdentifier = dashedServiceNamespace + "-db",
            Credentials = Credentials.FromSecret(dbSecret),
            SecurityGroups = new ISecurityGroup[] { dbSecurityGroup },
            RemovalPolicy = RemovalPolicy.DESTROY,
            PubliclyAccessible = false,
        });

        // create ECS cluster
        var cluster = new Cluster(this, serviceNamespace + ".cluster", new ClusterProps
        {
            ClusterName = dashedServiceNamespace + "-cluster",
            Vpc = todoVpc, 
            EnableFargateCapacityProviders = true,
        });
        
        // setup the load balancer
        var applicationLoadBalancer = new ApplicationLoadBalancer(this, serviceNamespace + ".alb", new ApplicationLoadBalancerProps
        {
            Vpc = todoVpc,
            SecurityGroup = albSecurityGroup,
            InternetFacing = true,
            VpcSubnets = new SubnetSelection {
                SubnetType = SubnetType.PUBLIC
            },
            IpAddressType = IpAddressType.IPV4,
            LoadBalancerName = dashedServiceNamespace
        });
        var targetGroup = new ApplicationTargetGroup(this, serviceNamespace + ".targetGroup", new ApplicationTargetGroupProps
        {
            Protocol = ApplicationProtocol.HTTP,
            Port = 80,
            HealthCheck = new HealthCheck()
            {
                Path = "/healthcheck"
            },
            TargetType = TargetType.IP,
            Vpc = todoVpc,
            TargetGroupName = dashedServiceNamespace
        });
        applicationLoadBalancer.AddListener(serviceNamespace + ".alb.listener", new ApplicationListenerProps
        {
            Protocol = ApplicationProtocol.HTTP,
            Port = 80,
            DefaultTargetGroups = new IApplicationTargetGroup[]{ targetGroup }
        });
        
        var listenerCertificate = new ListenerCertificate(TodoCertArn);
        applicationLoadBalancer.AddListener(serviceNamespace + ".alb.https.listener", new ApplicationListenerProps
        {
            Protocol = ApplicationProtocol.HTTPS,
            Port = 443,
            SslPolicy = SslPolicy.RECOMMENDED,
            Certificates = new IListenerCertificate[]{listenerCertificate},
            DefaultAction = ListenerAction.Forward(new IApplicationTargetGroup[]{targetGroup})
        });
        
        var ecsRole = new Role(this, serviceNamespace + ".ecsRole", new RoleProps
        {
            AssumedBy = new ServicePrincipal("ecs-tasks.amazonaws.com")
        });
        
        ecsRole.AddToPolicy(new PolicyStatement(new PolicyStatementProps
        {
            Effect = Effect.ALLOW,
            Actions = new []{
                "ecr:GetAuthorizationToken",
                "ecr:BatchCheckLayerAvailability",
                "ecr:GetDownloadUrlForLayer",
                "ecr:BatchGetImage",
                "logs:CreateLogStream",
                "logs:PutLogEvents"
                },
            Resources = new []{"*"}
        }));
        
        var taskRole = new Role(this, serviceNamespace + ".taskRole", new RoleProps
        {
            AssumedBy = new ServicePrincipal("ecs-tasks.amazonaws.com")
        });
        
        taskRole.AddToPolicy(new PolicyStatement(new PolicyStatementProps
        {
            Effect = Effect.ALLOW,
            Actions = new[]
            {
                "secretsmanager:GetResourcePolicy",
                "secretsmanager:GetSecretValue",
                "secretsmanager:DescribeSecret",
                "secretsmanager:ListSecretVersionIds",
                "secretsmanager:ListSecrets",
                "sns:Publish"
            },
            Resources = new[] { "*" }
        }));
        
        // create ECR
        var ecrRepository = new Repository(this, serviceNamespace + ".ecr", new RepositoryProps{ RepositoryName = serviceNamespace.ToLower()});
        
        // create the task definition
        var taskDefinition = new TaskDefinition(this, serviceNamespace + ".taskDefinition", new TaskDefinitionProps {
            Compatibility = Compatibility.FARGATE,
            Family = dashedServiceNamespace,
            Cpu = "1024",
            MemoryMiB = "3072",
            RuntimePlatform = new RuntimePlatform
            {
                OperatingSystemFamily = OperatingSystemFamily.LINUX,
                CpuArchitecture = CpuArchitecture.X86_64,
            },
            ExecutionRole =  ecsRole,
            TaskRole = taskRole
        });
        taskDefinition.AddContainer(serviceNamespace + ".container", new ContainerDefinitionOptions
        {
            ContainerName = dashedServiceNamespace + "-container",
            Image = ContainerImage.FromEcrRepository(ecrRepository, "todo.users"),
            PortMappings = new IPortMapping[] { new PortMapping
            {
                Name = "todotempportname",
                ContainerPort = 80,
                Protocol = Amazon.CDK.AWS.ECS.Protocol.TCP,
                AppProtocol = AppProtocol.Http
            } },
            Logging = new AwsLogDriver(new AwsLogDriverProps
            {
                StreamPrefix = dashedServiceNamespace + "-container",
                LogGroup = new LogGroup(this, serviceNamespace + ".container.log.groups", new LogGroupProps
                {
                    LogGroupName = dashedServiceNamespace + "-container"
                })
            })
        });
        
        // set up the Fargate Service

        
        var fargateService = new FargateService(this, serviceNamespace + ".fargateService", new FargateServiceProps
        {
            Cluster   = cluster,
            TaskDefinition = taskDefinition,
            AssignPublicIp = true,
            ServiceName = dashedServiceNamespace + "-fargate-service",
            VpcSubnets = new SubnetSelection {
                SubnetType = SubnetType.PUBLIC
            },
            SecurityGroups = new ISecurityGroup[] {fargateSecGroup },
            DesiredCount = 1,
        });
        fargateService.AttachToApplicationTargetGroup(targetGroup);
    }
}

