using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.ElasticLoadBalancingV2;
using Amazon.CDK.AWS.SSM;
using Constructs;
using Protocol = Amazon.CDK.AWS.EC2.Protocol;

namespace Infrastructure.Vpc;

public class VpcStack : Stack
{
    private const string DefaultRegion = "us-east-1";
    private const string BaseNamespace = "todo";
    private const string ServiceName = "vpc";
    private const string CloudFrontPrefixListId = "pl-3b927c52";
    
    public IVpc Vpc { get; set; }
    public ApplicationLoadBalancer LoadBalancer { get; set; }
    internal VpcStack(Construct scope, string stackId, IStackProps props) : base(scope, stackId,
        props)
    {        
        const string serviceNamespace = BaseNamespace + "." + ServiceName;
        const string dashedServiceNamespace = BaseNamespace + "-" + ServiceName;
        var region = props.Env?.Region ?? DefaultRegion;
        
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
            SubnetType = SubnetType.PRIVATE_WITH_EGRESS
        };
        var availabilityZones = new[] {region + "a", region + "b"};
        Vpc = new Amazon.CDK.AWS.EC2.Vpc(this, BaseNamespace + ".VPC", new VpcProps
        {
            IpAddresses = IpAddresses.Cidr("10.0.0.0/16"),
            DefaultInstanceTenancy = DefaultInstanceTenancy.DEFAULT,
            SubnetConfiguration = new ISubnetConfiguration[]
            {
                subnetPub, subnetPrivate
            },
            AvailabilityZones = availabilityZones
        });

        var cloudFrontPrefixList = Peer.PrefixList(CloudFrontPrefixListId);
        
        var httpSecurityGroup = new SecurityGroup(this, serviceNamespace + ".alb.sg.http", new SecurityGroupProps
        {
            AllowAllOutbound = true,
            SecurityGroupName = serviceNamespace + ".alb.securityGroup.http",
            Vpc = Vpc,
        });
        httpSecurityGroup.AddIngressRule(cloudFrontPrefixList, new Port(new PortProps
        {
            FromPort = 80, ToPort = 80, Protocol = Protocol.TCP, StringRepresentation = "80:80:TCP"
        }));
        var httpsSecurityGroup = new SecurityGroup(this, serviceNamespace + ".alb.sg.https", new SecurityGroupProps
        {
            AllowAllOutbound = true,
            SecurityGroupName = serviceNamespace + ".alb.securityGroup.https",
            Vpc = Vpc
        });
        httpsSecurityGroup.AddIngressRule(cloudFrontPrefixList, new Port(new PortProps
        {
            FromPort = 443, ToPort = 443, Protocol = Protocol.TCP, StringRepresentation = "443:443:TCP"
        }));
        
        var unused = new StringParameter(this, serviceNamespace + ".stringParameter.alb.https.sg", new StringParameterProps
        {
            ParameterName = serviceNamespace + ".alb.https.sg",
            StringValue = httpsSecurityGroup.SecurityGroupId
        });
        
        LoadBalancer = new ApplicationLoadBalancer(this, serviceNamespace + ".alb", new ApplicationLoadBalancerProps
        {
            Vpc = Vpc,
            InternetFacing = true,
            VpcSubnets = new SubnetSelection {
                SubnetType = SubnetType.PUBLIC,
                AvailabilityZones = availabilityZones
            },
            IpAddressType = IpAddressType.IPV4,
            LoadBalancerName = dashedServiceNamespace + "-alb",
            SecurityGroup = httpSecurityGroup
        });
        LoadBalancer.AddSecurityGroup(httpsSecurityGroup);
        
        var targetGroupArn = StringParameter.ValueFromLookup(this, "todo.instance.targetGroup");
        var targetGroup = ApplicationTargetGroup.FromTargetGroupAttributes(this, serviceNamespace + ".targetGroup",
            new TargetGroupAttributes
            {
                TargetGroupArn = targetGroupArn
            });
        
        LoadBalancer.AddListener(serviceNamespace + ".alb.http.listener", new ApplicationListenerProps
        {
            Protocol = ApplicationProtocol.HTTP,
            Port = 80,
            DefaultTargetGroups = new [] { targetGroup },
        });
        
        var certificateArn = StringParameter.ValueFromLookup(this, "todo.routing.stringParameter.certificate.arn");
        var certificate = ListenerCertificate.FromArn(certificateArn);
        LoadBalancer.AddListener(serviceNamespace + ".alb.https.listener", new ApplicationListenerProps
        {
            Protocol = ApplicationProtocol.HTTPS,
            Port = 443,
            SslPolicy = SslPolicy.RECOMMENDED,
            Certificates = new IListenerCertificate[]{certificate},
            DefaultAction = ListenerAction.Forward(new []{targetGroup})
        });
    }
}