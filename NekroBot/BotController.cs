namespace NekroBot
{
    using System.Threading.Tasks;

    using Autofac;

    using Common.Logging;

    internal class BotController
    {
        private static readonly ILog Log = LogManager.GetLogger<BotController>();

        private IContainer container;

        private BotRunner botRunner;

        private BotDisposer disposer;

        public async Task Start()
        {
            var builder = new ContainerBuilder();

            builder.RegisterAssemblyModules(GetType().Assembly);

            this.container = builder.Build();

            Log.Debug(m => m("Загружена конфигурация DI"));

            this.botRunner = this.container.Resolve<BotRunner>();
            this.disposer = this.container.Resolve<BotDisposer>();

            await this.botRunner.Run();
        }

        public Task Stop()
        {
            return this.disposer.Dispose();
        }
    }
}