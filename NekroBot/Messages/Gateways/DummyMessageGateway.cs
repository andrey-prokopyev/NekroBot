namespace NekroBot.Messages.Gateways
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;
    using System.Linq;
    using System.Threading.Tasks;

    using Common.Logging;

    internal class DummyMessageGateway : IMessageGateway
    {
        private static readonly ILog Log = LogManager.GetLogger<DummyMessageGateway>();

        public string Name { get; } = "TestMessageGateway";

        public MessageGatewayCapabilities Capabilities { get; } = MessageGatewayCapabilities.SendMessage;

        public IEnumerable<MessageUpdate> GetMessageUpdates()
        {
            yield return new MessageUpdate { Sender = "TestSender", Source = Name, Text = $"This is Test, and now is '{DateTime.Now.ToString(CultureInfo.InvariantCulture)}'" };
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