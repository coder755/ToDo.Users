using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.ECR;
using Amazon.CDK.AWS.ECS;
using Amazon.CDK.AWS.ElasticLoadBalancingV2;
using Amazon.CDK.AWS.IAM;
using Amazon.CDK.AWS.Logs;
using Amazon.CDK.AWS.SSM;
using Constructs;
using HealthCheck = Amazon.CDK.AWS.ElasticLoadBalancingV2.HealthCheck;
using Protocol = Amazon.CDK.AWS.EC2.Protocol;

namespace Infrastructure.Instance;

public class InstanceStack : Stack
{
    private const string BaseNamespace = "todo";
    private const string ServiceName = "instance";
    internal InstanceStack(Construct scope, string stackId, InstanceStackProps props) : base(scope, stackId, props)
    {
        const string serviceNamespace = BaseNamespace + "." + ServiceName;
        const string dashedServiceNamespace = BaseNamespace + "-" + ServiceName;

        // create ECS cluster
        var cluster = new Cluster(this, serviceNamespace + ".cluster", new ClusterProps
        {
            ClusterName = dashedServiceNamespace + "-cluster",
            Vpc = props.Vpc, 
            EnableFargateCapacityProviders = true,
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
            Vpc = props.Vpc,
            TargetGroupName = dashedServiceNamespace
        });
        
        var unused = new StringParameter(this, serviceNamespace + "stringParameter.targetGroup", new StringParameterProps
        {
            ParameterName = serviceNamespace + ".targetGroup",
            StringValue = targetGroup.TargetGroupArn
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
        var ecrRepository = new Repository(this, serviceNamespace + ".ecr", new RepositoryProps
        {
            RepositoryName = serviceNamespace.ToLower(), RemovalPolicy = RemovalPolicy.DESTROY
        } );
        
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
                Name = dashedServiceNamespace + "-container-port-mapping",
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
        var loadBalancerHttpsSgId = StringParameter.ValueFromLookup(this, "todo.vpc.alb.https.sg");
        var loadBalancerHttpsSg = SecurityGroup.FromSecurityGroupId(this, serviceNamespace + ".lb.https.sg", loadBalancerHttpsSgId );
        var fargateSecGroup = new SecurityGroup(this, serviceNamespace + ".fargateService.securityGroup", new SecurityGroupProps
        {
            AllowAllOutbound = true,
            SecurityGroupName = serviceNamespace + ".fargateService.securityGroup",
            Vpc = props.Vpc
        });
        fargateSecGroup.AddIngressRule(Peer.AnyIpv4(), new Port(new PortProps
        {
            FromPort = 80, ToPort = 80, Protocol = Protocol.TCP, StringRepresentation = "80:80:TCP", 
        }));
        fargateSecGroup.AddIngressRule(loadBalancerHttpsSg, Port.AllTcp(), "Allow traffic from ALB to Fargate on all ports");
        
        var unused2 = new StringParameter(this, serviceNamespace + ".stringParameter.fargate.sg", new StringParameterProps
        {
            ParameterName = serviceNamespace + ".fargate.sg",
            StringValue = fargateSecGroup.SecurityGroupId
        });
        
        var fargateService = new FargateService(this, serviceNamespace + ".fargateService", new FargateServiceProps
        {
            Cluster   = cluster,
            TaskDefinition = taskDefinition,
            AssignPublicIp = false,
            ServiceName = dashedServiceNamespace + "-fargate-service",
            VpcSubnets = new SubnetSelection {
                SubnetType = SubnetType.PRIVATE_WITH_EGRESS
            },
            SecurityGroups = new ISecurityGroup[] {fargateSecGroup},
            DesiredCount = 1,
        });
        fargateService.AttachToApplicationTargetGroup(targetGroup);
    }
}

