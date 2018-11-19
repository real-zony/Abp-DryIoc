using Abp.AspNetCore;
using Abp.AspNetCore.Configuration;
using Abp.Configuration.Startup;
using Abp.Modules;
using AspNetCoreApp.Application;
using AspNetCoreApp.EntityFrameworkCore;

namespace AspNetCoreApp
{
    [DependsOn(typeof(AspNetCoreAppApplicationModule),
        typeof(AspNetCoreAppEntityFrameworkCore),
        typeof(AbpAspNetCoreModule))]
    public class AspNetCoreAppModule : AbpModule
    {
        public override void PreInitialize()
        {
            Configuration.Modules.AbpAspNetCore().CreateControllersForAppServices(typeof(AspNetCoreAppApplicationModule).Assembly);
        }
    }
}