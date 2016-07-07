namespace NekroBot.Data
{
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Linq;

    using NekroBot.State;

    internal class TelegramDataContext : DbContext, ITelegramState
    {
        public TelegramDataContext()
            : base("name=TelegramDataContext")
        {
        }

        public virtual DbSet<TelegramState> TelegramState { get; set; }

        public void SaveChat(long chatId)
        {
            this.TelegramState.AddOrUpdate(new TelegramState { ChatId = chatId });
            this.SaveChanges();
        }

        public long GetChat()
        {
            return this.TelegramState.FirstOrDefault()?.ChatId ?? 0;
        }
    }
}
