using System;
using System.Collections.Generic;
using System.Text;

namespace ZXMAK2.Dependency
{
    public interface IResolver
    {
        T Resolve<T>(params Argument[] args);
        T Resolve<T>(string name, params Argument[] args);
        T TryResolve<T>(params Argument[] args);
        T TryResolve<T>(string name, params Argument[] args);
        bool CheckAvailable<T>(params Argument[] args);
        bool CheckAvailable<T>(string name, params Argument[] args);

        void RegisterInstance<T>(string name, T instance);
    }
}
