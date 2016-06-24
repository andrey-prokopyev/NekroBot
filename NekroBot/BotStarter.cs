namespace NekroBot
{
    using System.Threading.Tasks;

    using Autofac;

    using Common.Logging;

    internal class BotStarter
    {
        private static readonly ILog Log = LogManager.GetLogger<BotStarter>();

        private IContainer container;

        private BotRunner botRunner;

        public async Task Start()
        {
            var builder = new ContainerBuilder();

            builder.RegisterAssemblyModules(GetType().Assembly);

            this.container = builder.Build();

            Log.Debug(m => m("Загружена конфигурация DI"));

            this.botRunner = this.container.Resolve<BotRunner>();

            await this.botRunner.Run();
        }
    }
}