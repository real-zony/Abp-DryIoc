using Abp.Application.Features;
using Abp.Auditing;
using Abp.BackgroundJobs;
using Abp.Configuration.Startup;
using Abp.Domain.Uow;
using Abp.EntityHistory;
using Abp.Localization;
using Abp.Modules;
using Abp.Notifications;
using Abp.PlugIns;
using Abp.Reflection;
using Abp.Resources.Embedded;
using Abp.Runtime.Caching.Configuration;
using DryIoc;

namespace Abp.Dependency.Installers
{
    /// <summary>
    /// ABP 框架核心类安装器
    /// 本类用于注册 ABP 框架当中核心组件
    /// </summary>
    internal class AbpCoreInstaller : IDryIocInstaller
    {
        public void Install(IIocManager iocManager)
        {
            iocManager.IocContainer.RegisterMany(new[] {typeof(IUnitOfWorkDefaultOptions), typeof(UnitOfWorkDefaultOptions)}, typeof(UnitOfWorkDefaultOptions), Reuse.Singleton);
            iocManager.IocContainer.RegisterMany(new[] {typeof(INavigationConfiguration), typeof(NavigationConfiguration)}, typeof(NavigationConfiguration), Reuse.Singleton);
            iocManager.IocContainer.RegisterMany(new[] {typeof(ILocalizationConfiguration), typeof(LocalizationConfiguration)}, typeof(LocalizationConfiguration), Reuse.Singleton);
            iocManager.IocContainer.RegisterMany(new[] {typeof(IAuthorizationConfiguration), typeof(AuthorizationConfiguration)}, typeof(AuthorizationConfiguration), Reuse.Singleton);
            iocManager.IocContainer.RegisterMany(new[] {typeof(IValidationConfiguration), typeof(ValidationConfiguration)}, typeof(ValidationConfiguration), Reuse.Singleton);
            iocManager.IocContainer.RegisterMany(new[] {typeof(IFeatureConfiguration), typeof(FeatureConfiguration)}, typeof(FeatureConfiguration), Reuse.Singleton);
            iocManager.IocContainer.RegisterMany(new[] {typeof(ISettingsConfiguration), typeof(SettingsConfiguration)}, typeof(SettingsConfiguration), Reuse.Singleton);
            iocManager.IocContainer.RegisterMany(new[] {typeof(IModuleConfigurations), typeof(ModuleConfigurations)}, typeof(ModuleConfigurations), Reuse.Singleton);
            iocManager.IocContainer.RegisterMany(new[] {typeof(IEventBusConfiguration), typeof(EventBusConfiguration)}, typeof(EventBusConfiguration), Reuse.Singleton);
            iocManager.IocContainer.RegisterMany(new[] {typeof(IMultiTenancyConfig), typeof(MultiTenancyConfig)}, typeof(MultiTenancyConfig), Reuse.Singleton);
            iocManager.IocContainer.RegisterMany(new[] {typeof(ICachingConfiguration), typeof(CachingConfiguration)}, typeof(CachingConfiguration), Reuse.Singleton);
            iocManager.IocContainer.RegisterMany(new[] {typeof(IAuditingConfiguration), typeof(AuditingConfiguration)}, typeof(AuditingConfiguration), Reuse.Singleton);
            iocManager.IocContainer.RegisterMany(new[] {typeof(IBackgroundJobConfiguration), typeof(BackgroundJobConfiguration)}, typeof(BackgroundJobConfiguration), Reuse.Singleton);
            iocManager.IocContainer.RegisterMany(new[] {typeof(INotificationConfiguration), typeof(NotificationConfiguration)}, typeof(NotificationConfiguration), Reuse.Singleton);
            iocManager.IocContainer.RegisterMany(new[] {typeof(IEmbeddedResourcesConfiguration), typeof(EmbeddedResourcesConfiguration)}, typeof(EmbeddedResourcesConfiguration), Reuse.Singleton);
            iocManager.IocContainer.RegisterMany(new[] {typeof(IAbpStartupConfiguration), typeof(AbpStartupConfiguration)}, typeof(AbpStartupConfiguration), Reuse.Singleton);
            iocManager.IocContainer.RegisterMany(new[] {typeof(IEntityHistoryConfiguration), typeof(EntityHistoryConfiguration)}, typeof(EntityHistoryConfiguration), Reuse.Singleton);
            iocManager.IocContainer.RegisterMany(new[] {typeof(ITypeFinder), typeof(TypeFinder)}, typeof(TypeFinder), Reuse.Singleton);
            iocManager.IocContainer.RegisterMany(new[] {typeof(IAbpPlugInManager), typeof(AbpPlugInManager)}, typeof(AbpPlugInManager), Reuse.Singleton);
            iocManager.IocContainer.RegisterMany(new[] {typeof(IAbpModuleManager), typeof(AbpModuleManager)}, typeof(AbpModuleManager), Reuse.Singleton);
            iocManager.IocContainer.RegisterMany(new[] {typeof(IAssemblyFinder), typeof(AbpAssemblyFinder)}, typeof(AbpAssemblyFinder), Reuse.Singleton);
            iocManager.IocContainer.RegisterMany(new[] {typeof(ILocalizationManager), typeof(LocalizationManager)}, typeof(LocalizationManager), Reuse.Singleton);
        }
    }
}