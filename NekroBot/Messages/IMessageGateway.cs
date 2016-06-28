namespace NekroBot.Messages
{
    using System.Collections.Generic;
    using System.Threading.Tasks;

    public interface IMessageGateway
    {
        IEnumerable<MessageUpdate> GetMessageUpdates();

        Task Send(MessageUpdate messageUpdate);

        string Name { get; }

        MessageGatewayCapabilities Capabilities { get; }
    }
}