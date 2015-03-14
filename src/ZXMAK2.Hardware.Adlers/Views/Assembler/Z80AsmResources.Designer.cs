namespace ZXMAK2.Hardware.Adlers.Views.AssemblerView
{
    partial class Z80AsmResources
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
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.splitterRoutines = new System.Windows.Forms.SplitContainer();
            this.treeZ80Resources = new System.Windows.Forms.TreeView();
            this.htmlItemDesc = new System.Windows.Forms.WebBrowser();
            this.buttonDone = new System.Windows.Forms.Button();
            this.buttonAdd = new System.Windows.Forms.Button();
            this.groupBox1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitterRoutines)).BeginInit();
            this.splitterRoutines.Panel1.SuspendLayout();
            this.splitterRoutines.Panel2.SuspendLayout();
            this.splitterRoutines.SuspendLayout();
            this.SuspendLayout();
            // 
            // groupBox1
            // 
            this.groupBox1.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.groupBox1.Controls.Add(this.splitterRoutines);
            this.groupBox1.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.groupBox1.Location = new System.Drawing.Point(12, 12);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(377, 497);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Source code to use:";
            // 
            // splitterRoutines
            // 
            this.splitterRoutines.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitterRoutines.Location = new System.Drawing.Point(3, 19);
            this.splitterRoutines.Name = "splitterRoutines";
            // 
            // splitterRoutines.Panel1
            // 
            this.splitterRoutines.Panel1.Controls.Add(this.treeZ80Resources);
            // 
            // splitterRoutines.Panel2
            // 
            this.splitterRoutines.Panel2.Controls.Add(this.htmlItemDesc);
            this.splitterRoutines.Size = new System.Drawing.Size(371, 475);
            this.splitterRoutines.SplitterDistance = 145;
            this.splitterRoutines.TabIndex = 3;
            // 
            // treeZ80Resources
            // 
            this.treeZ80Resources.CheckBoxes = true;
            this.treeZ80Resources.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeZ80Resources.Font = new System.Drawing.Font("Calibri", 9.75F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.treeZ80Resources.HideSelection = false;
            this.treeZ80Resources.Location = new System.Drawing.Point(0, 0);
            this.treeZ80Resources.Name = "treeZ80Resources";
            this.treeZ80Resources.Size = new System.Drawing.Size(145, 475);
            this.treeZ80Resources.TabIndex = 0;
            this.treeZ80Resources.AfterCheck += new System.Windows.Forms.TreeViewEventHandler(this.treeZ80Resources_AfterCheck);
            this.treeZ80Resources.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeZ80Resources_AfterSelect);
            // 
            // htmlItemDesc
            // 
            this.htmlItemDesc.Dock = System.Windows.Forms.DockStyle.Fill;
            this.htmlItemDesc.Location = new System.Drawing.Point(0, 0);
            this.htmlItemDesc.MinimumSize = new System.Drawing.Size(20, 20);
            this.htmlItemDesc.Name = "htmlItemDesc";
            this.htmlItemDesc.ScrollBarsEnabled = false;
            this.htmlItemDesc.Size = new System.Drawing.Size(222, 475);
            this.htmlItemDesc.TabIndex = 0;
            // 
            // buttonDone
            // 
            this.buttonDone.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonDone.Location = new System.Drawing.Point(395, 41);
            this.buttonDone.Name = "buttonDone";
            this.buttonDone.Size = new System.Drawing.Size(51, 23);
            this.buttonDone.TabIndex = 2;
            this.buttonDone.Text = "Done";
            this.buttonDone.UseVisualStyleBackColor = true;
            this.buttonDone.Click += new System.EventHandler(this.buttonDone_Click);
            // 
            // buttonAdd
            // 
            this.buttonAdd.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonAdd.Location = new System.Drawing.Point(395, 12);
            this.buttonAdd.Name = "buttonAdd";
            this.buttonAdd.Size = new System.Drawing.Size(51, 23);
            this.buttonAdd.TabIndex = 3;
            this.buttonAdd.Text = "Add";
            this.buttonAdd.UseVisualStyleBackColor = true;
            this.buttonAdd.Click += new System.EventHandler(this.buttonAdd_Click);
            // 
            // Z80AsmResources
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(458, 510);
            this.Controls.Add(this.buttonAdd);
            this.Controls.Add(this.buttonDone);
            this.Controls.Add(this.groupBox1);
            this.Name = "Z80AsmResources";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.Text = "Z80AsmResources";
            this.groupBox1.ResumeLayout(false);
            this.splitterRoutines.Panel1.ResumeLayout(false);
            this.splitterRoutines.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitterRoutines)).EndInit();
            this.splitterRoutines.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.TreeView treeZ80Resources;
        private System.Windows.Forms.WebBrowser htmlItemDesc;
        private System.Windows.Forms.SplitContainer splitterRoutines;
        private System.Windows.Forms.Button buttonDone;
        private System.Windows.Forms.Button buttonAdd;
    }
}