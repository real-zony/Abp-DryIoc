using System;
using System.Linq;
using Castle.DynamicProxy;
using DryIoc;
using ImTools;

namespace Abp.Dependency
{
    public static class DryIocInterception
    {
        static readonly DefaultProxyBuilder ProxyBuilder = new DefaultProxyBuilder();

        public static void Intercept(this IRegistrator registrator,Type serviceType,Type interceptorType, object serviceKey = null)
        {
            Type proxyType;
            if (serviceType.IsInterface())
                proxyType = ProxyBuilder.CreateInterfaceProxyTypeWithTargetInterface(
                    serviceType, ArrayTools.Empty<Type>(), ProxyGenerationOptions.Default);
            else if (serviceType.IsClass())
                proxyType = ProxyBuilder.CreateClassProxyTypeWithTarget(
                    serviceType, ArrayTools.Empty<Type>(), ProxyGenerationOptions.Default);
            else
                throw new ArgumentException(
                    $"Intercepted service type {serviceType} is not a supported, cause it is nor a class nor an interface");

            var decoratorSetup = serviceKey == null
                ? Setup.DecoratorWith(useDecorateeReuse: true)
                : Setup.DecoratorWith(r => serviceKey.Equals(r.ServiceKey), useDecorateeReuse: true);

            registrator.Register(serviceType, proxyType,
                made: Made.Of((Type type) => type.GetConstructors().SingleOrDefault(c => c.GetParameters().Length != 0), 
                    Parameters.Of.Type<IInterceptor[]>(interceptorType.MakeArrayType()),
                    PropertiesAndFields.Auto),
                setup: decoratorSetup);
        }
    
        public static void Intercept<TService, TInterceptor>(this IRegistrator registrator, object serviceKey = null) 
            where TInterceptor : class, IInterceptor
        {
            Intercept(registrator,typeof(TService),typeof(TInterceptor),serviceKey);
        }
    }
}