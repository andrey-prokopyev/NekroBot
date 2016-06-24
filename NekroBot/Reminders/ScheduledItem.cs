namespace NekroBot.Reminders
{
    internal class ScheduledItem
    {
        public string Name { get; set; }
        
        public Schedule Schedule;

        public IReminderAction Reminder;
    }
}