using System;
using System.Collections.Concurrent;
using System.IO;
using System.Windows.Forms;

namespace Omnicrom
{
    [Docking(DockingBehavior.Ask)]
    public partial class LogBox : UserControl
    {
        private readonly ConcurrentQueue<string> PendingLog = new ConcurrentQueue<string>();

        public LogBox()
        {
            InitializeComponent();
        }

        private void tmrLog_Tick(object sender, EventArgs e) { this.ProcessPendingLog(); }

        private void ProcessPendingLog()
        {
            if (!this.Disposing && !this.IsDisposed && this.IsHandleCreated)
                try
                {
                    if (!this.PendingLog.IsEmpty)
                    {
                        while (this.PendingLog.TryDequeue(out string item))
                        {
                            RichTextBoxExtensions.Log(item);
                        }
                    }
                }
                catch (Exception e) { MessageBox.Show(string.Format("Exception {0} Trace {1}", e.Message, e.StackTrace)); }
        }

        public void Log(string text) { this.PendingLog.Enqueue(text); }
    }

 

    partial class LogBox
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            this.tmrLog = new System.Windows.Forms.Timer(this.components);
            this.txtLog = new System.Windows.Forms.TextBox();
            this.SuspendLayout();
            // 
            // tmrLog
            // 
            this.tmrLog.Enabled = true;
            this.tmrLog.Tick += new System.EventHandler(this.tmrLog_Tick);
            // 
            // txtLog
            // 
            this.txtLog.Dock = System.Windows.Forms.DockStyle.Fill;
            this.txtLog.Location = new System.Drawing.Point(0, 0);
            this.txtLog.Multiline = true;
            this.txtLog.Name = "txtLog";
            this.txtLog.ReadOnly = true;
            this.txtLog.ScrollBars = System.Windows.Forms.ScrollBars.Both;
            this.txtLog.Size = new System.Drawing.Size(200, 200);
            this.txtLog.TabIndex = 0;
            // 
            // LogBox
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.txtLog);
            this.Name = "LogBox";
            this.Size = new System.Drawing.Size(200, 200);
            this.ResumeLayout(false);
            this.PerformLayout();
        }

        #endregion

        private System.Windows.Forms.Timer tmrLog;
        private System.Windows.Forms.TextBox txtLog;
    }
}
