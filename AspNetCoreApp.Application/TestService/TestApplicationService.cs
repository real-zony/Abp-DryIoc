using Abp.Application.Services;
using Abp.Dependency;
using Abp.Domain.Uow;

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
    }
}