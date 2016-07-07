namespace NekroBot.Reminders
{
    using System;
    using System.Threading.Tasks;

    internal interface IScheduler : IDisposable
    {
        Task AddReminder(ScheduledItem scheduledItem);
    }
}