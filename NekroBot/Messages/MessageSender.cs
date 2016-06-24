namespace NekroBot.Messages
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Common.Logging;

    public class MessageSender
    {
        private static readonly ILog Log = LogManager.GetLogger<MessageSender>();

        private readonly IEnumerable<IMessageGateway> gateways;

        private readonly IEnumerable<string> ignoredUserNames;

        public MessageSender(IEnumerable<IMessageGateway> gateways, IEnumerable<string> ignoredUserNames)
        {
            this.gateways = gateways;
            this.ignoredUserNames = ignoredUserNames;
        }

        public void Send(MessageUpdate messageUpdate)
        {
            try
            {
                if (this.ignoredUserNames.Contains(messageUpdate.Sender))
                {
                    Log.Trace(m => m($"Отправитель сообщения [{messageUpdate}] содержится в списке игнорируемых. Сообщение не будет отправлено"));
                    return;
                }

                var targets = this.gateways.Where(g => g.Name != messageUpdate.Source);

                var sendTasks = targets.Select(t => t.Send(messageUpdate));

                Task.WaitAll(sendTasks.ToArray());
            }
            catch (Exception e)
            {
                Log.Error(m => m($"При отправке сообщения [{messageUpdate}] произошла ошибка"), e);
            }
        }
    }
}