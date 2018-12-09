using System;

namespace AspNetCoreApp.Application.ScopeTest
{
    public class ScopeClass
    {
        public string GuidValue;

        public ScopeClass()
        {
            GuidValue = Guid.NewGuid().ToString();
        }
    }
}