namespace NekroBot.Messages.Gateways.Telegram
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;

    using Common.Logging;

    using global::Telegram.Bot;
    using global::Telegram.Bot.Types;

    using Newtonsoft.Json;

    internal class TelegramMessageGateway : IMessageGateway
    {
        private static readonly ILog Log = LogManager.GetLogger<TelegramMessageGateway>();

        private readonly Api api;

        private readonly Formatter formatter;

        private int offset;

        private long chatId;

        public TelegramMessageGateway(string apiKey, Formatter formatter, MessageGatewayCapabilities capabilities)
        {
            this.formatter = formatter;
            Capabilities = capabilities;
            this.api = new Api(apiKey);
        }

        public string Name { get; } = "Telegram";

        public MessageGatewayCapabilities Capabilities { get; }

        public IEnumerable<MessageUpdate> GetMessageUpdates()
        {
            var telegramUpdates = this.api.GetUpdates(offset).Result;

            Log.Trace(m => m($"Получено {telegramUpdates.Length} обновлений"));

            var filteredUpdates = telegramUpdates.Where(u => u.Type == UpdateType.MessageUpdate && u.Message.Type == MessageType.TextMessage && u.Message.Chat.Type == ChatType.Supergroup).ToArray();
            var updates = filteredUpdates.Select(u => new MessageUpdate { Source = Name, Sender = $"{u.Message.From.FirstName} {u.Message.From.LastName}", Text = u.Message.Text }).ToArray();

            if (telegramUpdates.Length > filteredUpdates.Length)
            {
                var except = telegramUpdates.Except(filteredUpdates);

                Log.Trace(m => m("Получены следующие необрабатываемые обновления"));

                foreach (var exceptionUpdate in except)
                {
                    string stringifiedUpdate = exceptionUpdate.Id.ToString();
                    try
                    {
                        stringifiedUpdate = JsonConvert.SerializeObject(exceptionUpdate);
                    }
                    catch (Exception e)
                    {
                        Log.Error(m => m($"При сериализации обновления {stringifiedUpdate} произошла ошибка"), e);
                    }

                    Log.Trace(stringifiedUpdate);
                }
            }

            Log.Trace(m => m($"Из {telegramUpdates.Length} обрабатывается {updates.Length} обновлений"));

            if (this.chatId == default(int))
            {
                var updateToChat = filteredUpdates.FirstOrDefault();

                if (updateToChat != null)
                {
                    this.chatId = updateToChat.Message.Chat.Id;

                    Log.Debug(m => m($"Идентификатор чата установлен равным {this.chatId}"));
                }
            }

            if (telegramUpdates.Any())
            {
                this.offset = telegramUpdates.Max(u => u.Id) + 1;

                Log.Trace(m => m($"Значение смещения обновлений изменено на {this.offset}"));
            }

            return updates;
        }

        public async Task Send(MessageUpdate messageUpdate)
        {
            if (this.chatId == default(int))
            {
                Log.Warn(m => m($"Не установлен идентификатор чата. Сообщение {messageUpdate} проигнорировано"));
                return;
            }

            var text = this.formatter.Format(messageUpdate);
            if (string.IsNullOrEmpty(text))
            {
                Log.Trace(m => m($"Отформатированный текст сообщения [{messageUpdate}] является пустым. Сообщение не будет отправлено"));
                return;
            }

            var response = await this.api.SendTextMessage(this.chatId, text);

            Log.Trace(m => m($"Отправлено сообщение {messageUpdate}"));

            if (response == null)
            {
                Log.Error(m => m($"При отправке сообщения {messageUpdate} произошла ошибка"));
            }
        }
    }
}