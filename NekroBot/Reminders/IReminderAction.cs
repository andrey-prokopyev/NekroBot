namespace NekroBot.Reminders
{
    using System.Threading.Tasks;

    public interface IReminderAction
    {
        Task Execute();
    }
}