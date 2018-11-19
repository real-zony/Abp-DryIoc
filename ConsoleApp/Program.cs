using System;
using Abp;
using Abp.Modules;
using Abp.Runtime.Session;

namespace ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            // Abp 框架测试
            using (var bootstarp = AbpBootstrapper.Create<StartupModule>())
            {
                bootstarp.Initialize();
                
                // 解析 IAbpSession 看是否正常地进行了注入
                var session = bootstarp.IocManager.Resolve<IAbpSession>();

                if (session != null && session is ClaimsAbpSession claimsSession)
                {
                    Console.WriteLine("当前 Session 已经成功被注入为 ClaimAbpSession");
                }
            }

            Console.ReadLine();
        }
    }

    [DependsOn(typeof(AbpKernelModule))]
    public class StartupModule : AbpModule
    {
        
    }
}