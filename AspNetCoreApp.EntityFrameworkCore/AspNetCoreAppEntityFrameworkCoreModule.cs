using Abp.Modules;
using AspNetCoreApp.Core;

namespace AspNetCoreApp.EntityFrameworkCore
{
    [DependsOn(typeof(AspNetCoreAppCoreModule))]
    public class AspNetCoreAppEntityFrameworkCore : AbpModule 
    {
        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(AspNetCoreAppEntityFrameworkCore).Assembly);
        }
    }
}