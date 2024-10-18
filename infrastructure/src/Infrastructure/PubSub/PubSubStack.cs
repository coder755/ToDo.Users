using Amazon.CDK;
using Amazon.CDK.AWS.EC2;
using Amazon.CDK.AWS.SQS;
using Amazon.CDK.AWS.SSM;
using Constructs;

namespace Infrastructure.PubSub;

public class PubSubStack : Stack
{
    private const string BaseNamespace = "todo";
    private const string ServiceName = "pubsub";
    internal PubSubStack(Construct scope, string stackId, PubSubStackProps props) : base(scope, stackId,
        props)
    {
        const string serviceNamespace = BaseNamespace + "." + ServiceName;
        const string dashedServiceNamespace = BaseNamespace + "-" + ServiceName;

        var dlQueue = new Queue(this, serviceNamespace + ".queue.dlq", new QueueProps
        {
            QueueName = dashedServiceNamespace + "-queue-dlq",
        });

        var queue = new Queue(this, serviceNamespace + ".queue", new QueueProps
        {
            QueueName = dashedServiceNamespace + "-queue",
            VisibilityTimeout = Duration.Seconds(10),
            RetentionPeriod = Duration.Days(4),
            ReceiveMessageWaitTime = Duration.Seconds(20),
            DeadLetterQueue = new DeadLetterQueue
            {
                Queue = dlQueue,
                MaxReceiveCount = 3,
            },
        });
        
        var unused = new StringParameter(this, serviceNamespace + ".stringParameter.queue.url", new StringParameterProps
        {
            ParameterName = serviceNamespace + ".queue.url",
            StringValue = queue.QueueUrl
        });
        
        var sqsEndpoint = props.Vpc.AddInterfaceEndpoint(BaseNamespace + ".vpc.endpoint.sqs", new InterfaceVpcEndpointOptions
        {
            Service = InterfaceVpcEndpointAwsService.SQS,
            PrivateDnsEnabled = true,
            Subnets = new SubnetSelection
            {
                SubnetType = SubnetType.PRIVATE_WITH_EGRESS
            }
        });
    }
}