using System.Reflection;
using Abp.Configuration.Startup;
using Abp.Dependency;
using Abp.Events.Bus.Factories;
using Abp.Events.Bus.Handlers;
using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Castle.MicroKernel.SubSystems.Configuration;
using Castle.Windsor;
using DryIoc;

namespace Abp.Events.Bus
{
    /// <summary>
    /// Installs event bus system and registers all handlers automatically.
    /// </summary>
    internal class EventBusInstaller : IDryIocInstaller
    {
        private readonly IIocResolver _iocResolver;
        private readonly IEventBusConfiguration _eventBusConfiguration;
        private IEventBus _eventBus;

        public EventBusInstaller(IIocResolver iocResolver)
        {
            _iocResolver = iocResolver;
            _eventBusConfiguration = iocResolver.Resolve<IEventBusConfiguration>();
        }
        
        public void Install(IIocManager iocManager)
        {
            if (_eventBusConfiguration.UseDefaultEventBus)
            {
                iocManager.IocContainer.UseInstance<IEventBus>(EventBus.Default);
            }
            else
            {
                iocManager.IocContainer.Register<IEventBus,EventBus>(Reuse.Singleton);
            }

            _eventBus = iocManager.Resolve<IEventBus>();
            iocManager.RegisterTypeEventHandler += (manager, type, implementationType) =>
            {
                if (!typeof(IEventHandler).GetTypeInfo().IsAssignableFrom(implementationType))
                {
                    return;
                }

                var interfaces = implementationType.GetTypeInfo().GetInterfaces();
                foreach (var @interface in interfaces)
                {
                    if (!typeof(IEventHandler).GetTypeInfo().IsAssignableFrom(@interface))
                    {
                        continue;
                    }

                    var genericArgs = @interface.GetGenericArguments();
                    if (genericArgs.Length == 1)
                    {
                        _eventBus.Register(genericArgs[0], new IocHandlerFactory(_iocResolver, implementationType));
                    }
                }
            };
        }
    }
}
