namespace NekroBot
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Common.Logging;

    internal class BotDisposer
    {
        private static readonly ILog Log = LogManager.GetLogger<BotDisposer>();

        private readonly IEnumerable<IBotComponent> botComponents;

        public BotDisposer(IEnumerable<IBotComponent> botComponents)
        {
            this.botComponents = botComponents;
        }

        public async Task Dispose()
        {
            var disposeTasks = this.botComponents.Select(b => b.Stop());

            await Task.WhenAll(disposeTasks);

            Log.Info(m => m("Бот успешно остановлен"));
        }
    }
}