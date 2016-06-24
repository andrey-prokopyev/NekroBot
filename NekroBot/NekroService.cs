using System.ServiceProcess;

namespace NekroBot
{
    public partial class NekroService : ServiceBase
    {
        private readonly BotStarter starter;

        public NekroService()
        {
            starter = new BotStarter();
            InitializeComponent();
        }

        protected async override void OnStart(string[] args)
        {
            await this.starter.Start();
        }

        protected override void OnStop()
        {
        }
    }
}
