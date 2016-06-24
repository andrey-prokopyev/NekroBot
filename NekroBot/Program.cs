using System.ServiceProcess;

namespace NekroBot
{
    using System;
    using System.Diagnostics;
    using System.IO;
    using System.Linq;
    using System.Text;

    using Common.Logging;

    public static class Program
    {
        public static StringBuilder sb = new StringBuilder();

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        public static void Main(string[] args)
        {
            var Log = LogManager.GetLogger("Program");
            //Console.OutputEncoding = Encoding.UTF8;

            if (args.Contains("/console"))
            {
                Log.Info(m => m("Запуск в режиме консоли"));

                var runner = new BotStarter();
                runner.Start().Wait();
            }
            else
            {
                Log.Info(m => m("Запуск в режиме сервиса"));

                ServiceBase.Run(new NekroService());
            }
        }
    }
}
