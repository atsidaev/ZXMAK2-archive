using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Reflection;
using ZXMAK2.Dependency;


namespace ZXMAK2.Dependency
{
    public class Locator
    {
        private readonly static IResolver _instance = new ResolverUnity();

        public static T Resolve<T>()
        {
            return _instance.Resolve<T>();
        }

        public static T Resolve<T>(string name)
        {
            return _instance.Resolve<T>(name);
        }

        public static T TryResolve<T>()
        {
            return _instance.TryResolve<T>();
        }

        public static T TryResolve<T>(string name)
        {
            return _instance.TryResolve<T>(name);
        }
    }
}
