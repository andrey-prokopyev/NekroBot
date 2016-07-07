namespace NekroBot.State
{
    internal interface ITelegramState
    {
        void SaveChat(long chatId);

        long GetChat();
    }
}