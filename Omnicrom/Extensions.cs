using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Management;
using System.Text;
using System.Windows.Forms;
using Telerik.WinControls.UI;

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
        public static RichTextBox miglogbox;
        private static readonly Color _timestampcolor = Color.DimGray;
        private static readonly Color _logtextcolor = Color.LightGray;

        public static void Log(string text)
        {
            if (string.IsNullOrEmpty(text))
                return;

            try
            {
                SendMessageText(logbox, text, _logtextcolor);
                Functions.WriteLog(text);
            }
            catch (Exception ex) { Log($"Error from Omnicrom Log: Exception {ex.Message} Trace {ex.StackTrace}"); }  
        }

        public static void MigLog(string text)
        {
            if (string.IsNullOrEmpty(text))
                return;

            try { SendMessageText(miglogbox, text, _logtextcolor); }
            catch (Exception ex) { Log($"Error from Migration Log: Exception {ex.Message} Trace {ex.StackTrace}"); }
        }

        public static void AppLog(string text)
        {
            if (string.IsNullOrEmpty(text))
                return;

            try { SendMessageText(applogbox, text, _logtextcolor); }
            catch (Exception ex) { Log($"Error from App Log: Exception {ex.Message} Trace {ex.StackTrace}"); }
        }

        public static void SendMessageText(RichTextBox box, string text, Color color)
        {
            box.InvokeIfRequired(() =>
            {
                box = GetColoredTimeStamp(box);
                box.AppendText(text + "\r\n");
                GetColoredSubString(text, box);
                box.Refresh();
            });
        }

        private static RichTextBox GetColoredTimeStamp(RichTextBox box)
        {
            RichTextBox newbox = new RichTextBox();

            string timestamp = GetTimeStamp();

            box.SelectionStart = box.TextLength;
            box.SelectionLength = 0;

            box.SelectionColor = _timestampcolor;
            box.AppendText(timestamp);

            box.SelectionStart = box.TextLength;
            box.SelectionLength = 0;

            box.SelectionColor = _logtextcolor;

            newbox = box;
            return newbox;
        }

        private static List<string> negwords { get; set; }
        private static List<string> poswords { get; set; }
        private static string completetext;

      private static void GetColoredSubString(string text, RichTextBox box)
        {
            box.Undo();
            completetext += text;
            negwords = new List<string>();
            poswords = new List<string>();

            //var boxtext = box.Text;
            char[] spearator = { ' ', ',', '.', '?', '!', '/', ';', ':', '(', ')', '[', ']', '{', '}' };
            string[] strsplit = text.Split(spearator);
            //string[] strsplit = completetext.Split(spearator);
            //int wordcount = strsplit.Length;

            Color poscolor = Color.SeaGreen;
            Color negcolor = Color.IndianRed;
            Color normcolor = Color.WhiteSmoke;

            box.SelectionStart = box.TextLength;
            box.SelectionLength = box.Text.Length;
            box.SelectionColor = normcolor;

            foreach (string s in strsplit)
            {
                Color temp = normcolor;
                int wstart = 0;
                int strlen = 0;
                var word = s.ToLower();

                if (Global.ErrorStrings.Contains(word))
                {
                    wstart = box.Text.IndexOf(s);
                    strlen = s.Length;

                    temp = negcolor;
                }
                else if (Global.SuccessStrings.Contains(word))
                {
                    wstart = box.Text.IndexOf(s);
                    strlen = s.Length;
                  
                    temp = poscolor;
                }

                box.Select(wstart, strlen);
                box.SelectionColor = temp;
                //box.AppendText(s);
            }

            //box.AppendText("\r\n");
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
        public static RadLabel testlabel;

        public static void UpdateLabel(string text)
        {
            UpdateLabelProperty(testlabel, text, Color.White);
        }

        public static void UpdateLabelProperty(RadLabel label, string text, Color color)
        {
            try
            {
                label.InvokeIfRequired(() =>
                {
                    label.ForeColor = color;
                    label.Text = text;
                });
            }
            catch (Exception ex) { RichTextBoxExtensions.Log($"Exception {ex.Message} Trace {ex.StackTrace}"); }
        }
    }

    public static class LogManager
    {
        public static bool WriteLog(string message)
        {
            try
            {
                var name = $"{DateTime.Now:MMddyyyy}_OmniLog.txt";
                string path = Path.Combine(Global.MyDocuments, name);
                Global.OmnicromExternalLogPath = Global.MyDocuments;

                if (File.Exists(path))
                {
                    using (StreamWriter writer = File.AppendText(path))
                    {
                        writer.WriteLine($"{DateTime.Now} : {message}");
                    }
                }
                else
                {
                    using (StreamWriter writer = File.CreateText(path))
                    {
                        writer.WriteLine($"{DateTime.Now} : {message}");
                    }
                }
            }
            catch (Exception ex) { RichTextBoxExtensions.Log($"Error at WriteLog: Exception {ex.Message} Trace {ex.StackTrace}"); }

            return true;
        }
    }

    /// <summary>
    /// Define format of time stamp in <see cref="TraceLogger"/> and <see cref="TraceFileWriter"/>
    /// </summary>
    public enum TraceTimeStampStyle
    {
        /// <summary>
        /// Date time string as defined in TimeStampFormat property
        /// </summary>
        DateTimeString,
        /// <summary>
        /// Date time as tick count in large integer
        /// </summary>
        TickCount
    }

    /// <summary>
    /// TraceLogger write() / writeline() function callback to write message to target output.
    /// </summary>
    /// <seealso cref="TraceLoggerMessageReceived"/>
    public delegate void TraceLoggerWrite(string message);
    /// <summary>
    /// TraceLogger flush() function callback
    /// </summary>
    public delegate void TraceLoggerFlush();
    /// <summary>
    /// TraceLogger write() / writeline() function callback when message is received.
    /// </summary>
    /// <param name="message"></param>
    /// <seealso cref="TraceLoggerWrite"/>
    public delegate void TraceLoggerMessageReceived(ref string message);

    /// <summary>
    /// A simple implementation of <see cref="TraceListener"/> for text based logging
    /// </summary>
    /// <seealso cref="TraceListener"/>
    public class TraceLogger : TraceListener
    {
        TraceLoggerWrite OnWrite;
        TraceLoggerFlush OnFlush;
        TraceLoggerMessageReceived OnMessageReceived;
        private bool _EnableTracer;

        private const string NewLineDelimiter = "\r\n";
        private bool IsNewLine;

        private void DummyWrite(string message) { }
        private void DummyFlush() { }
        private void DummyMsgReceived(ref string message) { }

        /// <summary>
        /// Initializes a new instance of <see cref="TraceLogger"/>.
        /// </summary>
        /// <param name="writeCallback">Callback to TraceListener write() / writeline() function. Value can be null.</param>
        /// <param name="flushCallback">Callback to TraceListener flush() function. Value can be null.</param>
        /// <param name="messageReceivedCallback">Callback to TraceListener write() / writeline() function when message is received, can be used for message filtering. Value can be null.</param>
        public TraceLogger(TraceLoggerWrite writeCallback, TraceLoggerFlush flushCallback,
            TraceLoggerMessageReceived messageReceivedCallback = null)
        {
            Enabled = true;
            ShowTimeStamp = false;
            OnWrite = writeCallback ?? DummyWrite;
            OnFlush = flushCallback ?? DummyFlush;
            OnMessageReceived = messageReceivedCallback ?? DummyMsgReceived;
            IsNewLine = true;

            DateTimeFormatInfo timeFormat = CultureInfo.CurrentCulture.DateTimeFormat;
            TimeStampFormat = timeFormat.ShortDatePattern + " " + timeFormat.LongTimePattern;
            TimeStampFormat = TimeStampFormat.Replace("ss", "ss.fff"); //Include miliseconds
        }

        /// <summary>
        /// Received message from Trace source.
        /// </summary>CRLF
        /// <param name="message">Message received.</param>
        public override void Write(string message)
        {
            OnMessageReceived(ref message);
            if (string.IsNullOrEmpty(message)) return;

            message = ParseMessage(message);
            OnWrite(message);
            IsNewLine = false;
        }

        /// <summary>
        /// Receive message from Trace source followed by a line terminator.
        /// </summary>
        /// <param name="message">Message received.</param>
        public override void WriteLine(string message)
        {
            OnMessageReceived(ref message);
            if (string.IsNullOrEmpty(message)) return;

            message = ParseMessage(message);
            OnWrite(message + NewLineDelimiter);
            IsNewLine = true;
        }

        //ToDo: Add property, unified CRLF
        private string ParseMessage(string message)
        {
            string dateTimeStr = ShowTimeStamp ? AppendDateTime() : string.Empty;
            string result = IsNewLine ? dateTimeStr : string.Empty;

            if (message.Contains("\r") || message.Contains("\n"))
            {
                //Unified CR, CRLF, LFCR, LF
                message = message.Replace("\n", "\r");
                message = message.Replace("\r\r", "\r");

                string newLineFiller = new string(' ', dateTimeStr.Length);
                string[] multiLineMessage = message.Split('\r');
                result += multiLineMessage[0].Trim() + NewLineDelimiter;
                foreach (string msg in multiLineMessage.Skip(1))
                    result += newLineFiller + msg.Trim() + NewLineDelimiter;

                result = result.TrimEnd();
            }
            else result += message;
            return result;
        }

        private string AppendDateTime()
        {
            switch (TimeStampStyle)
            {
                case TraceTimeStampStyle.DateTimeString: return "[" + DateTime.Now.ToString(TimeStampFormat) + "] ";
                case TraceTimeStampStyle.TickCount: return "[" + DateTime.Now.Ticks.ToString() + "] ";
            }
            return "-";
        }

        /// <summary>
        /// Flushes trace buffer.
        /// </summary>
        public override void Flush() { OnFlush(); }

        /// <summary>
        /// Enable / Disable TraceListener.
        /// </summary>
        /// <value>
        /// <para><c>True</c> = Monitor trace listener activites.</para>
        /// <para><c>False</c> = Suspend trace listener activities.</para>
        /// </value>
        public bool Enabled
        {
            get
            {
                return _EnableTracer;
            }
            set
            {
                if (_EnableTracer == value) return; //Avoid multiple set / clear

                _EnableTracer = value;
                if (_EnableTracer == true)
                    Trace.Listeners.Add(this);
                else
                    Trace.Listeners.Remove(this);
            }
        }

        /// <summary>
        /// Enable / Disable time stamp in log.
        /// </summary>
        public bool ShowTimeStamp { get; set; }

        /// <summary>
        /// Define date time display format when <see cref="TimeStampStyle"/> set as <see cref="TraceTimeStampStyle.DateTimeString"/>  . Use default format if undefined.
        /// Time stamp is append in front of message when <see cref="ShowTimeStamp"/> is enabled.
        /// </summary>
        public string TimeStampFormat { get; set; }

        /// <summary>
        /// Define time stamp style.
        /// </summary>
        /// <seealso cref="TimeStampFormat"/>
        public TraceTimeStampStyle TimeStampStyle { get; set; } = TraceTimeStampStyle.DateTimeString;

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
