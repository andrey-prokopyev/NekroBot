namespace NekroBot.DI
{
    using Autofac;

    using NekroBot.Data;

    internal class DataModule : Module
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
            builder.RegisterType<DataComponent>()
                .As<IBotComponent>()
                .SingleInstance();
        }
    }
}
