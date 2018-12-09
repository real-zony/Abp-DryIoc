using Abp.Domain.Uow;
using Abp.EntityFrameworkCore;
using Abp.Modules;
using Abp.MultiTenancy;
using DryIoc;

namespace Abp.Zero.EntityFrameworkCore
{
    /// <summary>
    /// Entity framework integration module for ASP.NET Boilerplate Zero.
    /// </summary>
    [DependsOn(typeof(AbpZeroCoreModule), typeof(AbpEntityFrameworkCoreModule))]
    public class AbpZeroCoreEntityFrameworkCoreModule : AbpModule
    {
        public override void PreInitialize()
        {
            Configuration.ReplaceService(typeof(IConnectionStringResolver),
                () =>
                {
                    IocManager.IocContainer.RegisterMany(new[]
                        {
                            typeof(IConnectionStringResolver),
                            typeof(IDbPerTenantConnectionStringResolver)
                        },
                        typeof(DbPerTenantConnectionStringResolver),
                        Reuse.Transient);
                });
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(AbpZeroCoreEntityFrameworkCoreModule).Assembly);
        }
    }
}
