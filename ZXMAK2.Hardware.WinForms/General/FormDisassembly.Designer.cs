namespace ZXMAK2.Hardware.WinForms.General
{
    partial class FormDisassembly
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
            this.dasmPanel1 = new ZXMAK2.Hardware.WinForms.General.DasmPanel();
            this.SuspendLayout();
            // 
            // dasmPanel1
            // 
            this.dasmPanel1.ActiveAddress = ((ushort)(0));
            this.dasmPanel1.BreakpointColor = System.Drawing.Color.Red;
            this.dasmPanel1.BreakpointForeColor = System.Drawing.Color.Black;
            this.dasmPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.dasmPanel1.Font = new System.Drawing.Font("Courier New", 13F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.dasmPanel1.Location = new System.Drawing.Point(0, 0);
            this.dasmPanel1.Name = "dasmPanel1";
            this.dasmPanel1.Size = new System.Drawing.Size(525, 273);
            this.dasmPanel1.TabIndex = 0;
            this.dasmPanel1.Text = "dasmPanel1";
            this.dasmPanel1.TopAddress = ((ushort)(0));
            // 
            // FormDisassembly
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.White;
            this.ClientSize = new System.Drawing.Size(525, 273);
            this.Controls.Add(this.dasmPanel1);
            this.ForeColor = System.Drawing.Color.Black;
            this.Name = "FormDisassembly";
            this.ShowIcon = false;
            this.Text = "Disassembly";
            this.ResumeLayout(false);

        }

        #endregion

        private DasmPanel dasmPanel1;
    }
}