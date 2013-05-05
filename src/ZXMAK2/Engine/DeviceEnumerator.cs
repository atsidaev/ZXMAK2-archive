using System;
using System.Collections.Generic;
using System.Text;
using ZXMAK2.Interfaces;
using ZXMAK2.Entities;
using System.IO;
using System.Reflection;

namespace ZXMAK2.Engine
{
    public class DeviceEnumerator
    {
        public void Refresh()
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
            Descriptors = listDescriptors;
        }

        private void PreLoadPlugins()
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
        
        public IList<BusDeviceDescriptor> Descriptors { get; private set; }

        public IEnumerable<BusDeviceDescriptor> SelectWithout(
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

        public IEnumerable<BusDeviceDescriptor> SelectByCategoryWithout(
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

        public IEnumerable<BusDeviceDescriptor> SelectByType<T>()
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
    }
}
