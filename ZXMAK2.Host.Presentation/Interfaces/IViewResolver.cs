using System;
using ZXMAK2.Dependency;


namespace ZXMAK2.MVP.Interfaces
{
    public interface IViewResolver
    {
        T Resolve<T>(params Argument[] arguments);
    }
}
