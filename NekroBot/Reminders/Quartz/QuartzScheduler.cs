namespace NekroBot.Reminders.Quartz
{
    using System.Collections.Specialized;
    using System.Threading.Tasks;

    using Common.Logging;

    using global::Quartz;
    using global::Quartz.Impl;

    using LogManager = Common.Logging.LogManager;

    internal class QuartzScheduler : Reminders.IScheduler
    {
        private static readonly ILog Log = LogManager.GetLogger<QuartzScheduler>();
        private static readonly object SchedulerLock = new object();

        private const string JobGroup = "NekroBot";

        private IScheduler scheduler;

        public Task AddReminder(ScheduledItem scheduledItem)
        {
            return Task.Run(
                () =>
                    {
                        EnsureSchedulerIsReady();

                        var jobData = new JobDataMap { [ReminderActionJob.ActionFieldName] = scheduledItem.Reminder };

                        var job = JobBuilder.Create<ReminderActionJob>()
                            .UsingJobData(jobData)
                            .WithIdentity(scheduledItem.Name, JobGroup)
                            .Build();

                        var trigger = TriggerBuilder.Create()
                            .WithIdentity(scheduledItem.Name)
                            .StartNow()
                            .WithCronSchedule(scheduledItem.Schedule.CronExpression, b => b.WithMisfireHandlingInstructionDoNothing())
                            .Build();
                        
                        scheduler.ScheduleJob(job, trigger);
                    });
        }


        private void EnsureSchedulerIsReady()
        {
            if (this.scheduler != null) return;

            lock (SchedulerLock)
            {
                var schedulerParams = new NameValueCollection
                                          {
                                              {"quartz.threadPool.threadCount", "1"}
                                          };
                var factory = new StdSchedulerFactory(schedulerParams);
                scheduler = factory.GetScheduler();
                scheduler.Start();

                Log.Info("Starting QuartzScheduler.");
            }
        }

        /// <summary>
        /// Выполняет определяемые приложением задачи, связанные с удалением, высвобождением или сбросом неуправляемых ресурсов.
        /// </summary>
        public void Dispose()
        {
            this.scheduler.Shutdown(true);
        }
    }
}