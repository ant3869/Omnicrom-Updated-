using System;
using System.Drawing;

namespace Omnicrom.Forms
{
    public partial class DialogBoxForm : Telerik.WinControls.UI.RadForm
    {
        private string MessageTitle { get; set; }
        private string MessageText { get; set; }
        public bool DiagResult { get; set; }
        private Color originalOKcolor { get; set; }
        private Color originalCANCELcolor { get; set; }

        public DialogBoxForm(string title, string message)
        {
            InitializeComponent();
            DiagResult = false;
            MessageTitle = string.Empty;
            MessageText = string.Empty;

            MessageTitle = title;
            MessageText = message;

            LoadCustomTheme();
        }

        private void LoadCustomTheme()
        {
            //ThemeResolutionService.ApplicationThemeName = "Office2019Dark";
            originalOKcolor = Button_Dialog_OK.ButtonElement.BorderElement.ForeColor;
            originalCANCELcolor = Button_Dialog_Cancel.ButtonElement.BorderElement.ForeColor;

            this.Text = MessageTitle;
            this.Label_Dialog_Message.Text = MessageText;
        }

        private void MessageBoxForm_Load(object sender, EventArgs e)
        {
            this.BringToFront();
            this.Focus();
        }

        private void Button_Dialog_OK_Click(object sender, EventArgs e)
        {
            this.DiagResult = true;
            this.Close();
        }

        private void Button_Dialog_Cancel_Click(object sender, EventArgs e)
        {
            this.DiagResult = false;
            this.Close();
        }

        private void Button_Dialog_OK_MouseLeave(object sender, EventArgs e)
        {
            Button_Dialog_OK.ButtonElement.BorderElement.ForeColor = originalOKcolor;
            Button_Dialog_OK.ButtonElement.BorderElement.ForeColor2 = originalOKcolor;
        }

        private void Button_Dialog_Cancel_MouseLeave(object sender, EventArgs e)
        {
            Button_Dialog_Cancel.ButtonElement.BorderElement.ForeColor = originalCANCELcolor;
            Button_Dialog_Cancel.ButtonElement.BorderElement.ForeColor2 = originalCANCELcolor;
        }

        private void Button_Dialog_OK_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            //Button_Dialog_OK.ButtonElement.BorderElement.ForeColor = Color.PaleGreen;
            //Button_Dialog_OK.ButtonElement.BorderElement.ForeColor2 = Color.PaleGreen;
        }

        private void Button_Dialog_Cancel_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
        {
            Button_Dialog_Cancel.ButtonElement.BorderElement.ForeColor = Color.IndianRed;
            Button_Dialog_Cancel.ButtonElement.BorderElement.ForeColor2 = Color.IndianRed;
        }
    }
}
