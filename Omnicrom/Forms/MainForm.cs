using System;
//using NexcoSystem;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Telerik.WinControls;
using Telerik.WinControls.UI;
using System.Diagnostics;
using static Omnicrom.Global;
using static Omnicrom.Functions;
using static Omnicrom.RichTextBoxExtensions;
using System.Threading.Tasks;
using Telerik.WinControls.Primitives;
using Omnicrom.Forms;

namespace Omnicrom
{

    //var T = Task.Run(() =>
    //{
    //    try
    //    {

    //    }
    //    catch (Exception e) { MessageBox.Show(string.Format("Exception {0} Trace {1}", e.Message, e.StackTrace)); }
    //});

    //return T;

    public partial class MainForm : RadForm
    {
        #region Initiaization

        public MainForm()
        {
            InitializeComponent();
            InitializeForm();
        }

        private void InitializeForm()
        {
            UpdateSplashScreenText("Initializing form data");

            try
            {
                InitializeFormComponents();
                ConnectProxyControls();          
                LoadFormProperties();
                InitalizeRemovableEvents();
                LoadSystemInformation();
            }
            catch (Exception e) { MessageBox.Show(string.Format("Exception {0} Trace {1}", e.Message, e.StackTrace)); }

            //InitalizeStopLock();     
        }

        private void ConnectProxyControls()
        {
            UpdateSplashScreenText("Connecting proxy controls");

            RichTextBoxExtensions.logbox = this.LogBox_Omnicrom;
            Global.omnicromlogbox = RichTextBoxExtensions.logbox;
            Global.mainform = this;
        }

        private void LoadFormProperties()
        {
            UpdateSplashScreenText("Loading form properties");

            try
            {
                //this.FormElement.TitleBar.MaximizeButton.VisualState = "Collapsed";
                this.FormElement.Size = new System.Drawing.Size(1000, 495); // 900 600
                //this.StatusStrip_Main.Size = new System.Drawing.Size(992, 20);
                this.FormElement.AutoToolTip = false;
            }
            catch (Exception e) { MessageBox.Show(string.Format("Exception {0} Trace {1}", e.Message, e.StackTrace)); }
        }

        private void InitializeFormComponents()
        {
            UpdateSplashScreenText("Loading form components");

            PageView_Logs.SelectedPage.Name = "ViewPage_Logs_Omnicrom";
            CurrentLogBox = LogBox_Omnicrom;
            CurrentLogBox.Name = "Omnicrom_Log";
            SoundOn = false;

            OmnicromRunTime = new Stopwatch();
            IdleTimer = new Stopwatch();
            //statustimer = new Timer();
        }

        private void LoadSystemInformation()
        {
            UpdateSplashScreenText("Loading system information");

            //await Task.Run(() =>
            //{ });

            LoadLocalDiskDetails();
            LoadRemovableDiskDetails();
            LoadMachineDetails();
            LoadPerformanceCounters();      
        }

        #endregion Initiaization

        #region Load Objects

        private void LoadLocalDiskDetails()
        {
            UpdateSplashScreenText("Loading local drive data");
            LoadLocalDrive();

            Label_Drives_LocalName.Text = LocalDrive.Name;
            Label_Drives_LocalTotalSpace.Text = LocalDrive.TotalSpaceText;
        }

        private void LoadRemovableDiskDetails()
        {
            UpdateSplashScreenText("Loading removable drive data");
            LoadRemovableDisk();
            ShowRemovableDriveGroupBox();
        }

        private void ShowRemovableDriveGroupBox()
        {
            if ((RemovableDriveFound == true && Label_Drives_RemovableName.Text == Removable.Name) || (RemovableDriveFound == false && GroupBox_Drives_Removable.Visible == false))
                return;

            if (!RemovableDriveFound)
            {   
                GroupBox_Drives_Removable.Visible = false;
                PictureBox_Drives_RemovableImage.Visible = false;
            }
            else
            {
                Label_Drives_RemovableName.Text = Removable.Name;
                Label_Drives_RemovableTotalSpace.Text = Removable.TotalSpaceText;

                if (GroupBox_Drives_Removable.Visible != true || PictureBox_Drives_RemovableImage.Visible != true)
                {
                    PictureBox_Drives_RemovableImage.Visible = true;
                    GroupBox_Drives_Removable.Visible = true;
                    GroupBox_Drives_Removable.Show();
                }
            }
        }

        private void LoadMachineDetails()
        {
            UpdateSplashScreenText("Loading local machine data");
            LoadDeviceObject();

            // Load Machine Details
            Label_Machine_Name.DataBindings.Add("Text", LocalMachine, "Name");
            Label_Machine_Tag.DataBindings.Add("Text", LocalMachine, "ServiceTag");
            Label_Machine_UpTime.DataBindings.Add("Text", LocalMachine, "SystemUpTime");
            Label_Machine_Model.DataBindings.Add("Text", LocalMachine, "Model");
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            UpdateSplashScreenText("Finishing up");
            CheckViewSelection("ViewPage_Logs_Omnicrom");
 
            OmnicromRunTime.Start();
            statustimer.Start();
            IdleTimer.Start();

            this.BringToFront();
            this.Focus();    
            this.TopMost = false;

            UpdateSplashScreenText("Finished loading sequence. Starting ...");
            Log("Omnicrom started successfully.");
        }

        #endregion Load Objects

        #region Update Status

        private bool played = false;
        string saved = "";
        private async void SetStatus(string text)
        {
            EffectManager em = new EffectManager();
            PictureBox_Status_Ok.Hide();
            PictureBox_Status_Warn.Hide();
            PictureBox_Status_Error.Hide();
            Color color;

            switch (text)
            {
                case "OK":
                    color = Color.LightGreen;
                    this.PictureBox_Status_Ok.Show();
                    break;
                case "Warning": 
                    color = Color.Yellow;
                    this.PictureBox_Status_Warn.Show();
                    break;
                case "Error": 
                    color = Color.Red;
                    this.PictureBox_Status_Error.Show();
                    break;
                default: color = Color.Azure; break;
            }

            // Set status text
            Label_Status_Indicator.Text = text;

            // Show status blink effect
            if (saved != text)
                await em.SoftBlink(Label_Status_Indicator, color);

            // Set status text color
            Label_Status_Indicator.ForeColor = color;
            saved = text;

            // Play status related audio wav (if wav not just previous played)
            if (!played)
            {
                EffectManager.PlaySound(text);
                played = true;
            }
            else
                played = false;
        }

        private void CompareDisk()
        {
            var description = GetStatusDescriptionText();
            var indicator = GetStatusIndicatorText(description);

            try
            {
                Label_Status_Description.Text = description;
                SetStatus(indicator);
            }
            catch (Exception e) { Log(string.Format("Exception {0} Trace {1}", e.Message, e.StackTrace)); }
        }

        #endregion Update Status

        #region Performace Updates

        private void statustimer_Tick(object sender, EventArgs e)
        {
            try
            {
                CheckRemovableSatus();
                UpdateLocalMachineInfo();
                UpdateLocalDiskInfo();
                UpdateRemovableDiskInfo();

                CompareDisk();
            }
            catch (Exception ex) { Log(string.Format("Exception {0} Trace {1}", ex.Message, ex.StackTrace)); }
        }

        private void UpdateLocalMachineInfo()
        { 
            try
            {
                // Machine (_Total)
                int fcpu = GetCPUUse();
                int fram = GetRAMUse();
                int fdsk = GetDiskUseAvg();
                int fidle = GetDiskIdleAvg();
                int fpge = GetPGEUse();

                ProgressBar_Machine_CPU.Value1 = fcpu;
                ProgressBar_Machine_RAM.Value1 = fram;
                ProgressBar_Machine_DSK.Value1 = fdsk;
                ProgressBar_Machine_DSK.Value2 = fidle;
                ProgressBar_Machine_PGE.Value1 = fpge;

                // Machine UpTime
                Label_Machine_UpTime.Text = GetSystemUpTime();

                //// Omnicrom RunTime
                Label_Logs_OmnicromRunTime.Text = Converter.ConvertUpTime(OmnicromRunTime.Elapsed);
            }
            catch (Exception e) { Log(string.Format("Exception {0} Trace {1}", e.Message, e.StackTrace)); }
        }

        private void UpdateLocalDiskInfo()
        {   
            try
            {
                // Local Disk (C:)
                int flocaldsk = (int)DSK_Counter_Local.NextValue();
                long flocalread = (long)DSKR_Counter_Local.NextValue();
                long flocalwrite = (long)DSKW_Counter_Local.NextValue();

                ProgressBar_Drives_LocalUsedSpace.Value1 = (int)LocalDrive.UsedSpacePercent;
                Label_Drives_LocalFreeSpace.Text = $"{LocalDrive.FreeSpaceText} free of";

                ProgressBar_Drives_LocalDiskActivity.Value1 = flocaldsk;

                Label_Drives_LocalRead.Text = Converter.ConvertByteSize(flocalread) + "/sec";
                Label_Drives_LocalWrite.Text = Converter.ConvertByteSize(flocalwrite) + "/sec";
            }
            catch (Exception e) { Log(string.Format("Exception {0} Trace {1}", e.Message, e.StackTrace)); }                     
        }

        private void UpdateRemovableDiskInfo()
        {
            try
            {
                if (!RemovableDriveFound)
                    return;
                else if (RemovableDriveFound == true && GroupBox_Drives_Removable.Visible != true)
                    ShowRemovableDriveGroupBox();


                // Removable Disk (X:)
                int fremovedsk = (int)DSK_Counter_Removable.NextValue();
                long fremoveread = (long)DSKR_Counter_Removable.NextValue();
                long fremovewrite = (long)DSKW_Counter_Removable.NextValue();

                ProgressBar_Drives_RemovableUsedSpace.Value1 = (int)Removable.UsedSpacePercent;
                Label_Drives_RemovableFreeSpace.Text = $"{Removable.FreeSpaceText} free of";

                ProgressBar_Drives_RemovableDiskActivity.Value1 = fremovedsk;
                Label_Drives_RemovableRead.Text = Converter.ConvertByteSize(fremoveread) + "/sec";
                Label_Drives_RemovableWrite.Text = Converter.ConvertByteSize(fremovewrite) + "/sec";        
            }
            catch (Exception e) { Log(string.Format("Exception {0} Trace {1}", e.Message, e.StackTrace)); }
        }


        #endregion  Performace Updates

        #region Log View Page Events


        // Click Events
        private void Button_Logs_ScrollToTop_Click(object sender, EventArgs e)
        {
            CurrentLogBox.SelectionStart = 1;
            CurrentLogBox.ScrollToCaret();
        }

        private void Button_Logs_ExternalOpen_Click(object sender, EventArgs e)
        {

        }

        private void Button_Logs_Clear_Click(object sender, EventArgs e)
        {
            CurrentLogBox.Clear();
        }

        private void Button_Logs_Find_Click(object sender, EventArgs e)
        {
            Find(TextBox_Logs_FindBoxInput.Text, false, CurrentLogBox);
        }
  

        // Mouse Over Events
        private void Button_Logs_Clear_MouseMove(object sender, MouseEventArgs e)
        {
            //Button_Logs_Clear.ButtonElement.BorderHighlightColor = Color.Red;
            Button_Logs_Clear.ButtonElement.BorderElement.ForeColor = Color.Red;
            //Button_Logs_Clear.ButtonElement.SetThemeValueOverride(FillPrimitive.ForeColorProperty, Color.Red, "MouseOver", typeof(FormImageBorderPrimitive));
            //radButton1.ButtonElement.ButtonFillElement.BackColor = Color.FromArgb(153, 204, 55);
            Label_ToolTip_DisplayText.Text = "Clear text from the logbox currently being displayed.";
        }

        private void Button_Logs_ScrollToTop_MouseMove(object sender, MouseEventArgs e)
        {
            Label_ToolTip_DisplayText.Text = "Scroll to the top of selected log text.";
        }

        private void Button_Logs_ExternalOpen_MouseMove(object sender, MouseEventArgs e)
        {
            Label_ToolTip_DisplayText.Text = "Open selected log externally as a text file.";
        }

        private void Button_Logs_Find_MouseMove(object sender, MouseEventArgs e)
        {
            Label_ToolTip_DisplayText.Text = "Confirm log search with provided text paramater.";
        }

        private void TextBox_Logs_FindBoxInput_MouseMove(object sender, MouseEventArgs e)
        {
            Label_ToolTip_DisplayText.Text = "Find associated text in logbox below.";
        }

        private void Label_Logs_Header_MouseMove(object sender, MouseEventArgs e)
        {
            Label_ToolTip_DisplayText.Text = "Title of currently selected page index.";
        }

        // View Page Events

        private void PageView_Logs_SelectedPageChanged(object sender, EventArgs e)
        {
            if (PageView_Logs.SelectedPage.Name == null)
                PageView_Logs.SelectedPage.Name = "ViewPage_Logs_Omnicrom";

            CheckViewSelection(PageView_Logs.SelectedPage.Name);
        }

        private void LogBox_Omnicrom_TextChanged(object sender, EventArgs e)
        {
            if (LogBox_Omnicrom.Visible)
            {
                LogBox_Omnicrom.SelectionStart = LogBox_Omnicrom.TextLength;
                LogBox_Omnicrom.ScrollToCaret();
            }
        }

        private void CheckViewSelection(string selected)
        {
            CurrentLogBox = new RichTextBox();

            Button_Logs_ScrollToTop.Visible = true;
            TextBox_Logs_FindBoxInput.Visible = true;
            Button_Logs_Find.Visible = true;
            Button_Logs_ExternalOpen.Visible = true;
            Button_Logs_Clear.Visible = true;

            Label_Logs_Header.Text = string.Empty;
            CurrentLogBox.Name = string.Empty;

            switch (selected)
            {
                case "ViewPage_Logs_QuickTools":
                    Label_Logs_Header.Text = "Quick Tools";
                    Button_Logs_ScrollToTop.Visible = false;
                    Button_Logs_Find.Visible = false;
                    Button_Logs_Clear.Visible = false;
                    TextBox_Logs_FindBoxInput.Visible = false;
                    Button_Logs_ExternalOpen.Visible = false;
                    break;
                case "ViewPage_Logs_USMT":
                    Label_Logs_Header.Text = "USMT Log";
                    //CurrentLogBox = LogBox_USMT;
                    CurrentLogBox.Name = "USMTLog.txt";
                    break;
                case "ViewPage_Logs_AppInstaller":
                    Label_Logs_Header.Text = "Application Log";
                    //CurrentLogBox = LogBox_AppInstall;
                    CurrentLogBox.Name = "AppInstallLog.txt";
                    break;
                case "ViewPage_Logs_Omnicrom":
                    Label_Logs_Header.Text = "Omnicrom Log";
                    CurrentLogBox = LogBox_Omnicrom;
                    CurrentLogBox.Name = "OmnicromLog.txt";
                    break;
            }

            UpdateForm(this);
        }

        #endregion  Log View Page Events

        #region Machine Control Events


        // Mouse Over Events
        private void radGroupBox3_MouseMove(object sender, MouseEventArgs e)
        {
            Label_ToolTip_DisplayText.Text = "Panel displaying attributes as related to this machine.";
        }

        private void Label_Machine_Model_MouseMove(object sender, MouseEventArgs e)
        {
            Label_ToolTip_DisplayText.Text = "Model of local machine currently being accessed.";
        }

        private void Label_Machine_Name_MouseMove(object sender, MouseEventArgs e)
        {
            Label_Machine_Name.ForeColor = Color.AliceBlue;
            Label_ToolTip_DisplayText.Text = "Machine display name. NOTE: Clicking this text will copy contents to the clipboard.";
        }

        private void Label_Machine_Tag_MouseMove(object sender, MouseEventArgs e)
        {
            Label_Machine_Tag.ForeColor = Color.AliceBlue;
            Label_ToolTip_DisplayText.Text = "Manufacturer issued Service Tag tied to this machine as parsed from Bios. NOTE: Clicking this text will copy contents to the clipboard.";
        }

        private void Label_Machine_UpTime_MouseMove(object sender, MouseEventArgs e)
        {
            Label_ToolTip_DisplayText.Text = "Local machine's runtime clocked since its last power cycle.";
        }

        private void ProgressBar_Machine_CPU_MouseMove(object sender, MouseEventArgs e)
        {
            Label_ToolTip_DisplayText.Text = "Progressbar displaying system's current CPU usage.";
        }

        private void ProgressBar_Machine_RAM_MouseMove(object sender, MouseEventArgs e)
        {
            Label_ToolTip_DisplayText.Text = "Progressbar displaying system's current memory usage.";
        }

        private void ProgressBar_Machine_DSK_MouseMove(object sender, MouseEventArgs e)
        {
            Label_ToolTip_DisplayText.Text = "Progressbar displaying the local system's average disk use. The average is taken from local and, if connected, external disk activity.";
        }

        private void ProgressBar_Machine_PGE_MouseMove(object sender, MouseEventArgs e)
        {
            Label_ToolTip_DisplayText.Text = "Progressbar displaying the local system's page file use.";
        }

        private void Button_Machine_LogOut_MouseMove(object sender, MouseEventArgs e)
        {
            Label_ToolTip_DisplayText.Text = "Logout current Windows user.";
        }

        private void Button_Machine_Restart_MouseMove(object sender, MouseEventArgs e)
        {
            Label_ToolTip_DisplayText.Text = "Restart local machine.";
        }

        private void Button_Machine_Shutdown_MouseMove(object sender, MouseEventArgs e)
        {
            Label_ToolTip_DisplayText.Text = "Shutdown local machine.";
        }

        private void Button_Machine_TaskManager_MouseMove(object sender, MouseEventArgs e)
        {
            Label_ToolTip_DisplayText.Text = "Launch Windows Task Manager.";
        }


        #endregion Machine Control Events

        #region Drives Control Events


        // Removable Mouse Over Events
        private void GroupBox_Drives_Removable_MouseMove(object sender, MouseEventArgs e)
        {
            Label_ToolTip_DisplayText.Text = "Panel displaying attributes as related to currently connected external drive.";
        }

        private void Label_Drives_RemovableName_MouseMove(object sender, MouseEventArgs e)
        {
            Label_ToolTip_DisplayText.Text = "Letter name representing current external drive ready and eligible for data handling.";
        }

        private void ProgressBar_Drives_RemovableUsedSpace_MouseMove(object sender, MouseEventArgs e)
        {
            Label_ToolTip_DisplayText.Text = "Progressbar displaying external used storage space.";
        }

        private void Label_Drives_RemovableFreeSpace_MouseMove(object sender, MouseEventArgs e)
        {
            Label_ToolTip_DisplayText.Text = "Amount of currently free storage space available to the connected external drive.";
        }

        private void Label_Drives_RemovableTotalSpace_MouseMove(object sender, MouseEventArgs e)
        {
            Label_ToolTip_DisplayText.Text = "Total storage amount of connected external drive.";
        }

        private void ProgressBar_Drives_RemovableDiskActivity_MouseMove(object sender, MouseEventArgs e)
        {
            Label_ToolTip_DisplayText.Text = "Progressbar displaying external disk activity within a 0-100 value range. 0 reflecting no current activity.";
        }

        private void Label_Drives_RemovableRead_MouseMove(object sender, MouseEventArgs e)
        {
            Label_ToolTip_DisplayText.Text = "Amount in bytes of data being read by external disk per second.";
        }

        private void Label_Drives_RemovableWrite_MouseMove(object sender, MouseEventArgs e)
        {
            Label_ToolTip_DisplayText.Text = "Amount in bytes of data being written by external disk per second.";
        }

        private void Label_Drives_RemovablePSTcount_MouseMove(object sender, MouseEventArgs e)
        {
            Label_ToolTip_DisplayText.Text = "Estimated file count with extension .PST from the connected external drive's 'User' Folder. NOTE: Amount shown is an approximation";
        }

        private void Label_Drives_RemovableDIRcount_MouseMove(object sender, MouseEventArgs e)
        {
            Label_ToolTip_DisplayText.Text = "Estimated directory count from the connected external drive's 'User' Folder. NOTE: Amount shown is an approximation";
        }

        private void Label_Drives_RemovableFILEcount_MouseMove(object sender, MouseEventArgs e)
        {
            Label_ToolTip_DisplayText.Text = "Estimated file count from the connected external drive's 'User' Folder. NOTE: Amount shown is an approximation";
        }

        private void Label_Drives_RemovableAPPcount_MouseMove(object sender, MouseEventArgs e)
        {
            Label_ToolTip_DisplayText.Text = "Estimated application count from the connected external drive's 'User' Folder. NOTE: Amount shown is an approximation";
        }





        #endregion Drives Control Events

        #region Settings Control Events

        // Mouse Over Events
        private void GroupBox_Settings_MouseMove(object sender, MouseEventArgs e)
        {
            Label_ToolTip_DisplayText.Text = "Enable or Disable Omnicrom option settings.";
        }

        private void ToggleSwitch_Settings_ToolTips_MouseMove(object sender, MouseEventArgs e)
        {
            Label_ToolTip_DisplayText.Text = "Toggle on/off tool tip text display. This is what you are currently reading.";
        }

        private void ToggleSwitch_Settings_Sound_MouseMove(object sender, MouseEventArgs e)
        {
            Label_ToolTip_DisplayText.Text = "Toggle on/off audio effects. This includes alerts, alarms and notification sounds.";
        }

        private void ToggleSwitch_Settings_Darkmode_MouseMove(object sender, MouseEventArgs e)
        {
            Label_ToolTip_DisplayText.Text = "Toggle on/off dark mode for current visual theme.";
        }

        private void ToggleSwitch_Settings_ShowLogs_MouseMove(object sender, MouseEventArgs e)
        {
            Label_ToolTip_DisplayText.Text = "Toggle on/off the extended form Log section.";
        }



        #endregion Settings Control Events

        #region Status Control Events

        public void Reset_ToolTip_MouseLeave(object sender, EventArgs e)
        {
            RadButton but = sender as RadButton;
            Label_ToolTip_DisplayText.Text = "";
     
            //but.ButtonElement.ResetLayout
        }

        // Mouse Over Events
        private void GroupBox_Status_MouseMove(object sender, MouseEventArgs e)
        {
            Label_ToolTip_DisplayText.Text = "Panel displaying attributes as related to the status of the relation between the detected physical drives..";
        }

        private void Label_Status_Indicator_MouseMove(object sender, MouseEventArgs e)
        {
            Label_ToolTip_DisplayText.Text = "Status result of various performance, and comparison checks. (OK: Operations should perform as expected, Warning: Non-fatal issue found, Error: Operational breaking issue found)";
        }

        private void Label_Status_Description_MouseMove(object sender, MouseEventArgs e)
        {
            Label_ToolTip_DisplayText.Text = "Brief description detailing the results of operational checks that may affect tools used in Omnicrom.";
        }

        private void Label_Logs_OmnicromRunTime_MouseMove(object sender, MouseEventArgs e)
        {
            Label_ToolTip_DisplayText.Text = "Elapsed run time of current Omnicrom session.";
        }




        #endregion Status Control Events

        // Copy to clipboard events
        private void Label_Machine_Name_MouseClick(object sender, MouseEventArgs e)
        {
            CopyTextFromLabel(Label_Machine_Name);
        }

        private void Label_Machine_Tag_MouseClick(object sender, MouseEventArgs e)
        {
            CopyTextFromLabel(Label_Machine_Tag);
        }


        #region Settings toggle switch events


        // Settings toggle switch events
        private void ToggleSwitch_Settings_StopLock_ValueChanged(object sender, EventArgs e)
        {
            //ActivateStopLock();
        }

        private void ToggleSwitch_Settings_ToolTips_ValueChanged(object sender, EventArgs e)
        {
            if (!Label_ToolTip_DisplayText.Visible)
                Label_ToolTip_DisplayText.Visible = true;
            else
                Label_ToolTip_DisplayText.Visible = false;
        }

        private void ToggleSwitch_Settings_Sound_ValueChanged(object sender, EventArgs e)
        {
            if (SoundOn != true)
            {
                SoundOn = true;
                EffectManager.PlaySound("common");
            }
            else
                SoundOn = false;
        }

        private void ToggleSwitch_Settings_Darkmode_ValueChanged(object sender, EventArgs e)
        {
            if (ThemeResolutionService.ApplicationThemeName != "VisualStudio2012Light")
            {
                //Telerik.WinControls.Themes.CrystalTheme CrytsalTheme = new Telerik.WinControls.Themes.CrystalTheme();
                ThemeResolutionService.ApplicationThemeName = "VisualStudio2012Light";
            }
            else
            {
                //ThemeResolutionService.LoadPackageResource("NetFrameWork.WalmartDarkMintGreen.tssp");
                ThemeResolutionService.ApplicationThemeName = "VisualStudio2012Dark";
            }

            UpdateForm(this);
        }

        private void ToggleSwitch_Settings_ShowLogs_ValueChanged(object sender, EventArgs e)
        {
            if (mainform.Width != 1000)
                mainform.Size = new System.Drawing.Size(1000, 495);            
            else
                mainform.Size = new System.Drawing.Size(752, 495); // 1000, 495         

            UpdateForm(this);
        }


        #endregion Settings toggle switch events



        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {
            DisposePerformanceCounters();
        }


        // Machine shortcut click events
        private void Button_Machine_LogOut_Click(object sender, EventArgs e)
        {
            DialogBoxForm messagebox = new DialogBoxForm("Logout", "Are you sure you want to Logout?");
            messagebox.ShowDialog();

            if (messagebox.DiagResult)
                LogoutCurrentUser();
        }

        private void Button_Machine_Restart_Click(object sender, EventArgs e)
        {
            DialogBoxForm messagebox = new DialogBoxForm("Restart", "Are you sure you want to Restart?");
            messagebox.ShowDialog();

            if (messagebox.DiagResult)
                SystemRestart();
        }

        private void Button_Machine_Shutdown_Click(object sender, EventArgs e)
        {
            DialogBoxForm messagebox = new DialogBoxForm("Shutdown", "Are you sure you want to Shutdown?");
            messagebox.ShowDialog();

            if (messagebox.DiagResult)
                SystemShutDown();
        }

        private async void Button_Machine_TaskManager_Click(object sender, EventArgs e)
        {
            Log("Launching Windows Task Manager ...");
            await Task.Run(() => Process.Start("taskmgr.exe"));
        }

        private void GroupBox_Drives_Removable_VisibleChanged(object sender, EventArgs e)
        {

        }
    }
}
