using System;
using System.IO;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using System.Reflection;
using ZXMAK2.Interfaces;
using ZXMAK2.Engine;

namespace ZXMAK2.Controls.Configuration
{
    public partial class FormAddDeviceWizard : Form
    {
        public FormAddDeviceWizard()
        {
            InitializeComponent();
            tabControl.ItemSize = new Size(0, 1);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            refreshDeviceList();
        }

        private BusDeviceBase m_device = null;
        public BusDeviceBase Device { get { return m_device; } }

        private List<BusDeviceBase> m_ignoreList = new List<BusDeviceBase>();
        public List<BusDeviceBase> IgnoreList
        {
            get { return m_ignoreList; }
            set { m_ignoreList = value; /*refreshDeviceList();*/ }
        }

        private void tabControl_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabControl.TabIndex == 0)
            {
                //lblActionHint.Text = "Device Category";
                //lblActionAim.Text = "What category of device do you want to add?";
                lblActionHint.Text = "Device Type";
                lblActionAim.Text = "What type of device do you want to add?";
                btnNext.Text = "Finish";
            }
        }

        private void btnNext_Click(object sender, EventArgs e)
        {
            if (tabControl.SelectedIndex == 0)
            {
                int index = getSelectedCategoryIndex();
                if(index<0)
                    return;
                ListViewItem lvi = lstCategory.Items[index];
                m_device = (BusDeviceBase)lvi.Tag;
                DialogResult = System.Windows.Forms.DialogResult.OK;
                Close();
            }
        }

        private void btnBack_Click(object sender, EventArgs e)
        {
            if (tabControl.SelectedIndex == 1)
            {
                tabControl.SelectedIndex--;
                btnNext.Text = "Next >";
                btnNext.Enabled = true;     // bcz already selected
                btnBack.Enabled = false;
            }
        }
        
        #region Step 1 - Device Category
        
        //private BusCategory m_busCategory;
        
        private static BusCategory[] categoryList = new BusCategory[]
        {
            BusCategory.Memory, BusCategory.ULA, BusCategory.Disk, BusCategory.Sound,
            BusCategory.Music, BusCategory.Tape, BusCategory.Keyboard, BusCategory.Mouse,
            BusCategory.Other,
        };

        private void lstCategory_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            btnNext.Text = "Finish";
            btnNext.Enabled = e.IsSelected;
            lblActionHint.Text = "Device Type";
            lblActionAim.Text = "What type of device do you want to add?";
        }

        private int getSelectedCategoryIndex()
        {
            foreach (int index in lstCategory.SelectedIndices)
                return index;
            return -1;
        }

        #endregion

        private void refreshDeviceList()
        {
            lstCategory.Items.Clear();

            List<ListViewItem> list = new List<ListViewItem>();
            foreach (Type type in CollectBusDeviceTypes())
            {
                try
                {
                    BusDeviceBase device = (BusDeviceBase)Activator.CreateInstance(type);
                    ListViewItem lvi = new ListViewItem();
                    lvi.Tag = device;
                    lvi.Text = string.Format("{0} - {1}",device.Category, device.Name);
                    lvi.ImageIndex = FormMachineSettings.FindImageIndex(device.Category);
                    list.Add(lvi);
                }
                catch(Exception ex)
                {
                    LogAgent.Error(ex);
                }
            }
            list.Sort(CategoryComparison);
            lstCategory.Items.AddRange(list.ToArray());
            lstCategory.SelectedIndices.Clear();
            lstCategory.SelectedIndices.Add(0);
        }

        private int CategoryComparison<T>(T x, T y) where T : ListViewItem
        {
            BusDeviceBase dev1 = x.Tag as BusDeviceBase;
            BusDeviceBase dev2 = y.Tag as BusDeviceBase;
            if(dev1!=null && dev2!=null)
            {
                if (dev1.Category != dev2.Category)
                    return dev1.Category.CompareTo(dev2.Category);
                else 
                    return dev1.Name.CompareTo(dev2.Name);
            }
            return 0;
        }

        private Type[] CollectBusDeviceTypes()
        {
            List<Type> list = new List<Type>();
            
            string folderName = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
            folderName = Path.Combine(folderName, "Plugins");
            if (Directory.Exists(folderName))
            {
                foreach (string fileName in Directory.GetFiles(folderName, "*.dll", SearchOption.AllDirectories))
                {
                    try
                    {
                        Assembly asm = Assembly.LoadFrom(fileName);
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
            foreach (Assembly asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    foreach (Type type in asm.GetTypes())
                        if (type.IsClass && !type.IsAbstract && typeof(BusDeviceBase).IsAssignableFrom(type))
                        {
                            bool ignore = false;
                            foreach (BusDeviceBase bd in IgnoreList)
                                if (bd != null && bd.GetType() == type)
                                {
                                    ignore = true;
                                    break;
                                }
                            if (!ignore)
                                list.Add(type);
                        }
                }
                catch (Exception ex)
                {
                    LogAgent.Error(ex);
                    DialogProvider.Show(
                        string.Format("Bad plugin assembly!\nSee logs for details\n\n{0}", asm.Location), 
                        "ERROR",
                        DlgButtonSet.OK,
                        DlgIcon.Error);
                }
            }
            return list.ToArray();
        }

        private void lstCategory_DoubleClick(object sender, EventArgs e)
        {
            if (btnNext.Enabled)
                btnNext_Click(sender, e);
        }
    }
}
