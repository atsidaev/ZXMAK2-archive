namespace ZXMAK2.Hardware.WinForms.General
{
    partial class FormMemory
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
            this.dataPanel1 = new ZXMAK2.Hardware.WinForms.General.DataPanel();
            this.SuspendLayout();
            // 
            // dataPanel1
            // 
            this.dataPanel1.ColCount = 8;
            this.dataPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dataPanel1.Font = new System.Drawing.Font("Courier New", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.dataPanel1.Location = new System.Drawing.Point(0, 0);
            this.dataPanel1.Name = "dataPanel1";
            this.dataPanel1.Size = new System.Drawing.Size(450, 151);
            this.dataPanel1.TabIndex = 0;
            this.dataPanel1.Text = "dataPanel1";
            this.dataPanel1.TopAddress = ((ushort)(0));
            // 
            // FormMemory
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(450, 151);
            this.Controls.Add(this.dataPanel1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.ForeColor = System.Drawing.Color.Black;
            this.Name = "FormMemory";
            this.ShowIcon = false;
            this.Text = "Memory";
            this.ResumeLayout(false);

        }

        #endregion

        private DataPanel dataPanel1;
    }
}