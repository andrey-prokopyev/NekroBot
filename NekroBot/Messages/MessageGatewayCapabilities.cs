namespace NekroBot.Messages
{
    using System;

    [Flags]
    public enum MessageGatewayCapabilities
    {
        GetUpdates = 1,
        SendMessage = 2
    }
}