using System;
using System.Linq;
using System.Reflection;
using Abp.Application.Features;
using Abp.Dependency;
using Castle.Core;
using Castle.MicroKernel;

namespace Abp.Authorization
{
    /// <summary>
    /// This class is used to register interceptors on the Application Layer.
    /// </summary>
    internal static class AuthorizationInterceptorRegistrar
    {
        public static void Initialize(IIocManager iocManager)
        {
            iocManager.RegisterTypeEventHandler += (manager, type, implementationType) =>
            {
                if (ShouldIntercept(implementationType))
                {
                    iocManager.AddInterceptor(type,typeof(AuthorizationInterceptor));
                }
            };
        }

        private static bool ShouldIntercept(Type type)
        {
            if (SelfOrMethodsDefinesAttribute<AbpAuthorizeAttribute>(type))
            {
                return true;
            }

            if (SelfOrMethodsDefinesAttribute<RequiresFeatureAttribute>(type))
            {
                return true;
            }

            return false;
        }

        private static bool SelfOrMethodsDefinesAttribute<TAttr>(Type type)
        {
            if (type.GetTypeInfo().IsDefined(typeof(TAttr), true))
            {
                return true;
            }

            return type
                .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                .Any(m => m.IsDefined(typeof(TAttr), true));
        }
    }
}