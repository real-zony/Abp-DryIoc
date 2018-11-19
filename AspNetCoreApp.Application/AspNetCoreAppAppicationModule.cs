using Abp.Modules;
using AspNetCoreApp.Core;

namespace AspNetCoreApp.Application
{
    [DependsOn(typeof(AspNetCoreAppCoreModule))]
    public class AspNetCoreAppApplicationModule : AbpModule
    {
        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(AspNetCoreAppApplicationModule).Assembly);
        }
    }
}