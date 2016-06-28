namespace NekroBot.Messages
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reactive.Subjects;
    using System.Threading.Tasks;

    using Common.Logging;

    public class MessageUpdateFetcher
    {
        private static readonly ILog Log = LogManager.GetLogger<MessageUpdateFetcher>();

        private readonly IEnumerable<IMessageGateway> gateways;

        private readonly Subject<MessageUpdate> messageUpdateObservable;

        public MessageUpdateFetcher(IEnumerable<IMessageGateway> gateways, Subject<MessageUpdate> messageUpdateObservable)
        {
            this.gateways = gateways;
            this.messageUpdateObservable = messageUpdateObservable;
        }

        public void ProcessUpdates()
        {
            try
            {
                Log.Trace(m => m("Запускается запрос новых сообщений"));

                var updateTasks = this.gateways.Where(g => g.Capabilities.HasFlag(MessageGatewayCapabilities.GetUpdates)).Select(
                    g => Task.Run(
                        () =>
                            {
                                var updates = g.GetMessageUpdates();

                                foreach (var messageUpdate in updates)
                                {
                                    this.messageUpdateObservable.OnNext(messageUpdate);
                                }
                            })).ToArray();

                Task.WaitAll(updateTasks);
            }
            catch (Exception e)
            {
                Log.Error(m => m("При запросе обновлений произошла ошибка"), e);
            }
        }
    }
}