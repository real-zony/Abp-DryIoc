using System.Reflection;
using Abp.Collections.Extensions;
using Abp.Dependency;
using Abp.EntityFramework;
using Abp.EntityFramework.Repositories;
using Abp.EntityFrameworkCore.Configuration;
using Abp.EntityFrameworkCore.Repositories;
using Abp.EntityFrameworkCore.Uow;
using Abp.Modules;
using Abp.Orm;
using Abp.Reflection;
using DryIoc;

namespace Abp.EntityFrameworkCore
{
    /// <summary>
    /// This module is used to implement "Data Access Layer" in EntityFramework.
    /// </summary>
    [DependsOn(typeof(AbpEntityFrameworkCommonModule))]
    public class AbpEntityFrameworkCoreModule : AbpModule
    {
        private readonly ITypeFinder _typeFinder;

        public AbpEntityFrameworkCoreModule(ITypeFinder typeFinder)
        {
            _typeFinder = typeFinder;
        }

        public override void PreInitialize()
        {
            IocManager.Register<IAbpEfCoreConfiguration, AbpEfCoreConfiguration>();
        }

        public override void Initialize()
        {
            IocManager.RegisterAssemblyByConvention(typeof(AbpEntityFrameworkCoreModule).Assembly);
            IocManager.IocContainer.Register(typeof(IDbContextProvider<>),typeof(UnitOfWorkDbContextProvider<>),Reuse.Transient);

            RegisterGenericRepositoriesAndMatchDbContexes();
        }

        private void RegisterGenericRepositoriesAndMatchDbContexes()
        {
            var dbContextTypes =
                _typeFinder.Find(type =>
                {
                    var typeInfo = type.GetTypeInfo();
                    return typeInfo.IsPublic &&
                           !typeInfo.IsAbstract &&
                           typeInfo.IsClass &&
                           typeof(AbpDbContext).IsAssignableFrom(type);
                });

            if (dbContextTypes.IsNullOrEmpty())
            {
                Logger.Warn("No class found derived from AbpDbContext.");
                return;
            }

            using (IScopedIocResolver scope = IocManager.CreateScope())
            {
                foreach (var dbContextType in dbContextTypes)
                {
                    Logger.Debug("Registering DbContext: " + dbContextType.AssemblyQualifiedName);

                    scope.Resolve<IEfGenericRepositoryRegistrar>().RegisterForDbContext(dbContextType, IocManager, EfCoreAutoRepositoryTypes.Default);

//                    IocManager.IocContainer.Register(
//                        Component.For<ISecondaryOrmRegistrar>()
//                            .Named(Guid.NewGuid().ToString("N"))
//                            .Instance(new EfCoreBasedSecondaryOrmRegistrar(dbContextType, scope.Resolve<IDbContextEntityFinder>()))
//                            .LifestyleTransient()
//                    );
                    IocManager.IocContainer.UseInstance<ISecondaryOrmRegistrar>(new EfCoreBasedSecondaryOrmRegistrar(dbContextType,
                        scope.Resolve<IDbContextEntityFinder>()));
                }

                scope.Resolve<IDbContextTypeMatcher>().Populate(dbContextTypes);
            }
        }
    }
}
