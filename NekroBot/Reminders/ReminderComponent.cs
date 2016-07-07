namespace NekroBot.Reminders
{
    using System.Linq;
    using System.Threading.Tasks;

    internal class ReminderComponent : IBotComponent
    {
        private readonly IScheduler scheduler;

        private readonly ReminderConfiguration reminderConfiguration;

        public ReminderComponent(IScheduler scheduler, ReminderConfiguration reminderConfiguration)
        {
            this.scheduler = scheduler;
            this.reminderConfiguration = reminderConfiguration;
        }

        public Task Run()
        {
            var addTasks = this.reminderConfiguration.ScheduledItems.Select(s => this.scheduler.AddReminder(s));

            return Task.WhenAll(addTasks);
        }

        public Task Stop()
        {
            return Task.Run(
                () =>
                    {
                        this.scheduler.Dispose();
                    });
        }
    }
}