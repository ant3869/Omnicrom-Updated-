namespace Omnicrom.Forms
{
    partial class DialogBoxForm
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
            this.office2019DarkTheme1 = new Telerik.WinControls.Themes.Office2019DarkTheme();
            this.visualStudio2012DarkTheme1 = new Telerik.WinControls.Themes.VisualStudio2012DarkTheme();
            this.office2013DarkTheme1 = new Telerik.WinControls.Themes.Office2013DarkTheme();
            this.crystalDarkTheme1 = new Telerik.WinControls.Themes.CrystalDarkTheme();
            this.Label_Dialog_Message = new Telerik.WinControls.UI.RadLabel();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.Button_Dialog_Cancel = new Telerik.WinControls.UI.RadButton();
            this.Button_Dialog_OK = new Telerik.WinControls.UI.RadButton();
            ((System.ComponentModel.ISupportInitialize)(this.Label_Dialog_Message)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Button_Dialog_Cancel)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.Button_Dialog_OK)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this)).BeginInit();
            this.SuspendLayout();
            // 
            // Label_Dialog_Message
            // 
            this.Label_Dialog_Message.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
            this.Label_Dialog_Message.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(6)))), ((int)(((byte)(133)))), ((int)(((byte)(208)))));
            this.Label_Dialog_Message.Location = new System.Drawing.Point(63, 38);
            this.Label_Dialog_Message.Name = "Label_Dialog_Message";
            this.Label_Dialog_Message.Size = new System.Drawing.Size(274, 25);
            this.Label_Dialog_Message.TabIndex = 1;
            this.Label_Dialog_Message.Text = "Are you sure you want to shutdown?";
            // 
            // pictureBox1
            // 
            this.pictureBox1.Image = global::Omnicrom.Properties.Resources.Copy__Help_icon_icon;
            this.pictureBox1.Location = new System.Drawing.Point(12, 26);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(49, 50);
            this.pictureBox1.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBox1.TabIndex = 0;
            this.pictureBox1.TabStop = false;
            // 
            // Button_Dialog_Cancel
            // 
            this.Button_Dialog_Cancel.Cursor = System.Windows.Forms.Cursors.Hand;
            this.Button_Dialog_Cancel.DialogResult = System.Windows.Forms.DialogResult.Cancel;
            this.Button_Dialog_Cancel.DisplayStyle = Telerik.WinControls.DisplayStyle.Text;
            this.Button_Dialog_Cancel.ForeColor = System.Drawing.Color.IndianRed;
            this.Button_Dialog_Cancel.Location = new System.Drawing.Point(13, 100);
            this.Button_Dialog_Cancel.Name = "Button_Dialog_Cancel";
            this.Button_Dialog_Cancel.Size = new System.Drawing.Size(110, 33);
            this.Button_Dialog_Cancel.TabIndex = 2;
            this.Button_Dialog_Cancel.Text = "&Cancel";
            this.Button_Dialog_Cancel.ThemeName = "CrystalDark";
            this.Button_Dialog_Cancel.Click += new System.EventHandler(this.Button_Dialog_Cancel_Click);
            this.Button_Dialog_Cancel.MouseLeave += new System.EventHandler(this.Button_Dialog_Cancel_MouseLeave);
            this.Button_Dialog_Cancel.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Button_Dialog_Cancel_MouseMove);
            ((Telerik.WinControls.UI.RadButtonElement)(this.Button_Dialog_Cancel.GetChildAt(0))).DisplayStyle = Telerik.WinControls.DisplayStyle.Text;
            ((Telerik.WinControls.UI.RadButtonElement)(this.Button_Dialog_Cancel.GetChildAt(0))).Text = "&Cancel";
            ((Telerik.WinControls.UI.RadButtonElement)(this.Button_Dialog_Cancel.GetChildAt(0))).CustomFontSize = 12F;
            // 
            // Button_Dialog_OK
            // 
            this.Button_Dialog_OK.Cursor = System.Windows.Forms.Cursors.Hand;
            this.Button_Dialog_OK.DialogResult = System.Windows.Forms.DialogResult.OK;
            this.Button_Dialog_OK.DisplayStyle = Telerik.WinControls.DisplayStyle.Text;
            this.Button_Dialog_OK.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F);
            this.Button_Dialog_OK.ForeColor = System.Drawing.Color.PaleGreen;
            this.Button_Dialog_OK.Location = new System.Drawing.Point(270, 100);
            this.Button_Dialog_OK.Name = "Button_Dialog_OK";
            // 
            // 
            // 
            this.Button_Dialog_OK.RootElement.BorderHighlightColor = System.Drawing.Color.FromArgb(((int)(((byte)(152)))), ((int)(((byte)(251)))), ((int)(((byte)(136)))));
            this.Button_Dialog_OK.RootElement.CustomFontSize = 12F;
            this.Button_Dialog_OK.RootElement.EnableBorderHighlight = true;
            this.Button_Dialog_OK.Size = new System.Drawing.Size(110, 33);
            this.Button_Dialog_OK.TabIndex = 3;
            this.Button_Dialog_OK.Text = "&Ok";
            this.Button_Dialog_OK.ThemeName = "CrystalDark";
            this.Button_Dialog_OK.Click += new System.EventHandler(this.Button_Dialog_OK_Click);
            this.Button_Dialog_OK.MouseLeave += new System.EventHandler(this.Button_Dialog_OK_MouseLeave);
            this.Button_Dialog_OK.MouseMove += new System.Windows.Forms.MouseEventHandler(this.Button_Dialog_OK_MouseMove);
            ((Telerik.WinControls.UI.RadButtonElement)(this.Button_Dialog_OK.GetChildAt(0))).DisplayStyle = Telerik.WinControls.DisplayStyle.Text;
            ((Telerik.WinControls.UI.RadButtonElement)(this.Button_Dialog_OK.GetChildAt(0))).Text = "&Ok";
            ((Telerik.WinControls.UI.RadButtonElement)(this.Button_Dialog_OK.GetChildAt(0))).EnableBorderHighlight = true;
            ((Telerik.WinControls.UI.RadButtonElement)(this.Button_Dialog_OK.GetChildAt(0))).BorderHighlightColor = System.Drawing.Color.FromArgb(((int)(((byte)(152)))), ((int)(((byte)(251)))), ((int)(((byte)(136)))));
            ((Telerik.WinControls.UI.RadButtonElement)(this.Button_Dialog_OK.GetChildAt(0))).CustomFontSize = 12F;
            ((Telerik.WinControls.Primitives.BorderPrimitive)(this.Button_Dialog_OK.GetChildAt(0).GetChildAt(2))).TopColor = System.Drawing.SystemColors.ControlDark;
            ((Telerik.WinControls.Primitives.BorderPrimitive)(this.Button_Dialog_OK.GetChildAt(0).GetChildAt(2))).SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.HighQuality;
            ((Telerik.WinControls.Primitives.BorderPrimitive)(this.Button_Dialog_OK.GetChildAt(0).GetChildAt(2))).CanFocus = true;
            // 
            // DialogBoxForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(392, 145);
            this.ControlBox = false;
            this.Controls.Add(this.Button_Dialog_OK);
            this.Controls.Add(this.Button_Dialog_Cancel);
            this.Controls.Add(this.Label_Dialog_Message);
            this.Controls.Add(this.pictureBox1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximumSize = new System.Drawing.Size(400, 175);
            this.MinimumSize = new System.Drawing.Size(400, 175);
            this.Name = "DialogBoxForm";
            // 
            // 
            // 
            this.RootElement.ApplyShapeToControl = true;
            this.RootElement.MaxSize = new System.Drawing.Size(400, 175);
            this.ShowIcon = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "DialogBoxForm";
            this.ThemeName = "Office2019Dark";
            this.TopMost = true;
            ((System.ComponentModel.ISupportInitialize)(this.Label_Dialog_Message)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Button_Dialog_Cancel)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.Button_Dialog_OK)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private Telerik.WinControls.Themes.Office2019DarkTheme office2019DarkTheme1;
        private Telerik.WinControls.Themes.VisualStudio2012DarkTheme visualStudio2012DarkTheme1;
        private Telerik.WinControls.Themes.Office2013DarkTheme office2013DarkTheme1;
        private Telerik.WinControls.Themes.CrystalDarkTheme crystalDarkTheme1;
        private System.Windows.Forms.PictureBox pictureBox1;
        private Telerik.WinControls.UI.RadLabel Label_Dialog_Message;
        private Telerik.WinControls.UI.RadButton Button_Dialog_Cancel;
        private Telerik.WinControls.UI.RadButton Button_Dialog_OK;
    }
}
