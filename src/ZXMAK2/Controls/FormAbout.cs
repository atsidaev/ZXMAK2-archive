using System;
using System.Windows.Forms;


namespace ZXMAK2.Controls
{
	public class FormAbout : Form
    {
        #region Windows Form Designer generated code

        private System.Windows.Forms.Label labelVersionText;
        private System.Windows.Forms.Label labelCopyright;
        private System.Windows.Forms.Label labelAmstrad;
        private System.Windows.Forms.Button buttonOk;
        private System.Windows.Forms.Button buttonUpdate;
        private System.Windows.Forms.Label labelLogo;
        private System.Windows.Forms.PictureBox pctLogo;

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.buttonOk = new System.Windows.Forms.Button();
            this.buttonUpdate = new System.Windows.Forms.Button();
            this.labelVersionText = new System.Windows.Forms.Label();
            this.labelCopyright = new System.Windows.Forms.Label();
            this.labelAmstrad = new System.Windows.Forms.Label();
            this.labelLogo = new System.Windows.Forms.Label();
            this.pctLogo = new System.Windows.Forms.PictureBox();
            ((System.ComponentModel.ISupportInitialize)(this.pctLogo)).BeginInit();
            this.SuspendLayout();
            // 
            // buttonOk
            // 
            this.buttonOk.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonOk.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.buttonOk.Location = new System.Drawing.Point(406, 12);
            this.buttonOk.Name = "buttonOk";
            this.buttonOk.Size = new System.Drawing.Size(75, 23);
            this.buttonOk.TabIndex = 0;
            this.buttonOk.Text = "OK";
            this.buttonOk.UseVisualStyleBackColor = true;
            // 
            // buttonUpdate
            // 
            this.buttonUpdate.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.buttonUpdate.Location = new System.Drawing.Point(406, 41);
            this.buttonUpdate.Name = "buttonUpdate";
            this.buttonUpdate.Size = new System.Drawing.Size(75, 23);
            this.buttonUpdate.TabIndex = 1;
            this.buttonUpdate.Text = "Update";
            this.buttonUpdate.UseVisualStyleBackColor = true;
            this.buttonUpdate.Visible = false;
            // 
            // labelVersionText
            // 
            this.labelVersionText.AutoSize = true;
            this.labelVersionText.Location = new System.Drawing.Point(53, 88);
            this.labelVersionText.Name = "labelVersionText";
            this.labelVersionText.Size = new System.Drawing.Size(114, 13);
            this.labelVersionText.TabIndex = 3;
            this.labelVersionText.Text = "Version 0.0.0.0 [beta]";
            // 
            // labelCopyright
            // 
            this.labelCopyright.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.labelCopyright.AutoSize = true;
            this.labelCopyright.Location = new System.Drawing.Point(53, 112);
            this.labelCopyright.Name = "labelCopyright";
            this.labelCopyright.Size = new System.Drawing.Size(218, 26);
            this.labelCopyright.TabIndex = 4;
            this.labelCopyright.Text = "Copyright © 2001-2012 Alexander Makeev.\nAll rights reserved.";
            // 
            // labelAmstrad
            // 
            this.labelAmstrad.Anchor = ((System.Windows.Forms.AnchorStyles)(((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Left)
                        | System.Windows.Forms.AnchorStyles.Right)));
            this.labelAmstrad.AutoSize = true;
            this.labelAmstrad.Location = new System.Drawing.Point(53, 150);
            this.labelAmstrad.Name = "labelAmstrad";
            this.labelAmstrad.Size = new System.Drawing.Size(428, 39);
            this.labelAmstrad.TabIndex = 6;
            this.labelAmstrad.Text = "Portions of this software are copyright © Amstrad Consumer Electronics plc. Amstr" +
                "ad\nhave kindly given their permission for the redistribution of their copyrighte" +
                "d material but\nretain that copyright.";
            // 
            // labelLogo
            // 
            this.labelLogo.AutoSize = true;
            this.labelLogo.Font = new System.Drawing.Font("Courier New", 21.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.labelLogo.Location = new System.Drawing.Point(90, 31);
            this.labelLogo.Name = "labelLogo";
            this.labelLogo.Size = new System.Drawing.Size(117, 33);
            this.labelLogo.TabIndex = 9;
            this.labelLogo.Text = "ZXMAK2";
            // 
            // pctLogo
            // 
            this.pctLogo.Location = new System.Drawing.Point(12, 12);
            this.pctLogo.Name = "pctLogo";
            this.pctLogo.Size = new System.Drawing.Size(72, 73);
            this.pctLogo.SizeMode = System.Windows.Forms.PictureBoxSizeMode.CenterImage;
            this.pctLogo.TabIndex = 10;
            this.pctLogo.TabStop = false;
            // 
            // FormAbout
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.Window;
            this.ClientSize = new System.Drawing.Size(497, 207);
            this.Controls.Add(this.pctLogo);
            this.Controls.Add(this.labelLogo);
            this.Controls.Add(this.labelAmstrad);
            this.Controls.Add(this.labelCopyright);
            this.Controls.Add(this.labelVersionText);
            this.Controls.Add(this.buttonUpdate);
            this.Controls.Add(this.buttonOk);
            this.Font = new System.Drawing.Font("Tahoma", 8.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(204)));
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "FormAbout";
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "About ZXMAK2";
            ((System.ComponentModel.ISupportInitialize)(this.pctLogo)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        
        public FormAbout()
		{
			InitializeComponent();
            using(System.Drawing.Icon icon = Utils.GetAppIcon())
                pctLogo.Image = icon.ToBitmap();
			labelVersionText.Text = labelVersionText.Text.Replace("0.0.0.0", Application.ProductVersion);
		}

	}
}