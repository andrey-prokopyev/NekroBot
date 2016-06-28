namespace NekroBot.DI
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Configuration;
    using System.Linq;
    using System.Reactive.Linq;
    using System.Reactive.Subjects;

    using Autofac;
    using Autofac.Core;

    using NekroBot.Data;
    using NekroBot.Messages;
    using NekroBot.Messages.Gateways;
    using NekroBot.Messages.Gateways.Ipb;
    using NekroBot.Messages.Gateways.Telegram;
    using NekroBot.State;

    public class MessageModule : Module
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
            builder.RegisterType<MessageComponent>()
                .WithParameter(ResolvedParameter.ForNamed<IObservable<long>>("IntervalObservable"))
                .As<IBotComponent>()
                .SingleInstance();

            var ignoredUsersConfig = (IDictionary)ConfigurationManager.GetSection("messaging/ignored-users");

            builder.Register(c => new MessageSender(c.Resolve<IEnumerable<IMessageGateway>>(), ignoredUsersConfig?.Keys.Cast<string>() ?? new[] { "nbot", "NekroBot" }))
                .SingleInstance();

            builder.RegisterType<MessageUpdateFetcher>()
                .SingleInstance();

            builder.RegisterType<DummyMessageGateway>()
                .As<IMessageGateway>()
                .SingleInstance();

            builder.RegisterType<Formatter>()
                .SingleInstance();

            builder.RegisterType<TelegramDataContext>()
                .As<ITelegramState>()
                .SingleInstance();

            var telegramCfg = (IDictionary)ConfigurationManager.GetSection("messaging/telegram");
            var telegramCapabilitiesCfg = (IDictionary)ConfigurationManager.GetSection("messaging/capabilities/telegram");

            builder.Register(c => new TelegramMessageGateway(ConfigHelper.GetValueOrDefault<string>(telegramCfg, "api-key"), c.Resolve<Formatter>(), GetCapabilities(telegramCapabilitiesCfg), c.Resolve<ITelegramState>()))
                .As<IMessageGateway>()
                .SingleInstance();

            var ipbCfg = (IDictionary)ConfigurationManager.GetSection("messaging/ipb");
            var ipbCapabilitiesCfg = (IDictionary)ConfigurationManager.GetSection("messaging/capabilities/ipb");

            builder.Register(c => new IpbMessageGateway(ConfigHelper.GetValueOrDefault(ipbCfg, "origin", "http://ipb.local"), ConfigHelper.GetValueOrDefault(ipbCfg, "user", "nbot"), ConfigHelper.GetValueOrDefault<string>(ipbCfg, "password"), c.Resolve<Formatter>(), GetCapabilities(ipbCapabilitiesCfg)))
                .As<IMessageGateway>()
                .SingleInstance();

            var pollingCfg = (IDictionary)ConfigurationManager.GetSection("messaging/polling");

            builder.Register(c => Observable.Interval(pollingCfg != null && pollingCfg.Contains("interval") ? TimeSpan.Parse(pollingCfg["interval"].ToString()) : TimeSpan.FromSeconds(1)))
                .Named<IObservable<long>>("IntervalObservable")
                .As<IObservable<long>>()
                .SingleInstance();

            builder.RegisterType<Subject<MessageUpdate>>()
                .SingleInstance();
        }

        private MessageGatewayCapabilities GetCapabilities(IDictionary capabilitiesCfg)
        {
            if (capabilitiesCfg != null && capabilitiesCfg.Keys.Count != 0)
            {
                var capabilities = capabilitiesCfg.Keys.Cast<string>()
                    .Select(k => (MessageGatewayCapabilities)Enum.Parse(typeof(MessageGatewayCapabilities), k)).ToArray();

                var result = capabilities.First();

                return capabilities.Aggregate(result, (current, capability) => current | capability);
            }

            return MessageGatewayCapabilities.GetUpdates | MessageGatewayCapabilities.SendMessage;
        }
    }
}