using static Omnicrom.Properties.Resources;
using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Media;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace Omnicrom
{
    public class EffectManager
    {
        public bool isBlinking { get; set; }
        public int RunTime { get; set; }
        public async Task SoftBlink(Control ctrl, Color c2, short CycleTime_ms = 1800, bool BkClr = false)
        {
            var sw = new Stopwatch();
            short halfCycle = (short)Math.Round(CycleTime_ms * 0.5);
            Color c1 = Color.FromArgb(30, 30, 30);
            sw.Reset();
            sw.Start();

            while (sw.ElapsedMilliseconds < 2000)
            {
                isBlinking = true;
                await Task.Delay(1);
                var n = sw.ElapsedMilliseconds % CycleTime_ms;
                var per = (double)Math.Abs(n - halfCycle) / halfCycle;
                var red = (short)Math.Round((c2.R - c1.R) * per) + c1.R;
                var grn = (short)Math.Round((c2.G - c1.G) * per) + c1.G;
                var blw = (short)Math.Round((c2.B - c1.B) * per) + c1.B;
                var clr = Color.FromArgb(red, grn, blw);
                if (BkClr) ctrl.BackColor = clr; else ctrl.ForeColor = clr;
            }

            sw.Stop();
            isBlinking = false;
            ctrl.ForeColor = c2;
        }


        public static void PlaySound(string soundque)
        {
            if (!Global.SoundOn)
                return;

            try
            {
                SoundPlayer audioplayer = new SoundPlayer();

                switch (soundque)
                {
                    case "common": audioplayer.Stream = Beep1; break;
                    case "common2": audioplayer.Stream = Countdown; break;
                    case "Finished": audioplayer.Stream = LoadScript; break;
                    case "Failure": audioplayer.Stream = LoadScriptError; break;
                    case "Warning": audioplayer.Stream = Warn1; break;
                    case "Error": audioplayer.Stream = Error1; break;
                }

                audioplayer.Stop();
                audioplayer.Play();
            }
            catch { }
        }
    }
}
