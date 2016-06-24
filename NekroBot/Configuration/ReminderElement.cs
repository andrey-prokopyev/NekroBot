namespace NekroBot.Configuration
{
    using System.Configuration;

    public class ReminderElement : ConfigurationElement
    {
        [ConfigurationProperty("name")]
        public string Name => (string)this["name"];

        [ConfigurationProperty("schedule")]
        public string Schedule => (string)this["schedule"];

        [ConfigurationProperty("message")]
        public string Message => (string)this["message"];
    }
}