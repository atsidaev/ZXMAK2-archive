using System;
using ZXMAK2.Dependency;


namespace ZXMAK2.Interfaces
{
    public interface IViewResolver
    {
        T Resolve<T>(params Argument[] arguments);
    }
}
