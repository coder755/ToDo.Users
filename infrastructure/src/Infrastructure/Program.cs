using Amazon.CDK;
using Infrastructure.Database;
using Infrastructure.FrontEnd;
using Infrastructure.Instance;
using Infrastructure.Routing;
using Infrastructure.Vpc;

namespace Infrastructure;
internal static class Program
{
    private const string Account = "442042533215";
    private const string Region = "us-east-1";

    public static void Main(string[] args)
    {
        var app = new App();
        var env = new Environment
        {
            Account = Account,
            Region = Region
        };
        var props = new StackProps { Env = env };
        var vpcStack = new VpcStack(app, "TodoVpcStack", props);
        var frontEndStack = new FrontEndStack(app, "TodoFrontEndStack", props);
        var routingStack = new RoutingStack(app, "TodoRoutingStack", new RoutingStackProps
        {
            Env = env,
            LoadBalancer = vpcStack.LoadBalancer,
            Bucket = frontEndStack.Bucket,
        });
        var instanceStack = new InstanceStack(app, "TodoInstanceStack" , new InstanceStackProps
        {
            Env = env,
            Vpc = vpcStack.Vpc,
        });
        var databaseStack = new DatabaseStack(app, "TodoDbStack", new DatabaseStackProps
        {          
            Env = env,
            Vpc = vpcStack.Vpc
        });
        
        app.Synth();
    }
}

