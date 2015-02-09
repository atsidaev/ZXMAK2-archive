﻿using System;
using System.Linq;
using System.Xml;
using System.Drawing;
using System.Windows.Forms;
using System.Collections.Generic;

using ZXMAK2.Dependency;
using ZXMAK2.Host.Interfaces;
using ZXMAK2.Engine;
using ZXMAK2.Engine.Interfaces;
using ZXMAK2.Engine.Entities;
using ZXMAK2.Host.Presentation.Interfaces;
using ZXMAK2.Host.Entities;
using ZXMAK2.Host.WinForms.Views.Configuration.Devices;
using ZXMAK2.Host.WinForms.Tools;


namespace ZXMAK2.Host.WinForms.Views
{
    public class FormMachineSettings : Form, IMachineSettingsView
    {
        #region Windows Form Designer generated code

        private System.ComponentModel.IContainer components = null;
        private System.Windows.Forms.ListView lstNavigation;
        private System.Windows.Forms.ColumnHeader colDevice;
        private System.Windows.Forms.ColumnHeader colSummary;
        private System.Windows.Forms.Panel pnlSettings;
        private System.Windows.Forms.Button btnRemove;
        private System.Windows.Forms.Button btnAdd;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnApply;
        private Button btnUp;
        private Button btnDown;
        private Button btnWizard;
        private ContextMenuStrip ctxMenuWizard;
        private ZXMAK2.Host.WinForms.Controls.Separator separator1;
        private System.Windows.Forms.ImageList imageList;

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.ListViewItem listViewItem1 = new System.Windows.Forms.ListViewItem("Memory", 0);
            System.Windows.Forms.ListViewItem listViewItem2 = new System.Windows.Forms.ListViewItem("ULA", 1);
            System.Windows.Forms.ListViewItem listViewItem3 = new System.Windows.Forms.ListViewItem("Processor", 2);
            System.Windows.Forms.ListViewItem listViewItem4 = new System.Windows.Forms.ListViewItem("Beta Disk Interface", 3);
            System.Windows.Forms.ListViewItem listViewItem5 = new System.Windows.Forms.ListViewItem("Beeper", 4);
            System.Windows.Forms.ListViewItem listViewItem6 = new System.Windows.Forms.ListViewItem("Sound", 5);
            System.Windows.Forms.ListViewItem listViewItem7 = new System.Windows.Forms.ListViewItem("Tape", 6);
            System.Windows.Forms.ListViewItem listViewItem8 = new System.Windows.Forms.ListViewItem("Display", 7);
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormMachineSettings));
            this.lstNavigation = new System.Windows.Forms.ListView();
            this.colDevice = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.colSummary = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.imageList = new System.Windows.Forms.ImageList(this.components);
            this.pnlSettings = new System.Windows.Forms.Panel();
            this.btnRemove = new System.Windows.Forms.Button();
            this.btnAdd = new System.Windows.Forms.Button();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnApply = new System.Windows.Forms.Button();
            this.btnUp = new System.Windows.Forms.Button();
            this.btnDown = new System.Windows.Forms.Button();
            this.btnWizard = new System.Windows.Forms.Button();
            this.ctxMenuWizard = new System.Windows.Forms.ContextMenuStrip(this.components);
            this.separator1 = new ZXMAK2.Host.WinForms.Controls.Separator();
            this.SuspendLayout();
            // 
            // lstNavigation
            // 
            this.lstNavigation.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Left)));
            this.lstNavigation.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colDevice,
            this.colSummary});
            this.lstNavigation.FullRowSelect = true;
            this.lstNavigation.HideSelection = false;
            this.lstNavigation.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            listViewItem1,
            listViewItem2,
            listViewItem3,
            listViewItem4,
            listViewItem5,
            listViewItem6,
            listViewItem7,
            listViewItem8});
            this.lstNavigation.Location = new System.Drawing.Point(12, 12);
            this.lstNavigation.MultiSelect = false;
            this.lstNavigation.Name = "lstNavigation";
            this.lstNavigation.Size = new System.Drawing.Size(260, 332);
            this.lstNavigation.SmallImageList = this.imageList;
            this.lstNavigation.TabIndex = 0;
            this.lstNavigation.UseCompatibleStateImageBehavior = false;
            this.lstNavigation.View = System.Windows.Forms.View.Details;
            this.lstNavigation.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.lstNavigation_ItemSelectionChanged);
            // 
            // colDevice
            // 
            this.colDevice.Text = "Device";
            this.colDevice.Width = 128;
            // 
            // colSummary
            // 
            this.colSummary.Text = "Summary";
            this.colSummary.Width = 128;
            // 
            // imageList
            // 
            this.imageList.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imageList.ImageStream")));
            this.imageList.TransparentColor = System.Drawing.Color.Transparent;
            this.imageList.Images.SetKeyName(0, "RAMx16.png");
            this.imageList.Images.SetKeyName(1, "PCBx16.png");
            this.imageList.Images.SetKeyName(2, "ULAx16.png");
            this.imageList.Images.SetKeyName(3, "FDDx16.png");
            this.imageList.Images.SetKeyName(4, "BEEPERx16.png");
            this.imageList.Images.SetKeyName(5, "AY8910x16.png");
            this.imageList.Images.SetKeyName(6, "TAPEx16.png");
            this.imageList.Images.SetKeyName(7, "KBDx16.png");
            this.imageList.Images.SetKeyName(8, "MOUSx16.png");
            this.imageList.Images.SetKeyName(9, "DISPLAYx16.png");
            this.imageList.Images.SetKeyName(10, "DEBUGx16.png");
            // 
            // pnlSettings
            // 
            this.pnlSettings.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.pnlSettings.Location = new System.Drawing.Point(278, 12);
            this.pnlSettings.Name = "pnlSettings";
            this.pnlSettings.Size = new System.Drawing.Size(284, 332);
            this.pnlSettings.TabIndex = 1;
            // 
            // btnRemove
            // 
            this.btnRemove.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnRemove.Enabled = false;
            this.btnRemove.Location = new System.Drawing.Point(197, 362);
            this.btnRemove.Name = "btnRemove";
            this.btnRemove.Size = new System.Drawing.Size(75, 27);
            this.btnRemove.TabIndex = 2;
            this.btnRemove.Text = "Remove";
            this.btnRemove.UseVisualStyleBackColor = true;
            this.btnRemove.Click += new System.EventHandler(this.btnRemove_Click);
            // 
            // btnAdd
            // 
            this.btnAdd.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnAdd.Enabled = false;
            this.btnAdd.Location = new System.Drawing.Point(116, 362);
            this.btnAdd.Name = "btnAdd";
            this.btnAdd.Size = new System.Drawing.Size(75, 27);
            this.btnAdd.TabIndex = 3;
            this.btnAdd.Text = "Add...";
            this.btnAdd.UseVisualStyleBackColor = true;
            this.btnAdd.Click += new System.EventHandler(this.btnAdd_Click);
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(487, 362);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 27);
            this.btnCancel.TabIndex = 4;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnApply
            // 
            this.btnApply.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnApply.Location = new System.Drawing.Point(406, 362);
            this.btnApply.Name = "btnApply";
            this.btnApply.Size = new System.Drawing.Size(75, 27);
            this.btnApply.TabIndex = 5;
            this.btnApply.Text = "Apply";
            this.btnApply.UseVisualStyleBackColor = true;
            this.btnApply.Click += new System.EventHandler(this.btnApply_Click);
            // 
            // btnUp
            // 
            this.btnUp.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnUp.Location = new System.Drawing.Point(12, 362);
            this.btnUp.Name = "btnUp";
            this.btnUp.Size = new System.Drawing.Size(27, 27);
            this.btnUp.TabIndex = 6;
            this.btnUp.Text = "/\\";
            this.btnUp.UseVisualStyleBackColor = true;
            this.btnUp.Click += new System.EventHandler(this.btnUp_Click);
            // 
            // btnDown
            // 
            this.btnDown.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)));
            this.btnDown.Location = new System.Drawing.Point(45, 362);
            this.btnDown.Name = "btnDown";
            this.btnDown.Size = new System.Drawing.Size(27, 27);
            this.btnDown.TabIndex = 7;
            this.btnDown.Text = "\\/";
            this.btnDown.UseVisualStyleBackColor = true;
            this.btnDown.Click += new System.EventHandler(this.btnDown_Click);
            // 
            // btnWizard
            // 
            this.btnWizard.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnWizard.Image = ((System.Drawing.Image)(resources.GetObject("btnWizard.Image")));
            this.btnWizard.Location = new System.Drawing.Point(325, 362);
            this.btnWizard.Name = "btnWizard";
            this.btnWizard.Size = new System.Drawing.Size(75, 27);
            this.btnWizard.TabIndex = 8;
            this.btnWizard.Text = "Wizard";
            this.btnWizard.TextImageRelation = System.Windows.Forms.TextImageRelation.ImageBeforeText;
            this.btnWizard.UseVisualStyleBackColor = true;
            this.btnWizard.Click += new System.EventHandler(this.btnWizard_Click);
            // 
            // ctxMenuWizard
            // 
            this.ctxMenuWizard.Name = "ctxMenuWizard";
            this.ctxMenuWizard.Size = new System.Drawing.Size(61, 4);
            // 
            // separator1
            // 
            this.separator1.Alignment = System.Drawing.ContentAlignment.MiddleCenter;
            this.separator1.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
            | System.Windows.Forms.AnchorStyles.Right)));
            this.separator1.Location = new System.Drawing.Point(-3, 350);
            this.separator1.Name = "separator1";
            this.separator1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            this.separator1.Size = new System.Drawing.Size(580, 6);
            this.separator1.TabIndex = 9;
            this.separator1.Text = "separator1";
            // 
            // FormMachineSettings
            // 
            this.AcceptButton = this.btnApply;
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(574, 397);
            this.Controls.Add(this.separator1);
            this.Controls.Add(this.btnApply);
            this.Controls.Add(this.btnWizard);
            this.Controls.Add(this.btnUp);
            this.Controls.Add(this.btnDown);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.btnAdd);
            this.Controls.Add(this.btnRemove);
            this.Controls.Add(this.pnlSettings);
            this.Controls.Add(this.lstNavigation);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormMachineSettings";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Machine Settings";
            this.ResumeLayout(false);

        }

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #endregion

        #region private

        private readonly MachinesConfig m_machines = new MachinesConfig();
        private IHostService m_host;
        private IVirtualMachine m_vm;
        private BusManager m_workBus;
        private List<ConfigScreenControl> m_ctlList = new List<ConfigScreenControl>();
        private List<BusDeviceBase> m_devList = new List<BusDeviceBase>();

        #endregion


        public FormMachineSettings()
        {
            InitializeComponent();
            LoadMachines();
            btnWizard.Enabled = ctxMenuWizard.Items.Count > 0;
        }

        private void LoadMachines()
        {
            try
            {
                m_machines.Load();
                foreach (var name in m_machines.GetNames())
                {
                    var node = m_machines.GetConfig(name);
                    var item = ctxMenuWizard.Items.Add(name);
                    item.Tag = node;
                    item.Click += new EventHandler(ctxMenuWizardItem_Click);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        private void insertListViewItem(int index, UserControl control, BusDeviceBase device)
        {
            control.Location = new Point(0, 0);
            control.Size = pnlSettings.ClientSize;
            control.Visible = false;
            pnlSettings.Controls.Add(control);
            var csc = (ConfigScreenControl)control;
            m_ctlList.Insert(index, csc);
            m_devList.Insert(index, device);
            var lvi = new ListViewItem();
            lvi.Tag = csc;
            lvi.Text = device.Category.ToString();
            lvi.SubItems.Add(device.Name);
            lvi.ImageIndex = FindImageIndex(device.Category);
            lstNavigation.Items.Insert(index, lvi);
        }

        public static int FindImageIndex(BusDeviceCategory category)
        {
            switch (category)
            {
                case BusDeviceCategory.Memory:
                    return 0;
                case BusDeviceCategory.Other:
                    return 1;
                case BusDeviceCategory.ULA:
                    return 2;
                case BusDeviceCategory.Disk:
                    return 3;
                case BusDeviceCategory.Sound:
                    return 4;
                case BusDeviceCategory.Music:
                    return 5;
                case BusDeviceCategory.Tape:
                    return 6;
                case BusDeviceCategory.Keyboard:
                    return 7;
                case BusDeviceCategory.Mouse:
                    return 8;
                case BusDeviceCategory.Debugger:
                    return 10;

                default:
                    return 1;
            }
        }

        private UserControl CreateConfigScreenControl(BusManager bmgr, object objTarget)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                try
                {
                    var refName = typeof(ConfigScreenControl).Assembly.GetName().FullName;
                    var hasRef = asm.GetName().FullName == refName ||
                        asm.GetReferencedAssemblies()
                            .Any(name => name.FullName == refName);
                    if (!hasRef)
                    {
                        // skip assemblies without reference on assembly which contains ConfigScreenControl 
                        continue;
                    }
                    foreach (Type type in asm.GetTypes())
                    {
                        try
                        {
                            if (type.IsClass &&
                                !type.IsAbstract &&
                                type != typeof(CtlSettingsGenericDevice) &&
                                typeof(ConfigScreenControl).IsAssignableFrom(type) &&
                                typeof(UserControl).IsAssignableFrom(type))
                            {
                                var mi = type.GetMethod("Init", new Type[] { typeof(BusManager), typeof(IHostService), objTarget.GetType() });
                                if (mi == null)
                                    continue;
                                var obj = (UserControl)Activator.CreateInstance(type);
                                mi.Invoke(obj, new object[] { bmgr, m_host, objTarget });
                                return obj;
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex, type.FullName);
                        }
                    }
                }
                catch (Exception ex)
                {
                    Logger.Error(ex, asm.FullName);
                }
            }
            return null;
        }

        public void Init(IHostService host, IVirtualMachine vm)
        {
            m_host = host;
            m_vm = vm;

            m_workBus = new BusManager();
            m_workBus.Init(null, true);

            var xml = new XmlDocument();
            var root = xml.AppendChild(xml.CreateElement("Bus"));
            try
            {
                m_vm.Bus.SaveConfigXml(root);

                m_workBus.LoadConfigXml(root);
                m_workBus.Disconnect();
                initWorkBus();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
            }
        }

        public DlgResult ShowDialog(object owner)
        {
            var win32owner = owner as IWin32Window;
            if (win32owner != null)
            {
                return EnumMapper.GetDlgResult(base.ShowDialog(win32owner));
            }
            else
            {
                return EnumMapper.GetDlgResult(base.ShowDialog());
            }
        }


        private void initConfig(XmlNode busNode)
        {
            m_workBus.Disconnect();
            m_workBus.Clear();
            m_workBus.LoadConfigXml(busNode);
            m_workBus.Disconnect();
            initWorkBus();
        }

        private void initWorkBus()
        {
            lstNavigation.Items.Clear();
            foreach (var ctl in m_ctlList)
            {
                Controls.Remove(ctl);
                ctl.Dispose();
            }
            m_ctlList.Clear();
            m_devList.Clear();
            foreach (var device in m_workBus.FindDevices<BusDeviceBase>())
            {
                try
                {
                    var control = ResolveScreenControl(m_workBus, m_host, device);
                    insertListViewItem(lstNavigation.Items.Count, control, device);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex);
                    Locator.Resolve<IUserMessage>().Error(
                        "The following device was failed to initialize and will be removed:\n{0}", 
                        device.GetType());
                    m_workBus.Remove(device);
                }
            }

            lstNavigation.SelectedItems.Clear();
            lstNavigation.Items[0].Selected = true;
        }

        private UserControl ResolveScreenControl(BusManager workBus, IHostService host, BusDeviceBase device)
        {
            var control = CreateConfigScreenControl(workBus, device);
            try
            {
                if (control != null)
                {
                    return control;
                }
                return CreateGenericScreenControl(workBus, host, device);
            }
            catch
            {
                if (control != null)
                {
                    control.Dispose();
                }
                throw;
            }
        }

        private static UserControl CreateGenericScreenControl(BusManager workBus, IHostService host, BusDeviceBase device)
        {
            var control = new CtlSettingsGenericDevice();
            try
            {
                control.Init(workBus, host, device);
                return control;
            }
            catch
            {
                control.Dispose();
                throw;
            }
        }

        private void lstNavigation_ItemSelectionChanged(object sender, ListViewItemSelectionChangedEventArgs e)
        {
            for (int i = 0; i < m_ctlList.Count; i++)
            {
                var ctl = (UserControl)m_ctlList[i];
                ctl.Visible = e.ItemIndex == i && e.IsSelected;
            }

            bool allowRemove = e.IsSelected &&
                e.ItemIndex >= 0 &&
                e.ItemIndex < m_ctlList.Count;
            btnAdd.Enabled = true;
            btnRemove.Enabled = allowRemove;
            btnUp.Enabled = IsMoveUpAllowed();
            btnDown.Enabled = IsMoveDownAllowed();
        }

        private bool IsMoveUpAllowed()
        {
            int index = getSelectedIndex();
            if (index <= 0 || index >= m_devList.Count - 1)
                return false;
            var device = index < lstNavigation.Items.Count - 1 ?
                m_devList[index] :
                null;
            if (device == null)
                return false;
            if (device is IUlaDevice)
                return false;
            if (device is IMemoryDevice)
                return false;
            index = index - 1;
            if (index <= 0 || index >= m_devList.Count - 1)
                return false;
            device = index < lstNavigation.Items.Count - 1 ?
                m_devList[index] :
                null;
            if (device == null)
                return false;
            if (device is IUlaDevice)
                return false;
            if (device is IMemoryDevice)
                return false;
            return true;
        }

        private bool IsMoveDownAllowed()
        {
            int index = getSelectedIndex();
            if (index <= 0 || index >= m_devList.Count - 1)
                return false;
            var device = index < lstNavigation.Items.Count - 1 ?
                m_devList[index] :
                null;
            if (device == null)
                return false;
            if (device is IUlaDevice)
                return false;
            if (device is IMemoryDevice)
                return false;
            index = index + 1;
            if (index <= 0 || index >= m_devList.Count - 1)
                return false;
            device = index < lstNavigation.Items.Count - 1 ?
                m_devList[index] :
                null;
            if (device == null)
                return false;
            if (device is IUlaDevice)
                return false;
            if (device is IMemoryDevice)
                return false;
            return true;
        }

        private void btnApply_Click(object sender, EventArgs e)
        {
            try
            {
                foreach (var csc in m_ctlList)
                {
                    csc.Apply();
                }
                if (m_workBus.FindDevice<IUlaDevice>() == null)
                {
                    Locator.Resolve<IUserMessage>()
                        .Error("Bad configuration!\n\nPease add ULA device!");
                    return;
                }
                if (m_workBus.FindDevice<IMemoryDevice>() == null)
                {
                    Locator.Resolve<IUserMessage>()
                        .Error("Bad configuration!\n\nPease add Memory device!");
                    return;
                }

                if (!m_workBus.Connect())
                {
                    Locator.Resolve<IUserMessage>()
                        .Error("Apply failed!\n\nThere is a problem in your machine configuration!\nSee logs for details");
                    m_workBus.Disconnect();
                    return;
                }

                XmlDocument xml = new XmlDocument();
                XmlNode root = xml.AppendChild(xml.CreateElement("Bus"));
                m_workBus.SaveConfigXml(root);

                bool running = m_vm.IsRunning;
                m_vm.DoStop();


                var bmgr = m_vm.Bus;

                // workaround to save border color + Reset in case when memory changed
                var ula = bmgr.FindDevice<IUlaDevice>();
                var oldMemory = bmgr.FindDevice<IMemoryDevice>();
                int portFE = ula != null ? ula.PortFE : 0x00;
                bmgr.LoadConfigXml(root);
                ula = bmgr.FindDevice<IUlaDevice>();
                ula.PortFE = (byte)portFE;
                var memory = bmgr.FindDevice<IMemoryDevice>();
                if (memory != oldMemory)
                    m_vm.DoReset();

                m_vm.SaveConfig();
                if (running)
                    m_vm.DoRun();
                GC.Collect();
                Close();
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                m_workBus.Disconnect();
                Locator.Resolve<IUserMessage>()
                    .Error("Apply failed!\n\n{0}", ex.Message);
            }
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            int index = getSelectedIndex();
            if (index >= 0 && index < m_devList.Count)
            {
                m_workBus.Remove(m_devList[index]);
                var control = (UserControl)m_ctlList[index];
                Controls.Remove(control);
                control.Dispose();
                m_ctlList.RemoveAt(index);
                m_devList.RemoveAt(index);
                lstNavigation.Items.RemoveAt(index);
                lstNavigation.Refresh();
                if (lstNavigation.Items.Count > index)
                    lstNavigation.SelectedIndices.Add(index);
            }
        }

        private int getSelectedIndex()
        {
            foreach (int index in lstNavigation.SelectedIndices)
                return index;
            return -1;
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            try
            {
                using (var wizard = new FormAddDeviceWizard())
                {
                    wizard.IgnoreList = m_devList;
                    if (wizard.ShowDialog() != System.Windows.Forms.DialogResult.OK)
                    {
                        return;
                    }

                    // apply to avoid loss ULA & MEMORY TYPE
                    foreach (var csc in m_ctlList)
                    {
                        csc.Apply();
                    }

                    var device = wizard.Device;
                    m_workBus.Add(device);

                    m_workBus.Sort();
                    initWorkBus();
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex);
                Locator.Resolve<IUserMessage>()
                    .Error("Add failed!\n\n{0}", ex.Message);
            }
        }

        private void btnUp_Click(object sender, EventArgs e)
        {
            int indexFrom = getSelectedIndex();
            var deviceFrom = indexFrom < lstNavigation.Items.Count - 1 ?
                m_devList[indexFrom] :
                null;
            if (deviceFrom is IUlaDevice)
                return;
            if (deviceFrom is IMemoryDevice)
                return;
            int indexTo = indexFrom - 1;
            if (indexTo < 0)
                return;
            var deviceTo = indexTo < lstNavigation.Items.Count - 1 ?
                m_devList[indexTo] :
                null;
            if (deviceTo is IUlaDevice)
                return;
            if (deviceTo is IMemoryDevice)
                return;
            int tmp = deviceFrom.BusOrder;
            deviceFrom.BusOrder = deviceTo.BusOrder;
            deviceTo.BusOrder = tmp;

            var device = m_devList[indexFrom];
            m_devList.RemoveAt(indexFrom);
            m_devList.Insert(indexTo, device);
            var ctl = m_ctlList[indexFrom];
            m_ctlList.RemoveAt(indexFrom);
            m_ctlList.Insert(indexTo, ctl);
            var lvi = lstNavigation.Items[indexFrom];
            lstNavigation.BeginUpdate();
            lstNavigation.Items.RemoveAt(indexFrom);
            lstNavigation.Items.Insert(indexTo, lvi);
            lstNavigation.EndUpdate();
            btnUp.Enabled = IsMoveUpAllowed();
            btnDown.Enabled = IsMoveDownAllowed();
        }

        private void btnDown_Click(object sender, EventArgs e)
        {
            int indexFrom = getSelectedIndex();
            var deviceFrom = indexFrom < lstNavigation.Items.Count - 1 ?
                m_devList[indexFrom] :
                null;
            if (deviceFrom == null)
                return;
            if (deviceFrom is IUlaDevice)
                return;
            if (deviceFrom is IMemoryDevice)
                return;
            int indexTo = indexFrom + 1;
            if (indexTo > lstNavigation.Items.Count - 1)
                return;
            var deviceTo = indexTo < lstNavigation.Items.Count - 1 ?
                m_devList[indexTo] :
                null;
            if (deviceTo == null)
                return;
            if (deviceTo is IUlaDevice)
                return;
            if (deviceTo is IMemoryDevice)
                return;
            int tmp = deviceFrom.BusOrder;
            deviceFrom.BusOrder = deviceTo.BusOrder;
            deviceTo.BusOrder = tmp;

            var device = m_devList[indexTo];
            m_devList.RemoveAt(indexTo);
            m_devList.Insert(indexFrom, device);
            var ctl = m_ctlList[indexTo];
            m_ctlList.RemoveAt(indexTo);
            m_ctlList.Insert(indexFrom, ctl);
            var lvi = lstNavigation.Items[indexTo];
            lstNavigation.BeginUpdate();
            lstNavigation.Items.RemoveAt(indexTo);
            lstNavigation.Items.Insert(indexFrom, lvi);
            lstNavigation.EndUpdate();
            btnUp.Enabled = IsMoveUpAllowed();
            btnDown.Enabled = IsMoveDownAllowed();
        }

        private void btnWizard_Click(object sender, EventArgs e)
        {
            if (ctxMenuWizard.Items.Count < 1)
                return;
            var p = new Point(
                btnWizard.Location.X + btnWizard.Width / 2,
                btnWizard.Location.Y + btnWizard.Height / 2);
            p = this.PointToScreen(p);
            ctxMenuWizard.Show(p);
        }

        private void ctxMenuWizardItem_Click(object sender, EventArgs e)
        {
            var item = sender as ToolStripMenuItem;
            if (item == null)
            {
                return;
            }
            var busNode = item.Tag as XmlNode;

            if (busNode != null)
            {
                initConfig(busNode);
            }
            else
            {
                Locator.Resolve<IUserMessage>()
                    .Error("Invalid Configuration File!");
            }
        }
    }
}
