namespace NekroBot.Reminders
{
    using System.Reactive.Subjects;
    using System.Threading.Tasks;

    using NekroBot.Messages;

    public class MessageAction : IReminderAction
    {
        private const string SourceName = "Reminder";
        private readonly Subject<MessageUpdate> messageUpdateObservable;

        private readonly string message;

        public MessageAction(Subject<MessageUpdate> messageUpdateObservable, string message)
        {
            this.messageUpdateObservable = messageUpdateObservable;
            this.message = message;
        }

        public Task Execute()
        {
            return Task.Run(
                () =>
                    {
                        var update = new MessageUpdate {Sender = string.Empty, Source = SourceName, Text = this.message};
                        this.messageUpdateObservable.OnNext(update);

                    });
        }
    }
}