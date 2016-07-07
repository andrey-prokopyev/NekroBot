using System.ServiceProcess;

namespace NekroBot
{
    using System.Linq;

    using Common.Logging;

    public static class Program
    {
        private static readonly ILog Log = LogManager.GetLogger("Program");

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        public static void Main(string[] args)
        {
            if (args.Contains("/console"))
            {
                Log.Info(m => m("Запуск в режиме консоли"));

                var runner = new BotController();
                runner.Start().Wait();

                runner.Stop().Wait();
            }
            else
            {
                Log.Info(m => m("Запуск в режиме сервиса"));

                ServiceBase.Run(new NekroService());
            }
        }
    }
}
