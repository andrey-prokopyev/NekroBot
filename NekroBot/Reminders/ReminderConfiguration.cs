namespace NekroBot.Reminders
{
    using System.Collections.Generic;

    internal class ReminderConfiguration
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="T:System.Object"/> class.
        /// </summary>
        public ReminderConfiguration(IEnumerable<ScheduledItem> scheduledItems)
        {
            ScheduledItems = scheduledItems;
        }

        public IEnumerable<ScheduledItem> ScheduledItems { get; }
    }
}