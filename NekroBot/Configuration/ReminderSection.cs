namespace NekroBot.Configuration
{
    using System.Configuration;

    public class ReminderSection : ConfigurationSection
    {
        [ConfigurationProperty("", IsDefaultCollection = true)]
        public ReminderSettingsCollection Settings => (ReminderSettingsCollection)this[string.Empty];
    }
}