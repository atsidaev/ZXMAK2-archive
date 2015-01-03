using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;
using System.Reflection;

namespace ZXMAK2.Dependency
{
    public class Resolver : IResolver
    {
        private readonly XmlDocument m_config = new XmlDocument();
        private Dictionary<XmlNode, object> m_instances = new Dictionary<XmlNode, object>();
        private Dictionary<string, string> m_aliases = new Dictionary<string, string>();

        
        #region IResolver

        public T Resolve<T>(params Argument[] args)
        {
            return Resolve<T>(null, args);
        }

        public T Resolve<T>(string name, params Argument[] args)
        {
            return (T)Resolve(typeof(T).AssemblyQualifiedName, name, args);
        }

        public T TryResolve<T>(params Argument[] args)
        {
            return TryResolve<T>(null, args);
        }

        public T TryResolve<T>(string name, params Argument[] args)
        {
            try
            {
                return (T)Resolve(typeof(T).AssemblyQualifiedName, name, args);
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                return default(T);
            }
        }

        #endregion IResolver


        public void Load(string fileName)
        {
            m_aliases.Clear();
            m_instances.Clear();
            m_config.Load(fileName);
            foreach (XmlNode node in m_config.SelectNodes("/Resolver/Alias[count(@Name)>0 and count(@Type)>0]"))
            {
                var name = node.Attributes["Name"].Value;
                var type = node.Attributes["Type"].Value;
                m_aliases.Add(name, type);
            }
            RegisterInstance<IResolver>(this);
        }

        public void RegisterInstance<T>(T instance)
        {
            RegisterInstance<T>(instance, null);
        }

        public void RegisterInstance<T>(T instance, string name)
        {
            var keyType = typeof(T).AssemblyQualifiedName;
            var keyMapTo = instance.GetType().AssemblyQualifiedName;
            var lifeTime = LocatorLifeTime.Singleton;
            var entry = GetEntry(keyType, name);
            if (entry != null)
            {
                m_config.RemoveChild(entry);
            }
            var el = m_config.CreateElement("Entry");
            el.SetAttribute("Type", typeof(T).AssemblyQualifiedName);
            el.SetAttribute("MapTo", instance.GetType().AssemblyQualifiedName);
            el.SetAttribute("LifeTime", lifeTime.ToString());
            entry = m_config["Resolver"].AppendChild(el);
            m_instances[entry] = instance;
        }

        private object Resolve(string type, string name, params Argument[] args)
        {
            var entry = GetEntry(type, name);
            if (entry == null)
            {
                return null;
            }
            var strLife = entry.Attributes["LifeTime"] != null ?
                entry.Attributes["LifeTime"].Value : "Transient";
            var lifeTime = (LocatorLifeTime)Enum.Parse(typeof(LocatorLifeTime), strLife);
            switch (lifeTime)
            {
                case LocatorLifeTime.Singleton: return GetSingleton(entry, args);
                case LocatorLifeTime.Transient: return GetTransient(entry, args);
            }
            return null;
        }

        private XmlNode GetEntry(string keyType, string name)
        {
            keyType = keyType ?? string.Empty;
            keyType = keyType.Trim();
            if (keyType == string.Empty)
            {
                return null;
            }
            var keyName = name ?? string.Empty;
            foreach (XmlNode entry in m_config.SelectNodes("/Resolver/Entry[count(@Type)>0 and count(@MapTo)>0]"))
            {
                var strType = entry.Attributes["Type"].Value;
                var strName = entry.Attributes["Name"] != null ?
                    entry.Attributes["Name"].Value : string.Empty;
                if (m_aliases.ContainsKey(strType))
                {
                    strType = m_aliases[strType];
                }
                if (keyType.StartsWith(strType) && strName == keyName)
                {
                    return entry;
                }
            }
            return null;
        }

        private object GetTransient(XmlNode entry, Argument[] arguments)
        {
            var args = GetArguments(entry.SelectNodes("Argument[count(@Name)>0 and count(@Value)>0]"));
            foreach (var arg in arguments)
            {
                args[arg.Name] = arg.Value;
            }
            var mapToKey = entry.Attributes["MapTo"].Value;
            if (m_aliases.ContainsKey(mapToKey))
            {
                mapToKey = m_aliases[mapToKey];
            }
            var mapToType = Type.GetType(mapToKey, true);

            var ctor = FindConstructor(mapToType, args);
            if (ctor == null)
            {
                throw new Exception(string.Format(
                    "Constructor not found for {0}",
                    mapToKey));
            }
            var objArgs = new List<object>();
            foreach (var arg in ctor.GetParameters())
            {
                if (args.ContainsKey(arg.Name))
                {
                    objArgs.Add(args[arg.Name]);
                }
                else
                {
                    objArgs.Add(
                        Resolve(
                            arg.ParameterType.AssemblyQualifiedName,
                            null));
                }
            }
            return ctor.Invoke(objArgs.ToArray());
        }

        private object GetSingleton(XmlNode entry, Argument[] arguments)
        {
            lock (m_instances)
            {
                if (!m_instances.ContainsKey(entry))
                {
                    m_instances[entry] = GetTransient(entry, arguments);
                }
                return m_instances[entry];
            }
        }

        private ConstructorInfo FindConstructor(Type type, Dictionary<string, object> args)
        {
            foreach (var ctorInfo in type.GetConstructors(BindingFlags.Public | BindingFlags.Instance))
            {
                var ctorArgs = new List<string>();
                foreach (var ctorArg in ctorInfo.GetParameters())
                {
                    var key = ctorArg.Name;
                    ctorArgs.Add(key);
                }
                var satisfy = true;
                foreach (var name in args.Keys)
                {
                    if (!ctorArgs.Contains(name))
                    {
                        satisfy = false;
                        break;
                    }
                }
                if (satisfy)
                {
                    return ctorInfo;
                }
            }
            return null;
        }

        private Dictionary<string, object> GetArguments(XmlNodeList nodeCollection)
        {
            var list = new Dictionary<string, object>();
            foreach (XmlNode argNode in nodeCollection)
            {
                var name = argNode.Attributes["Name"].Value;
                var value = argNode.Attributes["Value"] != null ? argNode.Attributes["Value"].Value :
                    (string)null;
                list.Add(name, value);
            }
            return list;
        }

        private enum LocatorLifeTime
        {
            Singleton,
            Transient,
        }
    }
}
