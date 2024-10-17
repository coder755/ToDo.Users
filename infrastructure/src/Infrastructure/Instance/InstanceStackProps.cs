using Amazon.CDK;
using Amazon.CDK.AWS.CertificateManager;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.ElasticLoadBalancingV2;

namespace Infrastructure.Instance;

public class InstanceStackProps : StackProps
{
    public IVpc Vpc { get; set; }
}