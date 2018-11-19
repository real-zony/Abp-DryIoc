using System;
using System.Linq;
using System.Reflection;

namespace Abp.Dependency
{
    public static class IocManagerExtensions
    {
        public static void Install(this IIocManager iocManager,IDryIocInstaller installer)
        {
            installer.Install(iocManager);
        }

        public static void Install(this IIocManager iocManager, Assembly assembly)
        {
            var installers = assembly.GetTypes().Where(type => type.GetInterfaces().Any(@interface => @interface == typeof(IDryIocInstaller)));

            foreach (var installer in installers)
            {
                (Activator.CreateInstance(installer) as IDryIocInstaller)?.Install(iocManager);
            }
        }
    }
}