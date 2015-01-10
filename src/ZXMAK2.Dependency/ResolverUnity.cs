﻿using System;
using System.Linq;
using System.Collections.Generic;
using System.Configuration;
using Microsoft.Practices.Unity;
using Microsoft.Practices.Unity.Configuration;


namespace ZXMAK2.Dependency
{
    public class ResolverUnity : IResolver
    {
        private readonly IUnityContainer _container;


        public ResolverUnity()
        {
            _container = new UnityContainer();
            _container.LoadConfiguration();
            _container.RegisterInstance<IResolver>(this);
        }

        public ResolverUnity(string containerName)
        {
            _container = new UnityContainer();
            _container.LoadConfiguration(containerName);
            _container.RegisterInstance<IResolver>(this);
        }

        public T Resolve<T>(params Argument[] args)
        {
            if (args.Length > 0)
            {
                var poArgs = args.Select(arg => new ParameterOverride(arg.Name, arg.Value));
                return _container.Resolve<T>(poArgs.ToArray());
            }
            return _container.Resolve<T>();
        }

        public T Resolve<T>(string name, params Argument[] args)
        {
            if (args.Length > 0)
            {
                var poArgs = args.Select(arg => new ParameterOverride(arg.Name, arg.Value));
                return _container.Resolve<T>(name, poArgs.ToArray());
            }
            return _container.Resolve<T>(name);
        }

        public T TryResolve<T>(params Argument[] args)
        {
            try
            {
                if (!_container.IsRegistered<T>())
                {
                    return default(T);
                }
                return Resolve<T>(args);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return default(T);
            }
        }

        public void RegisterInstance<T>(string name, T instance)
        {
            _container.RegisterInstance<T>(name, instance);
        }

        public T TryResolve<T>(string name, params Argument[] args)
        {
            try
            {
                if (!_container.IsRegistered<T>(name))
                {
                    return default(T);
                }
                return Resolve<T>(name, args);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return default(T);
            }
        }
    }
}
