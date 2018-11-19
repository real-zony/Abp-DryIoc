using System;
using System.Linq;
using System.Reflection;
using Abp.Dependency;

namespace Abp.Domain.Uow
{
    /// <summary>
    /// This class is used to register interceptor for needed classes for Unit Of Work mechanism.
    /// </summary>
    internal static class UnitOfWorkRegistrar
    {
        /// <summary>
        /// Initializes the registerer.
        /// </summary>
        /// <param name="iocManager">IOC manager</param>
        public static void Initialize(IIocManager iocManager)
        {
            iocManager.RegisterTypeEventHandler += (manager, type, implementationType) =>
            {
                HandleTypesWithUnitOfWorkAttribute(iocManager,type,implementationType.GetTypeInfo());
                HandleConventionalUnitOfWorkTypes(iocManager,type, implementationType.GetTypeInfo());
            };
        }

        private static void HandleTypesWithUnitOfWorkAttribute(IIocManager iocManager,Type serviceType,TypeInfo implementationType)
        {
            if (IsUnitOfWorkType(implementationType) || AnyMethodHasUnitOfWork(implementationType))
            {
                iocManager.IocContainer.Intercept(serviceType,typeof(UnitOfWorkInterceptor));
            }
        }

        private static void HandleConventionalUnitOfWorkTypes(IIocManager iocManager,Type serviceType,TypeInfo implementationType)
        {
            if (!iocManager.IsRegistered<IUnitOfWorkDefaultOptions>())
            {
                return;
            }

            var uowOptions = iocManager.Resolve<IUnitOfWorkDefaultOptions>();

            if (uowOptions.IsConventionalUowClass(implementationType.AsType()))
            {
                iocManager.IocContainer.Intercept(serviceType,typeof(UnitOfWorkInterceptor));
            }
        }

        private static bool IsUnitOfWorkType(TypeInfo implementationType)
        {
            return UnitOfWorkHelper.HasUnitOfWorkAttribute(implementationType);
        }

        private static bool AnyMethodHasUnitOfWork(TypeInfo implementationType)
        {
            return implementationType
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Any(UnitOfWorkHelper.HasUnitOfWorkAttribute);
        }
    }
}