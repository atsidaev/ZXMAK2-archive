using System;
using System.Collections.Generic;
using System.Text;
using System.Xml;

namespace ZXMAK2
{
    public class Locator
    {
        private static LocatorResolver _instance = new LocatorResolver();

        public static T Resolve<T>()
        {
            return _instance.Resolve<T>();
        }

        private class LocatorResolver
        {
            private Dictionary<Type, LocatorEntry> m_entries = new Dictionary<Type, LocatorEntry>();
            private Dictionary<Type, object> m_instances = new Dictionary<Type, object>();

            public LocatorResolver()
            {
                Load();
            }

            public T Resolve<T>()
            {
                var entry = GetEntry<T>();
                if (entry == null)
                {
                    return default(T);
                }
                switch (entry.LifeTime)
                {
                    case LocatorLifeTime.Singleton: return GetSingleton<T>(entry);
                    case LocatorLifeTime.Transient: return GetTransient<T>(entry);
                }
                throw new ArgumentException();
            }

            private T GetTransient<T>(LocatorEntry entry)
            {
                return (T)Activator.CreateInstance(entry.Type);
            }

            private T GetSingleton<T>(LocatorEntry entry)
            {
                lock (m_instances)
                {
                    var key = typeof(T);
                    if (!m_instances.ContainsKey(key))
                    {
                        m_instances[key] = GetTransient<T>(entry);
                    }
                    return (T)m_instances[key];
                }
            }

            private LocatorEntry GetEntry<T>()
            {
                lock (m_entries)
                {
                    var key = typeof(T);
                    if (!m_entries.ContainsKey(key))
                    {
                        return null;
                    }
                    return m_entries[key];
                }
            }

            private void Load()
            {
                var xml = new XmlDocument();
                xml.LoadXml(global::ZXMAK2.Properties.Resources.Locator);
                var aliases = new Dictionary<string, string>();
                foreach (XmlNode node in xml.SelectNodes("/Locator/Alias[count(@Name)>0 and count(@Type)>0]"))
                {
                    var name = node.Attributes["Name"].Value;
                    var type = node.Attributes["Type"].Value;
                    aliases.Add(name, type);
                }
                foreach (XmlNode node in xml.SelectNodes("/Locator/Entry[count(@Type)>0 and count(@MapTo)>0]"))
                {
                    var strType = node.Attributes["Type"].Value;
                    var strMapTo = node.Attributes["MapTo"].Value;
                    var strLifeTime = node.Attributes["LifeTime"] != null ?
                        node.Attributes["LifeTime"].Value :
                        "Transient";
                    if (aliases.ContainsKey(strType))
                    {
                        strType = aliases[strType];
                    }
                    if (aliases.ContainsKey(strMapTo))
                    {
                        strMapTo = aliases[strMapTo];
                    }
                    var typeType = Type.GetType(strType, true);
                    var typeMapTo = Type.GetType(strMapTo, true);
                    var lifeTime = (LocatorLifeTime)Enum.Parse(typeof(LocatorLifeTime), strLifeTime);
                    if (typeType == null || typeMapTo == null)
                    {
                        continue;
                    }
                    m_entries.Add(typeType, new LocatorEntry(typeMapTo, lifeTime));
                }
            }
        }

        private class LocatorEntry
        {
            public Type Type { get; private set; }
            public LocatorLifeTime LifeTime { get; private set; }

            public LocatorEntry(Type type, LocatorLifeTime lifeTime)
            {
                Type = type;
                LifeTime = lifeTime;
            }
        }

        private enum LocatorLifeTime
        {
            Singleton,
            Transient,
        }
    }
}
