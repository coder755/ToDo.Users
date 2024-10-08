using Amazon.CDK;

namespace Infrastructure
{
    internal static class Program
    {
        private const string Account = "442042533215";
        private const string Region = "us-east-1";

        public static void Main(string[] args)
        {
            var evn = new Environment
            {
                Account = Account,
                Region = Region
            };
            var props = new StackProps { Env = evn };

            var app = new App();
            var unused = new UsersStack(app, "TodoUsersStack", "todoUsers", props);

            app.Synth();
        }
    }
}
