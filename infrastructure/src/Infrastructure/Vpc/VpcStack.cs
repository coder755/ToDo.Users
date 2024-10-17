using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Constructs;

namespace Infrastructure.Vpc;

public class VpcStack : Stack
{
    private const string DefaultRegion = "us-east-1";
    private const string BaseNamespace = "todo";
    private const string ServiceName = "vpc";
    
    public IVpc Vpc { get; set; }
    internal VpcStack(Construct scope, string stackId, IStackProps props) : base(scope, stackId,
        props)
    {        
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
    }
}