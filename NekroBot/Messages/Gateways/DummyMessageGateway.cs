namespace NekroBot.Messages.Gateways
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Common.Logging;

    internal class DummyMessageGateway : IMessageGateway
    {
        private static readonly ILog Log = LogManager.GetLogger<DummyMessageGateway>();

        public string Name { get; } = "TestMessageGateway";

        public IEnumerable<MessageUpdate> GetMessageUpdates()
        {
            //yield return new MessageUpdate { Sender = "TestSender", Source = Name, Text = $"This is Test, and now is '{DateTime.Now.ToString(CultureInfo.InvariantCulture)}'" };
            return Enumerable.Empty<MessageUpdate>();
        }

        public Task Send(MessageUpdate messageUpdate)
        {
            return Task.Run(
                () =>
                    {
                        Log.Trace(m => m($"Отправлено сообщение {messageUpdate}"));
                    });
        }
    }
}