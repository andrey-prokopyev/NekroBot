namespace NekroBot
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    internal class BotRunner
    {
        private readonly IEnumerable<IBotComponent> botComponents;

        public BotRunner(IEnumerable<IBotComponent> botComponents)
        {
            this.botComponents = botComponents;
        }

        public async Task Run()
        {
            await Task.WhenAll(this.botComponents.Select(c => c.Run()));
            await Loop();
        }

        private async Task Loop()
        {
            while (true)
            {
                await Task.Delay(TimeSpan.FromSeconds(10)).ConfigureAwait(false);
            }
        }
    }
}