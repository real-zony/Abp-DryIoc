using System;

namespace Abp.Dependency
{
    public delegate void RegisterTypeEventHandler(IIocManager iocManager, Type registerType,Type implementationType);
}