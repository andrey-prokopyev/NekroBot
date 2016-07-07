namespace NekroBot.Loop
{
    using System;
    using System.Threading;
    using System.Threading.Tasks;

    public class LoopComponent : IBotComponent
    {
        private readonly CancellationTokenSource cancellationTokenSource = new CancellationTokenSource();
        
        public async Task Run()
        {
            while (true)
            {
                await Task.Delay(TimeSpan.FromSeconds(10), this.cancellationTokenSource.Token).ConfigureAwait(false);
            }
        }

        public Task Stop()
        {
            this.cancellationTokenSource.Cancel();
            this.cancellationTokenSource.Dispose();

            return Task.FromCanceled(this.cancellationTokenSource.Token);
        }
    }
}