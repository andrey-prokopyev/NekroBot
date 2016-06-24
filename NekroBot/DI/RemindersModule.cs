namespace NekroBot.DI
{
    using System.Configuration;
    using System.Linq;
    using System.Reactive.Subjects;

    using Autofac;

    using NekroBot.Configuration;
    using NekroBot.Messages;
    using NekroBot.Reminders;
    using NekroBot.Reminders.Quartz;

    public class RemindersModule : Module
    {
        /// <summary>
        /// Override to add registrations to the container.
        /// </summary>
        /// <remarks>
        /// Note that the ContainerBuilder parameter is unique to this module.
        /// </remarks>
        /// <param name="builder">The builder through which components can be
        ///             registered.</param>
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<ReminderComponent>()
                .As<IBotComponent>()
                .SingleInstance();

            builder.RegisterType<QuartzScheduler>()
                .As<IScheduler>()
                .SingleInstance();

            builder.RegisterType<ReminderConfiguration>()
                .SingleInstance();

            var cfg = (ReminderSection)ConfigurationManager.GetSection("reminders");
            
            var reminderElements = cfg?.Settings?.Cast<ReminderElement>();
            if (reminderElements != null)
            {
                foreach (var reminderCfg in reminderElements)
                {
                    builder.Register(c => new ScheduledItem { Name = reminderCfg.Name, Schedule = new Schedule { CronExpression = reminderCfg.Schedule }, Reminder = new MessageAction(c.Resolve<Subject<MessageUpdate>>(), reminderCfg.Message) })
                        .As<ScheduledItem>()
                        .SingleInstance();
                }
            }
        }  
    }
}