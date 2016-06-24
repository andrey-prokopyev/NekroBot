namespace NekroBot.DI
{
    using System.Collections;
    using System.Configuration;

    public static class ConfigHelper
    {
        public static T GetValueOrDefault<T>(IDictionary config, string key, T defaultValue = default(T))
        {
            return config != null && config.Contains(key) ? (T)config[key] : defaultValue;
        }

        public static T GetValueOrDefault<T>(string sectionPath, string key, T defaultValue = default(T))
        {
            var cfgSection = (IDictionary)ConfigurationManager.GetSection("sectionPath");

            return GetValueOrDefault(cfgSection, key, defaultValue);
        }
    }
}