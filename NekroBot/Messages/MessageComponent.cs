namespace NekroBot.Messages
{
    using System;
    using System.Reactive.Subjects;
    using System.Threading.Tasks;

    using Common.Logging;

    public class MessageComponent : IBotComponent
    {
        private static readonly ILog Log = LogManager.GetLogger<MessageComponent>();

        private readonly IObservable<long> timerObservable;

        private readonly Subject<MessageUpdate> messageUpdateObservable;

        private readonly MessageUpdateFetcher messageUpdateFetcher;

        private readonly MessageSender messageSender;

        public MessageComponent(IObservable<long> timerObservable, Subject<MessageUpdate> messageUpdateObservable, MessageUpdateFetcher messageUpdateFetcher, MessageSender messageSender)
        {
            this.timerObservable = timerObservable;
            this.messageUpdateObservable = messageUpdateObservable;
            this.messageUpdateFetcher = messageUpdateFetcher;
            this.messageSender = messageSender;
        }

        public async Task Run()
        {
            Log.Debug(m => m("Запускается компонент обмена сообщениями"));

            await Task.Run(
                () =>
                    {
                        this.timerObservable.Subscribe(l => this.messageUpdateFetcher.ProcessUpdates());
                        this.messageUpdateObservable.Subscribe(m => this.messageSender.Send(m));
                    });
        }

        public Task Stop()
        {
            return Task.Run(() => { messageUpdateObservable.Dispose(); });
        }
    }
}