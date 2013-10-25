namespace ZXMAK2.Hardware.Adlers.UI
{
    partial class Assembler
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
            this.txtAsm = new System.Windows.Forms.RichTextBox();
            this.btnCompile = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();
            this.richCompileMessages = new System.Windows.Forms.RichTextBox();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.textSaveFileName = new System.Windows.Forms.TextBox();
            this.textMemAdress = new System.Windows.Forms.TextBox();
            this.checkFile = new System.Windows.Forms.CheckBox();
            this.checkMemory = new System.Windows.Forms.CheckBox();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // txtAsm
            // 
            this.txtAsm.AcceptsTab = true;
            this.txtAsm.Font = new System.Drawing.Font("Consolas", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(238)));
            this.txtAsm.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(64)))), ((int)(((byte)(64)))), ((int)(((byte)(64)))));
            this.txtAsm.Location = new System.Drawing.Point(12, 35);
            this.txtAsm.Name = "txtAsm";
            this.txtAsm.Size = new System.Drawing.Size(371, 448);
            this.txtAsm.TabIndex = 0;
            this.txtAsm.Text = "";
            this.txtAsm.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.txtAsm_KeyPress);
            this.txtAsm.KeyUp += new System.Windows.Forms.KeyEventHandler(this.txtAsm_KeyUp);
            // 
            // btnCompile
            // 
            this.btnCompile.Location = new System.Drawing.Point(389, 487);
            this.btnCompile.Name = "btnCompile";
            this.btnCompile.Size = new System.Drawing.Size(85, 23);
            this.btnCompile.TabIndex = 1;
            this.btnCompile.Text = "Compile";
            this.btnCompile.UseVisualStyleBackColor = true;
            this.btnCompile.Click += new System.EventHandler(this.btnCompile_Click);
            // 
            // btnClose
            // 
            this.btnClose.Location = new System.Drawing.Point(389, 518);
            this.btnClose.Name = "btnClose";
            this.btnClose.Size = new System.Drawing.Size(85, 23);
            this.btnClose.TabIndex = 2;
            this.btnClose.Text = "Close";
            this.btnClose.UseVisualStyleBackColor = true;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);
            // 
            // richCompileMessages
            // 
            this.richCompileMessages.Location = new System.Drawing.Point(12, 489);
            this.richCompileMessages.Name = "richCompileMessages";
            this.richCompileMessages.Size = new System.Drawing.Size(371, 52);
            this.richCompileMessages.TabIndex = 5;
            this.richCompileMessages.Text = "";
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.textSaveFileName);
            this.groupBox1.Controls.Add(this.textMemAdress);
            this.groupBox1.Controls.Add(this.checkFile);
            this.groupBox1.Controls.Add(this.checkMemory);
            this.groupBox1.Location = new System.Drawing.Point(388, 35);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(132, 165);
            this.groupBox1.TabIndex = 6;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Compile to:";
            // 
            // textSaveFileName
            // 
            this.textSaveFileName.Enabled = false;
            this.textSaveFileName.Location = new System.Drawing.Point(7, 113);
            this.textSaveFileName.Name = "textSaveFileName";
            this.textSaveFileName.Size = new System.Drawing.Size(119, 20);
            this.textSaveFileName.TabIndex = 3;
            // 
            // textMemAdress
            // 
            this.textMemAdress.Location = new System.Drawing.Point(7, 64);
            this.textMemAdress.Name = "textMemAdress";
            this.textMemAdress.Size = new System.Drawing.Size(119, 20);
            this.textMemAdress.TabIndex = 2;
            this.textMemAdress.Text = "#9C40";
            // 
            // checkFile
            // 
            this.checkFile.AutoSize = true;
            this.checkFile.Location = new System.Drawing.Point(7, 90);
            this.checkFile.Name = "checkFile";
            this.checkFile.Size = new System.Drawing.Size(42, 17);
            this.checkFile.TabIndex = 1;
            this.checkFile.Text = "File";
            this.checkFile.UseVisualStyleBackColor = true;
            // 
            // checkMemory
            // 
            this.checkMemory.AutoSize = true;
            this.checkMemory.Checked = true;
            this.checkMemory.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkMemory.Location = new System.Drawing.Point(7, 40);
            this.checkMemory.Name = "checkMemory";
            this.checkMemory.Size = new System.Drawing.Size(63, 17);
            this.checkMemory.TabIndex = 0;
            this.checkMemory.Text = "Memory";
            this.checkMemory.UseVisualStyleBackColor = true;
            this.checkMemory.CheckedChanged += new System.EventHandler(this.checkMemory_CheckedChanged);
            // 
            // Assembler
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(532, 548);
            this.Controls.Add(this.groupBox1);
            this.Controls.Add(this.richCompileMessages);
            this.Controls.Add(this.btnClose);
            this.Controls.Add(this.btnCompile);
            this.Controls.Add(this.txtAsm);
            this.Name = "Assembler";
            this.Text = "Assembler";
            this.groupBox1.ResumeLayout(false);
            this.groupBox1.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.RichTextBox txtAsm;
        private System.Windows.Forms.Button btnCompile;
        private System.Windows.Forms.Button btnClose;
        private System.Windows.Forms.RichTextBox richCompileMessages;
        private System.Windows.Forms.GroupBox groupBox1;
        private System.Windows.Forms.CheckBox checkFile;
        private System.Windows.Forms.CheckBox checkMemory;
        private System.Windows.Forms.TextBox textSaveFileName;
        private System.Windows.Forms.TextBox textMemAdress;
    }
}