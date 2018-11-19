using Abp;
using Abp.Modules;

namespace AspNetCoreApp.Core
{
    [DependsOn(typeof(AbpKernelModule))]
    public class AspNetCoreAppCoreModule : AbpModule
    {
        
    }
}