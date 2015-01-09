using System;
using ZXMAK2.Dependency;


namespace ZXMAK2.Host.Presentation.Interfaces
{
    public interface IViewResolver
    {
        T Resolve<T>(params Argument[] arguments);
    }
}
