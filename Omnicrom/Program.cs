using Omnicrom.Forms;
using System;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace Omnicrom
{
    static class Program
    {
        public static MainForm mainform = null;
        public static SplashScreenForm splashform = null;

        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            //show splash
            Thread splashThread = new Thread(
            new ThreadStart(delegate
            {
                splashform = new SplashScreenForm();
                Application.Run(splashform);
            }
            ));

            splashThread.SetApartmentState(ApartmentState.STA);
            splashThread.Start();

            //run form - time taking operation
            mainform = new MainForm();
            mainform.Load += new EventHandler(mainform_Load);
            Application.Run(mainform);
        }

        static void mainform_Load(object sender, EventArgs e)
        {
            Program.splashform.Label_nexco.InvokeIfRequired(() =>
            { Program.splashform.Label_nexco.Text = "Finished."; });

            //close splash
            if (splashform == null)
                return;

            splashform.Invoke(new Action(splashform.Close));
            splashform.Dispose();
            splashform = null;
        }
    }
}