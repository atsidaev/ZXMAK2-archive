using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using ZXMAK2.Interfaces;
using ZXMAK2.Entities;


namespace ZXMAK2.Engine
{
    public static class DeviceEnumerator
    {
        private static readonly object s_syncRoot = new object();
        private static IList<BusDeviceDescriptor> s_descriptors;

        public static IList<BusDeviceDescriptor> Descriptors
        {
            get
            {
                lock (s_syncRoot)
                {
                    if (s_descriptors == null)
                    {
                        Refresh();
                    }
                }
                return s_descriptors;
            }
        }
        
        public static void Refresh()
        {
            lock (s_syncRoot)
            {
                PreLoadPlugins();
                var listDescriptors = new List<BusDeviceDescriptor>();
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    try
                    {
                        foreach (var type in asm.GetTypes())
                        {
                            if (type.IsClass &&
                                !type.IsAbstract &&
                                typeof(BusDeviceBase).IsAssignableFrom(type))
                            {
                                try
                                {
                                    BusDeviceBase device = (BusDeviceBase)Activator.CreateInstance(type);
                                    var bdd = new BusDeviceDescriptor(
                                        type,
                                        device.Category,
                                        device.Name,
                                        device.Description);
                                    listDescriptors.Add(bdd);
                                }
                                catch (Exception ex)
                                {
                                    LogAgent.Error(ex);
                                }
                            }
                        }
                    }
                    catch (Exception ex)
                    {
                        LogAgent.Error(ex);
                    }
                }
                s_descriptors = listDescriptors;
            }
        }

        public static IEnumerable<BusDeviceDescriptor> SelectWithout(
            IEnumerable<Type> ignoreList)
        {
            var list = new List<BusDeviceDescriptor>();
            foreach (var bdd in Descriptors)
            {
                var ignore = false;
                foreach (var type in ignoreList)
                {
                    if (type==bdd.Type)
                    {
                        ignore = true;
                        break;
                    }
                }
                if (!ignore)
                {
                    list.Add(bdd);
                }
            }
            return list;
        }

        public static IEnumerable<BusDeviceDescriptor> SelectByCategoryWithout(
            BusDeviceCategory category, 
            IEnumerable<Type> ignoreList)
        {
            var list = new List<BusDeviceDescriptor>();
            foreach (var bdd in Descriptors)
            {
                if (bdd.Category != category)
                {
                    continue;
                }
                var ignore = false;
                foreach (var type in ignoreList)
                {
                    if (type == bdd.Type)
                    {
                        ignore = true;
                        break;
                    }
                }
                if (!ignore)
                {
                    list.Add(bdd);
                }
            }
            return list;
        }

        public static IEnumerable<BusDeviceDescriptor> SelectByType<T>()
        {
            var list = new List<BusDeviceDescriptor>();
            foreach (var bdd in Descriptors)
            {
                if (typeof(T).IsAssignableFrom(bdd.Type))
                {
                    list.Add(bdd);
                }
            }
            return list;
        }

        #region Private

        private static void PreLoadPlugins()
        {
            var folderName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            folderName = Path.Combine(folderName, "Plugins");
            if (Directory.Exists(folderName))
            {
                foreach (var fileName in Directory.GetFiles(folderName, "*.dll", SearchOption.AllDirectories))
                {
                    try
                    {
                        var asm = Assembly.LoadFrom(fileName);
                    }
                    catch (Exception ex)
                    {
                        LogAgent.Error(ex);
                        DialogProvider.Show(
                            string.Format("Load plugin failed!\n\n{0}", fileName),
                            "WARNING",
                            DlgButtonSet.OK,
                            DlgIcon.Warning);
                    }
                }
            }
        }

        #endregion Private
    }
}
