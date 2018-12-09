using System;
using Abp.Application.Services;
using Abp.Dependency;
using Abp.Domain.Uow;
using AspNetCoreApp.Application.ScopeTest;

namespace AspNetCoreApp.Application.TestService
{
    [UnitOfWork]
    public class TestApplicationService : ApplicationService,ITestApplicationService
    {
        private readonly IIocManager _iocManager;
        
        public TestApplicationService(IIocManager iocManager)
        {
            _iocManager = iocManager;
        }

        [UnitOfWork]
        public virtual string GetJson()
        {
            var result = AbpSession?.UserId;
            return "OJBK";
        }

        public virtual string GetTest()
        {
            _iocManager.Resolve<ITestApplicationService>().GetJson();
            return "OJBK";
        }

        public string GetScopedObject()
        {
            var scopeObject = _iocManager.Resolve<ScopeClass>();
            Console.WriteLine(scopeObject.GuidValue);
            var service = _iocManager.Resolve<TestApplication2>();
            service.InterceptorTest();
            return "OJBK";
        }
    }

    public class TestApplication2 : ApplicationService
    {
        private readonly IIocManager _iocManager;

        public TestApplication2(IIocManager iocManager)
        {
            _iocManager = iocManager;
        }

        // 因为拦截器只能拦截虚方法与接口方法.
        public virtual string InterceptorTest()
        {
            // 因为在一个请求范围之内，所以这里 scopeObject 的 Guid 值应该与 GetScopedObject 当中解析的值一样。
            var scopeObject = _iocManager.Resolve<ScopeClass>();
            return "Ok";
        }
    }
}