using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using Castle.MicroKernel;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using Castle.Windsor.Installer;
using Castle.Windsor.Proxy;
using DryIoc;
using ImTools;

namespace Abp.Dependency
{
    /// <summary>
    /// This class is used to directly perform dependency injection tasks.
    /// </summary>
    public class IocManager : IIocManager
    {
        /// <summary>
        /// The Singleton instance.
        /// </summary>
        public static IocManager Instance { get; private set; }

        /// <summary>
        /// Singletone instance for Castle ProxyGenerator.
        /// From Castle.Core documentation it is highly recomended to use single instance of ProxyGenerator to avoid memoryleaks and performance issues
        /// Follow next links for more details:
        /// <a href="https://github.com/castleproject/Core/blob/master/docs/dynamicproxy.md">Castle.Core documentation</a>,
        /// <a href="http://kozmic.net/2009/07/05/castle-dynamic-proxy-tutorial-part-xii-caching/">Article</a>
        /// </summary>
        private static readonly ProxyGenerator ProxyGeneratorInstance = new ProxyGenerator();

        /// <summary>
        /// Reference to the Castle Windsor Container.
        /// </summary>
        public IContainer IocContainer { get; private set; }

        /// <inheritdoc />
        public event RegisterTypeEventHandler RegisterTypeEventHandler;

        /// <summary>
        /// List of all registered conventional registrars.
        /// </summary>
        private readonly List<IConventionalDependencyRegistrar> _conventionalRegistrars;
        private readonly ConcurrentDictionary<Type, List<Type>> _waitRegisterInterceptor;

        static IocManager()
        {
            Instance = new IocManager();
        }

        /// <summary>
        /// Creates a new <see cref="IocManager"/> object.
        /// Normally, you don't directly instantiate an <see cref="IocManager"/>.
        /// This may be useful for test purposes.
        /// </summary>
        public IocManager()
        {
            _conventionalRegistrars = new List<IConventionalDependencyRegistrar>();
            _waitRegisterInterceptor = new ConcurrentDictionary<Type, List<Type>>();
        }

        /// <inheritdoc />
        public void InitializeInternalContainer(IContainer dryIocContainer)
        {
            IocContainer = dryIocContainer;
            
            //Register self!
            IocContainer.UseInstance(typeof(IocManager),this);
            IocContainer.UseInstance(typeof(IIocManager),this);
            IocContainer.UseInstance(typeof(IIocRegistrar),this);
            IocContainer.UseInstance(typeof(IIocResolver),this);
        }

        /// <inheritdoc />
        public IResolverContext ChildContainer { get; private set; }

        /// <inheritdoc />
        public void InitializeChildContainer(IResolverContext container)
        {
            ChildContainer = container;
        }

        /// <summary>
        /// Adds a dependency registrar for conventional registration.
        /// </summary>
        /// <param name="registrar">dependency registrar</param>
        public void AddConventionalRegistrar(IConventionalDependencyRegistrar registrar)
        {
            _conventionalRegistrars.Add(registrar);
        }

        /// <summary>
        /// Registers types of given assembly by all conventional registrars. See <see cref="AddConventionalRegistrar"/> method.
        /// </summary>
        /// <param name="assembly">Assembly to register</param>
        public void RegisterAssemblyByConvention(Assembly assembly)
        {
            RegisterAssemblyByConvention(assembly, new ConventionalRegistrationConfig());
        }

        /// <summary>
        /// Registers types of given assembly by all conventional registrars. See <see cref="AddConventionalRegistrar"/> method.
        /// </summary>
        /// <param name="assembly">Assembly to register</param>
        /// <param name="config">Additional configuration</param>
        public void RegisterAssemblyByConvention(Assembly assembly, ConventionalRegistrationConfig config)
        {
            var context = new ConventionalRegistrationContext(assembly, this, config);

            foreach (var registerer in _conventionalRegistrars)
            {
                registerer.RegisterAssembly(context);
            }

            if (config.InstallInstallers)
            {
                this.Install(assembly);
            }

            // 这里使用 TPL 并行库的原因是因为存在大量仓储类型与应用服务需要注册，应最大限度利用 CPU 来进行操作
            Parallel.ForEach(_waitRegisterInterceptor, keyValue =>
            {
                var proxyBuilder = new DefaultProxyBuilder();

                Type proxyType;
                if (keyValue.Key.IsInterface)
                    proxyType = proxyBuilder.CreateInterfaceProxyTypeWithTargetInterface(keyValue.Key, ArrayTools.Empty<Type>(), ProxyGenerationOptions.Default);
                else if (keyValue.Key.IsClass())
                    proxyType = proxyBuilder.CreateClassProxyTypeWithTarget(keyValue.Key,ArrayTools.Empty<Type>(),ProxyGenerationOptions.Default);
                else
                    throw new ArgumentException($"类型 {keyValue.Value} 不支持进行拦截器服务集成。");

                var decoratorSetup = Setup.DecoratorWith(useDecorateeReuse: true);
                
                // 使用 ProxyBuilder 创建好的代理类替换原有类型的实现
                IocContainer.Register(keyValue.Key,proxyType,
                    made: Made.Of(type=>type.GetConstructors().SingleOrDefault(c=>c.GetParameters().Length != 0),
                        Parameters.Of.Type<IInterceptor[]>(request =>
                        {
                            var objects = new List<object>();
                            foreach (var interceptor in keyValue.Value)
                            {
                                objects.Add(request.Container.Resolve(interceptor));
                            }

                            return objects.Cast<IInterceptor>().ToArray();
                        }),
                        PropertiesAndFields.Auto),
                    setup: decoratorSetup);
            });
            
            _waitRegisterInterceptor.Clear();
        }


        /// <inheritdoc />
        public void AddInterceptor<TService, TInterceptor>() where TInterceptor : IInterceptor
        {
            AddInterceptor(typeof(TService),typeof(TInterceptor));
        }

        /// <inheritdoc />
        public void AddInterceptor(Type serviceType, Type interceptorType)
        {
            if (_waitRegisterInterceptor.ContainsKey(serviceType))
            {
                var interceptors = _waitRegisterInterceptor[serviceType];
                if (interceptors.Contains(interceptorType)) return;
                
                _waitRegisterInterceptor[serviceType].Add(interceptorType);
            }
            else
            {
                _waitRegisterInterceptor.TryAdd(serviceType, new List<Type> {interceptorType});
            }
        }

        /// <summary>
        /// Registers a type as self registration.
        /// </summary>
        /// <typeparam name="TType">Type of the class</typeparam>
        /// <param name="lifeStyle">Lifestyle of the objects of this type</param>
        public void Register<TType>(DependencyLifeStyle lifeStyle = DependencyLifeStyle.Singleton,bool isAutoInjectProperty = false) where TType : class
        {
            Register(typeof(TType),lifeStyle,isAutoInjectProperty);
        }

        /// <summary>
        /// Registers a type as self registration.
        /// </summary>
        /// <param name="type">Type of the class</param>
        /// <param name="lifeStyle">Lifestyle of the objects of this type</param>
        public void Register(Type type, DependencyLifeStyle lifeStyle = DependencyLifeStyle.Singleton,bool isAutoInjectProperty = false)
        {
            IocContainer.Register(type,
                ApplyLifestyle(lifeStyle),
                made: Made.Of(FactoryMethod.ConstructorWithResolvableArguments,
                    propertiesAndFields: isAutoInjectProperty
                        ? PropertiesAndFields.Auto
                        : null));
            RegisterTypeEventHandler?.Invoke(this,
                type,
                type);
        }

        /// <summary>
        /// Registers a type with it's implementation.
        /// </summary>
        /// <typeparam name="TType">Registering type</typeparam>
        /// <typeparam name="TImpl">The type that implements <see cref="TType"/></typeparam>
        /// <param name="lifeStyle">Lifestyle of the objects of this type</param>
        public void Register<TType, TImpl>(DependencyLifeStyle lifeStyle = DependencyLifeStyle.Singleton,bool isAutoInjectProperty = false)
            where TType : class
            where TImpl : class, TType
        {
            Register(typeof(TType),typeof(TImpl),lifeStyle);
        }

        /// <summary>
        /// Registers a type with it's implementation.
        /// </summary>
        /// <param name="type">Type of the class</param>
        /// <param name="impl">The type that implements <paramref name="type"/></param>
        /// <param name="lifeStyle">Lifestyle of the objects of this type</param>
        public void Register(Type type, Type impl, DependencyLifeStyle lifeStyle = DependencyLifeStyle.Singleton,bool isAutoInjectProperty = false)
        {
            if (type == impl)
            {
                IocContainer.Register(type,
                    ApplyLifestyle(lifeStyle),
                    made: Made.Of(FactoryMethod.ConstructorWithResolvableArguments,
                        propertiesAndFields: isAutoInjectProperty
                            ? PropertiesAndFields.Auto
                            : null));
                RegisterTypeEventHandler?.Invoke(this,type,impl);
            }
            else
            {
                IocContainer.RegisterMany(new[]
                    {
                        type,
                        impl
                    },
                    impl,
                    made: Made.Of(FactoryMethod.ConstructorWithResolvableArguments,
                        propertiesAndFields: isAutoInjectProperty
                            ? PropertiesAndFields.Auto
                            : null),
                    reuse: ApplyLifestyle(lifeStyle));
                
                RegisterTypeEventHandler?.Invoke(this,type,impl);
                RegisterTypeEventHandler?.Invoke(this,impl,impl);
            }
        }

        /// <summary>
        /// Checks whether given type is registered before.
        /// </summary>
        /// <param name="type">Type to check</param>
        public bool IsRegistered(Type type)
        {
            return IocContainer.IsRegistered(type);
        }

        /// <summary>
        /// Checks whether given type is registered before.
        /// </summary>
        /// <typeparam name="TType">Type to check</typeparam>
        public bool IsRegistered<TType>()
        {
            return IsRegistered(typeof(TType));
        }


        /// <summary>
        /// Gets an object from IOC container.
        /// Returning object must be Released (see <see cref="IIocResolver.Release"/>) after usage.
        /// </summary> 
        /// <typeparam name="T">Type of the object to get</typeparam>
        /// <returns>The instance object</returns>
        public T Resolve<T>()
        {
            if (ChildContainer == null) return IocContainer.Resolve<T>();
            if (!ChildContainer.IsDisposed) return ChildContainer.Resolve<T>();

            return IocContainer.Resolve<T>();
        }

        /// <summary>
        /// Gets an object from IOC container.
        /// Returning object must be Released (see <see cref="Release"/>) after usage.
        /// </summary> 
        /// <typeparam name="T">Type of the object to cast</typeparam>
        /// <param name="type">Type of the object to resolve</param>
        /// <returns>The object instance</returns>
        public T Resolve<T>(Type type)
        {
            return (T)IocContainer.Resolve(type);
        }

        /// <summary>
        /// Gets an object from IOC container.
        /// Returning object must be Released (see <see cref="IIocResolver.Release"/>) after usage.
        /// </summary> 
        /// <typeparam name="T">Type of the object to get</typeparam>
        /// <param name="argumentsAsAnonymousType">Constructor arguments</param>
        /// <returns>The instance object</returns>
        public T Resolve<T>(object[] argumentsAsAnonymousType)
        {
            return IocContainer.Resolve<T>(argumentsAsAnonymousType);
        }

        /// <summary>
        /// Gets an object from IOC container.
        /// Returning object must be Released (see <see cref="IIocResolver.Release"/>) after usage.
        /// </summary> 
        /// <param name="type">Type of the object to get</param>
        /// <returns>The instance object</returns>
        public object Resolve(Type type)
        {
            return IocContainer.Resolve(type);
        }

        /// <summary>
        /// Gets an object from IOC container.
        /// Returning object must be Released (see <see cref="IIocResolver.Release"/>) after usage.
        /// </summary> 
        /// <param name="type">Type of the object to get</param>
        /// <param name="argumentsAsAnonymousType">Constructor arguments</param>
        /// <returns>The instance object</returns>
        public object Resolve(Type type, object[] argumentsAsAnonymousType)
        {
            return IocContainer.Resolve(type, argumentsAsAnonymousType);
        }

        ///<inheritdoc/>
        public T[] ResolveAll<T>()
        {
            return IocContainer.ResolveMany<T>().ToArray();
        }

        ///<inheritdoc/>
        public T[] ResolveAll<T>(object[] argumentsAsAnonymousType)
        {
            return IocContainer.ResolveMany<T>(args:argumentsAsAnonymousType).ToArray();
        }

        ///<inheritdoc/>
        public object[] ResolveAll(Type type)
        {
            return IocContainer.ResolveMany(type).ToArray();
        }

        ///<inheritdoc/>
        public object[] ResolveAll(Type type, object[] argumentsAsAnonymousType)
        {
            return IocContainer.ResolveMany(type, args:argumentsAsAnonymousType).ToArray();
        }

        /// <summary>
        /// Releases a pre-resolved object. See Resolve methods.
        /// </summary>
        /// <param name="obj">Object to be released</param>
        public void Release(object obj)
        {
            if(obj is IDisposable disposeObj)
            {
                disposeObj.Dispose();
            }
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            IocContainer.Dispose();
        }

        private static IReuse ApplyLifestyle(DependencyLifeStyle lifeStyle)
        {
            switch (lifeStyle)
            {
                case DependencyLifeStyle.Transient:
                    return Reuse.Transient;;
                case DependencyLifeStyle.Singleton:
                    return Reuse.Singleton;
                default:
                    return Reuse.Transient;
            }
        }
    }
}