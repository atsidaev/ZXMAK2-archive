using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Reflection;
using ZXMAK2.Dependency;


namespace ZXMAK2
{
    public class Locator
    {
        public static IResolver Instance { get; set; }


        public static T Resolve<T>()
        {
            return Instance.Resolve<T>();
        }

        public static T Resolve<T>(string name)
        {
            return Instance.Resolve<T>(name);
        }

        public static T TryResolve<T>()
        {
            return Instance.TryResolve<T>();
        }

        public static T TryResolve<T>(string name)
        {
            return Instance.TryResolve<T>(name);
        }
    }
}
