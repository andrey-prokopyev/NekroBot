using System.ServiceProcess;

namespace NekroBot
{
    using System;

    using Common.Logging;

    internal partial class NekroService : ServiceBase
    {
        private static readonly ILog Log = LogManager.GetLogger<NekroService>();

        private readonly BotController controller;

        public NekroService()
        {
            this.controller = new BotController();
            InitializeComponent();
        }

        protected async override void OnStart(string[] args)
        {
            try
            {
                await this.controller.Start();
            }
            catch (Exception e)
            {
                Log.Error(m => m("При запуске сервиса произошла ошибка"), e);
                this.Stop();
            }
        }

        protected override async void OnStop()
        {
            try
            {
                await this.controller.Stop();
            }
            catch (Exception e)
            {
                Log.Error(m => m("При остановке сервиса произошла ошибка"), e);
            }
            finally
            {
                this.Dispose(true);
            }
        }
    }
}
