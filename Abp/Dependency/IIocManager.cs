using System;
using DryIoc;

namespace Abp.Dependency
{
    /// <summary>
    /// This interface is used to directly perform dependency injection tasks.
    /// </summary>
    public interface IIocManager : IIocRegistrar, IIocResolver, IDisposable
    {
        /// <summary>
        /// Reference to the Castle Windsor Container.
        /// </summary>
        IContainer IocContainer { get; }

        /// <summary>
        /// Checks whether given type is registered before.
        /// </summary>
        /// <param name="type">Type to check</param>
        new bool IsRegistered(Type type);

        /// <summary>
        /// Checks whether given type is registered before.
        /// </summary>
        /// <typeparam name="T">Type to check</typeparam>
        new bool IsRegistered<T>();
        
        /// <summary>
        /// 类型注册事件
        /// </summary>
        event RegisterTypeEventHandler RegisterTypeEventHandler;

        /// <summary>
        /// 初始化 IocManager 内部的容器
        /// </summary>
        void InitializeInternalContainer(IContainer dryIocContainer);
    }
}