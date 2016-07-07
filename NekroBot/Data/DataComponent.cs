using System.Threading.Tasks;

namespace NekroBot.Data
{
    using System;
    using System.Configuration;
    using System.IO;

    using Common.Logging;

    public class DataComponent : IBotComponent
    {
        private static readonly ILog Log = LogManager.GetLogger<DataComponent>();

        public Task Run()
        {
            return Task.Run(
                () =>
                    {
                        var dataFolderPath = ConfigurationManager.AppSettings["data-folder-path"];
                        Log.Debug(m => m($"data-folder-path равен '{dataFolderPath}'"));

                        var executionPath = Environment.CurrentDirectory;
                        
                        string fullPath = Path.IsPathRooted(dataFolderPath) ? dataFolderPath : Path.GetFullPath(Path.Combine(executionPath, dataFolderPath));
                        AppDomain.CurrentDomain.SetData("DataDirectory", fullPath);

                        Log.Debug(m => m($"DataDirectory установлен в '{fullPath}'"));
                    });
        }

        public Task Stop()
        {
            return Task.FromResult(false);
        }
    }
}
