using System;
using System.Windows.Forms;

namespace Omnicrom.Forms
{
    public partial class SplashScreenForm : Telerik.WinControls.UI.RadForm
    {
        public SplashScreenForm()
        {
            InitializeComponent();
            InitializeForm();
        }

        private void InitializeForm()
        {
            radWaitingBar1.StartWaiting();
            //this.FormElement.Size = new System.Drawing.Size(740, 288);


        }

        private void SplashScreenForm_Shown(object sender, EventArgs e)
        {
            
        }

        private void radLabel2_Click(object sender, EventArgs e)
        {

        }
    }
}
