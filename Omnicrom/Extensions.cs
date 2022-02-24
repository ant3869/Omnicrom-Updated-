using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Management;
using System.Text;
using System.Windows.Forms;

namespace Omnicrom
{


    public static class ControlExtensions
    {
        public static void InvokeIfRequired(this ISynchronizeInvoke obj, MethodInvoker action)
        {
            if (obj.InvokeRequired)
            {
                var args = new object[0];
                obj.Invoke(action, args);
            }
            else
                action();
        }
    }

    public static class RichTextBoxExtensions
    {
        public static RichTextBox logbox;
        public static RichTextBox applogbox;
        private static readonly Color _timestampcolor = Color.DimGray;
        private static readonly Color _logtextcolor = Color.LightGray;

        public static void Log(string text)
        {
            if (string.IsNullOrEmpty(text))
                return;

            SendMessageText(logbox, text, _logtextcolor);
            Functions.WriteLog(text);
        }

        public static void AppLog(string text)
        {
            SendMessageText(applogbox, text, _logtextcolor);
        }

        public static void SendMessageText(RichTextBox box, string text)
        {
            SendMessageText(box, text, _logtextcolor);
        }

        public static void SendMessageText(RichTextBox box, string text, Color color)
        {
            string timestamp = GetTimeStamp();

            box.InvokeIfRequired(() =>
            {
                box.SelectionStart = box.TextLength;
                box.SelectionLength = 0;

                box.SelectionColor = _timestampcolor;
                box.AppendText(timestamp);
                box.ForeColor = box.ForeColor;

                //KeyWordColorChanger(box, text);

                var check = text.ToLower();

                if (check.Contains("error") || check.Contains("fail") || check.Contains("unable"))
                    color = Color.IndianRed;
                else if (check.Contains("success") || check.Contains("complete"))
                    color = Color.LightSeaGreen;

                box.SelectionColor = color;

                box.AppendText(text + "\n");
                box.SelectionColor = box.ForeColor;
            });
        }

        private static void KeyWordColorChanger(RichTextBox box, string text)
        {
            var color = Color.White;

            box.SelectionStart = box.TextLength;
            box.SelectionLength = 0;

 

            box.SelectionColor = color;
            box.AppendText(text + "\n");
            box.ForeColor = box.ForeColor;

            return;
        }

        private static string GetTimeStamp()
        {
            string basetime = DateTime.Now.ToString("h:mm tt");
            string timestamp = "[" + basetime + "]  ";

            return timestamp;
        }
    }

    public static class LabelExtensions
    {
        public static void UpdateLabelProperty(Label label, string text)
        {
            label.ForeColor = Color.White;
            label.Text = text;
        }
        public static void UpdateLabelProperty(Label label, string text, Color color)
        {
            label.ForeColor = color;
            label.Text = text;
        }
    }



    public static class ExecutableExtensions
    {
        public static void KillProcessAndChildren(int pid)
        {
            using (var searcher = new ManagementObjectSearcher("Select * From Win32_Process Where ParentProcessID=" + pid))
            {
                var moc = searcher.Get();

                foreach (ManagementObject mo in moc)
                    KillProcessAndChildren(Convert.ToInt32(mo["ProcessID"]));
                try
                {
                    var proc = Process.GetProcessById(pid);
                    proc.Kill();
                }
                catch (Exception e) { _ = e.Message; }
            }
        }
    }
}
