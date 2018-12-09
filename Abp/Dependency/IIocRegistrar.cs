using System;
using System.Reflection;
using Castle.DynamicProxy;

namespace Abp.Dependency
{
    /// <summary>
    /// Define interface for classes those are used to register dependencies.
    /// </summary>
    public interface IIocRegistrar
    {
        /// <summary>
        /// Adds a dependency registrar for conventional registration.
        /// </summary>
        /// <param name="registrar">dependency registrar</param>
        void AddConventionalRegistrar(IConventionalDependencyRegistrar registrar);

        /// <summary>
        /// Registers types of given assembly by all conventional registrars. See <see cref="IocManager.AddConventionalRegistrar"/> method.
        /// </summary>
        /// <param name="assembly">Assembly to register</param>
        void RegisterAssemblyByConvention(Assembly assembly);

        /// <summary>
        /// Registers types of given assembly by all conventional registrars. See <see cref="IocManager.AddConventionalRegistrar"/> method.
        /// </summary>
        /// <param name="assembly">Assembly to register</param>
        /// <param name="config">Additional configuration</param>
        void RegisterAssemblyByConvention(Assembly assembly, ConventionalRegistrationConfig config);

        /// <summary>
        /// 为指定的类型添加拦截器
        /// </summary>
        /// <typeparam name="TService">注册类型</typeparam>
        /// <typeparam name="TInterceptor">拦截器类型</typeparam>
        void AddInterceptor<TService, TInterceptor>() where TInterceptor : IInterceptor;
        
        /// <summary>
        /// 为指定的类型添加拦截器
        /// </summary>
        /// <param name="serviceType">注册类型</param>
        /// <param name="interceptorType">拦截器类型</param>
        void AddInterceptor(Type serviceType,Type interceptorType);

        /// <summary>
        /// Registers a type as self registration.
        /// </summary>
        /// <typeparam name="T">Type of the class</typeparam>
        /// <param name="lifeStyle">Lifestyle of the objects of this type</param>
        void Register<T>(DependencyLifeStyle lifeStyle = DependencyLifeStyle.Singleton,bool isAutoInjectProperty = false)
            where T : class;

        /// <summary>
        /// Registers a type as self registration.
        /// </summary>
        /// <param name="type">Type of the class</param>
        /// <param name="lifeStyle">Lifestyle of the objects of this type</param>
        void Register(Type type, DependencyLifeStyle lifeStyle = DependencyLifeStyle.Singleton,bool isAutoInjectProperty = false);

        /// <summary>
        /// Registers a type with it's implementation.
        /// </summary>
        /// <typeparam name="TType">Registering type</typeparam>
        /// <typeparam name="TImpl">The type that implements <see cref="TType"/></typeparam>
        /// <param name="lifeStyle">Lifestyle of the objects of this type</param>
        void Register<TType, TImpl>(DependencyLifeStyle lifeStyle = DependencyLifeStyle.Singleton,bool isAutoInjectProperty = false)
            where TType : class
            where TImpl : class, TType;

        /// <summary>
        /// Registers a type with it's implementation.
        /// </summary>
        /// <param name="type">Type of the class</param>
        /// <param name="impl">The type that implements <paramref name="type"/></param>
        /// <param name="lifeStyle">Lifestyle of the objects of this type</param>
        void Register(Type type, Type impl, DependencyLifeStyle lifeStyle = DependencyLifeStyle.Singleton,bool isAutoInjectProperty = false);

        /// <summary>
        /// Checks whether given type is registered before.
        /// </summary>
        /// <param name="type">Type to check</param>
        bool IsRegistered(Type type);

        /// <summary>
        /// Checks whether given type is registered before.
        /// </summary>
        /// <typeparam name="TType">Type to check</typeparam>
        bool IsRegistered<TType>();
    }
}