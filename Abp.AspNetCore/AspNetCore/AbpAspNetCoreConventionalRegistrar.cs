using System.Linq;
using Abp.Dependency;
using Microsoft.AspNetCore.Mvc;

namespace Abp.AspNetCore
{
    public class AbpAspNetCoreConventionalRegistrar : IConventionalDependencyRegistrar
    {
        public void RegisterAssembly(IConventionalRegistrationContext context)
        {
            //ViewComponents
            var types = context.Assembly.GetTypes()
                .AsParallel()
                .Where(type => typeof(ViewComponent).IsAssignableFrom(type))
                .Where(type => !type.IsGenericTypeDefinition)
                .Where(type => !type.IsAbstract)
                .AsSequential();

            foreach (var type in types)
            {
                context.IocManager.Register(type);
            }
        }
    }
}
