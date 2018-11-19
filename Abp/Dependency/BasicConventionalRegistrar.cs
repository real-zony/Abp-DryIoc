using System;
using System.Linq;
using System.Reflection;
using Abp.Application.Services;
using Abp.Extensions;
using Castle.DynamicProxy;

namespace Abp.Dependency
{
    // TODO: 此处类型应该抽离
    public class AssemblyType
    {
        public Type ServiceType { get; set; }

        public Type ImplType { get; set; }
    }
    
    /// <summary>
    /// 本类用于注册实现了 <see cref="ITransientDependency"/> 和 <see cref="ISingletonDependency"/> 接口的类型。
    /// </summary>
    public class BasicConventionalRegistrar : IConventionalDependencyRegistrar
    {
        public void RegisterAssembly(IConventionalRegistrationContext context)
        {
            // 瞬时对象注册
            var waitRegisterTransient = GetTypes<ITransientDependency>(context.Assembly).ToList();

            foreach (var transientType in waitRegisterTransient)
            {
                if (typeof(IApplicationService).IsAssignableFrom(transientType.ImplType))
                {
                    context.IocManager.Register(transientType.ServiceType,transientType.ImplType,DependencyLifeStyle.Transient,true);
                    continue;
                }
                
                context.IocManager.RegisterIfNot(transientType.ServiceType,transientType.ImplType,DependencyLifeStyle.Transient);
            }
            
            // 单例对象注册
            var waitRegisterSingleton = GetTypes<ISingletonDependency>(context.Assembly).ToList();

            foreach (var singletonType in waitRegisterSingleton)
            {
                context.IocManager.RegisterIfNot(singletonType.ServiceType,singletonType.ImplType,DependencyLifeStyle.Singleton);
            }
            
            // Castle.DynamicProxy 拦截器注册
            var waitRegisterInterceptor = GetTypes<IInterceptor>(context.Assembly).ToList();

            foreach (var interceptorType in waitRegisterInterceptor)
            {
                context.IocManager.RegisterIfNot(interceptorType.ServiceType,interceptorType.ImplType,DependencyLifeStyle.Transient);
            }
        }

        private ParallelQuery<AssemblyType> GetTypes<TInterface>(Assembly assembly)
        {
            Type GetServiceType(Type type)
            {
                var interfaces = type.GetInterfaces().Where(i => i != typeof(TInterface));

                // 优先匹配去除 I 之后的接口
                var defaultInterface = interfaces.FirstOrDefault(i => type.Name.Equals(i.Name.RemovePreFix("I")));
                if (defaultInterface != null) return defaultInterface;
                if (interfaces.FirstOrDefault() != null) return interfaces.FirstOrDefault();
                return type;
            }

            return assembly.GetTypes()
                .AsParallel()
                .Where(type => typeof(TInterface).IsAssignableFrom(type))
                .Where(type => type.GetInterfaces().Any() && !type.IsInterface)
                .Where(type => !type.IsGenericTypeDefinition)
                .Where(type => !type.IsAbstract)
                .Select(type => new AssemblyType
                {
                    ServiceType = GetServiceType(type),
                    ImplType = type
                });
        }
    }
}