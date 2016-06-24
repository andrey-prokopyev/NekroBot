namespace NekroBot.Reminders
{
    using System.Threading.Tasks;

    internal interface IScheduler
    {
        Task AddReminder(ScheduledItem scheduledItem);
    }
}