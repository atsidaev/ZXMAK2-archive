namespace ZXMAK2.Controls.Configuration
{
    partial class FormAddDeviceWizard
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.ListViewItem listViewItem1 = new System.Windows.Forms.ListViewItem("Memory", 0);
            System.Windows.Forms.ListViewItem listViewItem2 = new System.Windows.Forms.ListViewItem("ULA", 2);
            System.Windows.Forms.ListViewItem listViewItem3 = new System.Windows.Forms.ListViewItem("Disk", 3);
            System.Windows.Forms.ListViewItem listViewItem4 = new System.Windows.Forms.ListViewItem("Sound", 4);
            System.Windows.Forms.ListViewItem listViewItem5 = new System.Windows.Forms.ListViewItem("Music", 5);
            System.Windows.Forms.ListViewItem listViewItem6 = new System.Windows.Forms.ListViewItem("Tape", 6);
            System.Windows.Forms.ListViewItem listViewItem7 = new System.Windows.Forms.ListViewItem("Keyboard", 7);
            System.Windows.Forms.ListViewItem listViewItem8 = new System.Windows.Forms.ListViewItem("Mouse", 8);
            System.Windows.Forms.ListViewItem listViewItem9 = new System.Windows.Forms.ListViewItem("Other", 1);
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(FormAddDeviceWizard));
            this.pnlTop = new System.Windows.Forms.Panel();
            this.lblActionAim = new System.Windows.Forms.Label();
            this.lblActionHint = new System.Windows.Forms.Label();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnBack = new System.Windows.Forms.Button();
            this.btnNext = new System.Windows.Forms.Button();
            this.tabControl = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.lstCategory = new System.Windows.Forms.ListView();
            this.colCategory = ((System.Windows.Forms.ColumnHeader)(new System.Windows.Forms.ColumnHeader()));
            this.imageList = new System.Windows.Forms.ImageList(this.components);
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.pnlTop.SuspendLayout();
            this.tabControl.SuspendLayout();
            this.tabPage1.SuspendLayout();
            this.SuspendLayout();
            // 
            // pnlTop
            // 
            this.pnlTop.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.pnlTop.Controls.Add(this.lblActionAim);
            this.pnlTop.Controls.Add(this.lblActionHint);
            this.pnlTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTop.Location = new System.Drawing.Point(0, 0);
            this.pnlTop.Name = "pnlTop";
            this.pnlTop.Size = new System.Drawing.Size(426, 52);
            this.pnlTop.TabIndex = 0;
            // 
            // lblActionAim
            // 
            this.lblActionAim.AutoSize = true;
            this.lblActionAim.Location = new System.Drawing.Point(43, 27);
            this.lblActionAim.Name = "lblActionAim";
            this.lblActionAim.Size = new System.Drawing.Size(224, 13);
            this.lblActionAim.TabIndex = 1;
            this.lblActionAim.Text = "What category of device do you want to add?";
            // 
            // lblActionHint
            // 
            this.lblActionHint.AutoSize = true;
            this.lblActionHint.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.lblActionHint.Location = new System.Drawing.Point(20, 9);
            this.lblActionHint.Name = "lblActionHint";
            this.lblActionHint.Size = new System.Drawing.Size(101, 13);
            this.lblActionHint.TabIndex = 0;
            this.lblActionHint.Text = "Device Category";
            // 
            // btnCancel
            // 
            this.btnCancel.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnCancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.btnCancel.Location = new System.Drawing.Point(345, 320);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(75, 22);
            this.btnCancel.TabIndex = 1;
            this.btnCancel.Text = "Cancel";
            this.btnCancel.UseVisualStyleBackColor = true;
            // 
            // btnBack
            // 
            this.btnBack.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnBack.Enabled = false;
            this.btnBack.Location = new System.Drawing.Point(183, 320);
            this.btnBack.Name = "btnBack";
            this.btnBack.Size = new System.Drawing.Size(75, 22);
            this.btnBack.TabIndex = 2;
            this.btnBack.Text = "< Back";
            this.btnBack.UseVisualStyleBackColor = true;
            this.btnBack.Click += new System.EventHandler(this.btnBack_Click);
            // 
            // btnNext
            // 
            this.btnNext.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
            this.btnNext.Location = new System.Drawing.Point(264, 320);
            this.btnNext.Name = "btnNext";
            this.btnNext.Size = new System.Drawing.Size(75, 22);
            this.btnNext.TabIndex = 3;
            this.btnNext.Text = "Next >";
            this.btnNext.UseVisualStyleBackColor = true;
            this.btnNext.Click += new System.EventHandler(this.btnNext_Click);
            // 
            // tabControl
            // 
            this.tabControl.Appearance = System.Windows.Forms.TabAppearance.FlatButtons;
            this.tabControl.Controls.Add(this.tabPage1);
            this.tabControl.Controls.Add(this.tabPage2);
            this.tabControl.ItemSize = new System.Drawing.Size(58, 18);
            this.tabControl.Location = new System.Drawing.Point(-5, 52);
            this.tabControl.Multiline = true;
            this.tabControl.Name = "tabControl";
            this.tabControl.SelectedIndex = 0;
            this.tabControl.Size = new System.Drawing.Size(440, 262);
            this.tabControl.SizeMode = System.Windows.Forms.TabSizeMode.Fixed;
            this.tabControl.TabIndex = 4;
            this.tabControl.SelectedIndexChanged += new System.EventHandler(this.tabControl_SelectedIndexChanged);
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.lstCategory);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(432, 236);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "tabPage1";
            // 
            // lstCategory
            // 
            this.lstCategory.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom)
                        | System.Windows.Forms.AnchorStyles.Left)));
            this.lstCategory.Columns.AddRange(new System.Windows.Forms.ColumnHeader[] {
            this.colCategory});
            this.lstCategory.FullRowSelect = true;
            this.lstCategory.HeaderStyle = System.Windows.Forms.ColumnHeaderStyle.None;
            this.lstCategory.HideSelection = false;
            this.lstCategory.Items.AddRange(new System.Windows.Forms.ListViewItem[] {
            listViewItem1,
            listViewItem2,
            listViewItem3,
            listViewItem4,
            listViewItem5,
            listViewItem6,
            listViewItem7,
            listViewItem8,
            listViewItem9});
            this.lstCategory.Location = new System.Drawing.Point(8, 12);
            this.lstCategory.MultiSelect = false;
            this.lstCategory.Name = "lstCategory";
            this.lstCategory.Size = new System.Drawing.Size(186, 209);
            this.lstCategory.SmallImageList = this.imageList;
            this.lstCategory.TabIndex = 1;
            this.lstCategory.UseCompatibleStateImageBehavior = false;
            this.lstCategory.View = System.Windows.Forms.View.Details;
            this.lstCategory.ItemSelectionChanged += new System.Windows.Forms.ListViewItemSelectionChangedEventHandler(this.lstCategory_ItemSelectionChanged);
            this.lstCategory.DoubleClick += new System.EventHandler(this.lstCategory_DoubleClick);
            // 
            // colCategory
            // 
            this.colCategory.Text = "Category";
            this.colCategory.Width = 156;
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
            // 
            // tabPage2
            // 
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(432, 236);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "tabPage2";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // FormAddDeviceWizard
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.CancelButton = this.btnCancel;
            this.ClientSize = new System.Drawing.Size(426, 347);
            this.Controls.Add(this.tabControl);
            this.Controls.Add(this.pnlTop);
            this.Controls.Add(this.btnNext);
            this.Controls.Add(this.btnBack);
            this.Controls.Add(this.btnCancel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormAddDeviceWizard";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "Add Device Wizard";
            this.pnlTop.ResumeLayout(false);
            this.pnlTop.PerformLayout();
            this.tabControl.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel pnlTop;
        private System.Windows.Forms.Label lblActionAim;
        private System.Windows.Forms.Label lblActionHint;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnBack;
        private System.Windows.Forms.Button btnNext;
        private System.Windows.Forms.TabControl tabControl;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.ListView lstCategory;
        private System.Windows.Forms.ColumnHeader colCategory;
        private System.Windows.Forms.ImageList imageList;
    }
}