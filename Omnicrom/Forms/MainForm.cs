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
using System.Security.Principal;
using Microsoft.Win32;

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

    // Blue Color 0, 122, 204
    // Gray Color 64, 64, 64



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
            catch (Exception e) { Log($"Error intializing form: Exception {e.Message} Trace {e.StackTrace}"); }
        }

        private void ConnectProxyControls()
        {
            UpdateSplashScreenText("Connecting proxy controls");

            try
            {
                RichTextBoxExtensions.logbox = this.LogBox_Omnicrom;
                RichTextBoxExtensions.miglogbox = this.RichTextBox_Logs_USMT;
                Global.omnicromlogbox = RichTextBoxExtensions.logbox;
                Global.USMTLogBox = RichTextBoxExtensions.miglogbox;
                Global.mainform = this;
            }
            catch (Exception e) { Log($"Error connecting proxy controls: Exception {e.Message} Trace {e.StackTrace}"); }
        }

        private void LoadFormProperties()
        {
            UpdateSplashScreenText("Loading form properties");

            try
            {
                this.Size = new System.Drawing.Size(1000, 495); // 1000, 495
                this.FormElement.AutoToolTip = false;
                SoundOn = false;
            }
            catch (Exception e) { Log($"Error loading form properties: Exception {e.Message} Trace {e.StackTrace}"); }
        }

        private void InitializeFormComponents()
        {
            UpdateSplashScreenText("Loading form components");

            try
            {
                PageView_Logs.SelectedPage.Name = "ViewPage_Logs_Omnicrom";
                CurrentLogBox = LogBox_Omnicrom;
                CurrentLogBox.Name = "Omnicrom_Log";

                OmnicromRunTime = new Stopwatch();
                IdleTimer = new Stopwatch();
                RunningProcessIDs = new List<int>();
            }
            catch (Exception e) { Log($"Error loading system information: Exception {e.Message} Trace {e.StackTrace}"); }
        }

        private void LoadSystemInformation()
        {
            UpdateSplashScreenText("Loading system information");

            try 
            {
                LoadLocalDiskDetails();
                LoadRemovableDiskDetails();
                LoadMachineDetails();
                LoadPerformanceCounters();
            }
            catch (Exception e) { Log($"Error loading system information: Exception {e.Message} Trace {e.StackTrace}"); }    
        }

        #endregion Initiaization

        #region Load Objects

        private void LoadLocalDiskDetails()
        {
            UpdateSplashScreenText("Loading local drive data");

            try
            {
                LoadLocalDrive();

                Label_Drives_LocalName.Text = LocalDrive.Name;
                Label_Drives_LocalTotalSpace.Text = LocalDrive.TotalSpaceText;
            }
            catch (Exception ex) { Log($"Error loading local disk details: Exception {ex.Message} Trace {ex.StackTrace}"); }
        }

        private void LoadRemovableDiskDetails()
        {
            UpdateSplashScreenText("Loading removable drive data");

            try
            {
                LoadRemovableDisk();
                ShowRemovableDriveGroupBox();
            }
            catch (Exception ex) { Log($"Error loading removable disk details: Exception {ex.Message} Trace {ex.StackTrace}"); }  
        }

        private void ShowRemovableDriveGroupBox()
        {
            try
            {
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
            catch (Exception ex) { Log($"Error loading machine details: Exception {ex.Message} Trace {ex.StackTrace}"); }
        }

        private void LoadMachineDetails()
        {
            UpdateSplashScreenText("Loading local machine data");

            try 
            {
                LoadDeviceObject();

                // Load Machine Details
                Label_Machine_Name.DataBindings.Add("Text", LocalMachine, "Name");
                Label_Machine_Tag.DataBindings.Add("Text", LocalMachine, "ServiceTag");
                Label_Machine_UpTime.DataBindings.Add("Text", LocalMachine, "SystemUpTime");
                Label_Machine_Model.DataBindings.Add("Text", LocalMachine, "Model");
            }
            catch (Exception ex) { Log($"Error loading machine details: Exception {ex.Message} Trace {ex.StackTrace}"); }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            UpdateSplashScreenText("Loading main form objects");
    
            LoadStartApplications();

            UpdateSplashScreenText("Finished loading sequence. Starting ...");
            Log("Omnicrom started successfully.");
        }

        private async void LoadStartApplications()
        {
            try 
            {
                if (mainform.Width != 1000)
                    mainform.Size = new System.Drawing.Size(1000, 495);

                CheckViewSelection("ViewPage_Logs_Omnicrom");
                await Task.Run(() => {  StartCaffeineApp(); });

                OmnicromRunTime.Start();
                statustimer.Start();
                IdleTimer.Start();

                USMT_GPUpdateON = true;
                USMT_PostCleanUpON = true;
                USMT_FinishedSoundON = true;
                USMT_PostCompareON = true;

                this.BringToFront();
                this.Focus();
                this.TopMost = false;

                StartLogMonitor(OmnicromExternalLogPath);
            }
            catch (Exception ex) { Log($"Error starting form applications: Exception {ex.Message} Trace {ex.StackTrace}"); }       
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

            try
            {
                switch (text)
                {
                    case "OK":
                        color = Color.LightGreen;
                        this.PictureBox_Status_Ok.Show();
                        this.PictureBox_Status_Ok.BringToFront();
                        break;
                    case "Warning":
                        color = Color.Yellow;
                        this.PictureBox_Status_Warn.Show();
                        this.PictureBox_Status_Warn.BringToFront();
                        break;
                    case "Error":
                        color = Color.Red;
                        this.PictureBox_Status_Error.Show();
                        this.PictureBox_Status_Error.BringToFront();
                        break;
                    default: color = Color.Azure; break;
                }

                // Set status text
                Label_Status_Indicator.Text = text;

                // Show status blink effect
                if (saved != text)
                    await Task.Run(() => { em.SoftBlink(Label_Status_Indicator, color); });

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
            catch (Exception ex) { Log($"Error at set status: Exception {ex.Message} Trace {ex.StackTrace}"); }       
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
            catch (Exception ex) { Log($"Error at compare disk: Exception {ex.Message} Trace {ex.StackTrace}"); }
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
            catch (Exception ex) { Log($"Error at status timer tick: Exception {ex.Message} Trace {ex.StackTrace}"); }
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
                
                // Omnicrom RunTime
                Label_Logs_OmnicromRunTime.Text = Converter.ConvertUpTime(OmnicromRunTime.Elapsed);

                // Disk idle timer
                Label_TEST_IDLETIME.Text = Converter.ConvertUpTime(IdleTimer.Elapsed);
            }
            catch (Exception ex) { Log($"Error updating machine information: Exception {ex.Message} Trace {ex.StackTrace}"); }
        }

        private void UpdateLocalDiskInfo()
        {   
            try
            {
                // Local Disk (C:)
                int flocaldsk = GetDiskUse(DSK_Counter_Local);
                int flocalidle = GetDiskIdlePercentage(IDLE_Counter_Local);
                long flocalread = (long)DSKR_Counter_Local.NextValue();
                long flocalwrite = (long)DSKW_Counter_Local.NextValue();

                ProgressBar_Drives_LocalUsedSpace.Value1 = (int)LocalDrive.UsedSpacePercent;
                Label_Drives_LocalFreeSpace.Text = $"{LocalDrive.FreeSpaceText} free of";

                ProgressBar_Drives_LocalDiskActivity.Value1 = flocaldsk;
                ProgressBar_Drives_LocalDiskActivity.Value2 = flocalidle;
                Label_Drives_LocalRead.Text = Converter.ConvertByteSize(flocalread) + "/sec";
                Label_Drives_LocalWrite.Text = Converter.ConvertByteSize(flocalwrite) + "/sec";
            }
            catch (Exception ex) { Log($"Error updating local disk information: Exception {ex.Message} Trace {ex.StackTrace}"); }
        }

        private void UpdateRemovableDiskInfo()
        {
            try
            {
                if (!RemovableDriveFound)
                {
                    if (GroupBox_Drives_Removable.Visible != false)
                        ShowRemovableDriveGroupBox();
                }   
                else
                {
                    if (GroupBox_Drives_Removable.Visible != true)
                        ShowRemovableDriveGroupBox();

                    // Removable Disk (X:)
                    int fremovedsk = GetDiskUse(DSK_Counter_Removable);
                    int fremoveidle = GetDiskUse(IDLE_Counter_Removable);
                    long fremoveread = (long)DSKR_Counter_Removable.NextValue();
                    long fremovewrite = (long)DSKW_Counter_Removable.NextValue();

                    ProgressBar_Drives_RemovableUsedSpace.Value1 = (int)Removable.UsedSpacePercent;
                    Label_Drives_RemovableFreeSpace.Text = $"{Removable.FreeSpaceText} free of";

                    ProgressBar_Drives_RemovableDiskActivity.Value1 = fremovedsk;
                    ProgressBar_Drives_RemovableDiskActivity.Value2 = fremoveidle;
                    Label_Drives_RemovableRead.Text = Converter.ConvertByteSize(fremoveread) + "/sec";
                    Label_Drives_RemovableWrite.Text = Converter.ConvertByteSize(fremovewrite) + "/sec";
                }            
            }
            catch (Exception ex) { Log($"Error updating removable disk information: Exception {ex.Message} Trace {ex.StackTrace}"); }
        }


        #endregion  Performace Updates

        #region Log View Page Events


        // Click Events
        private void Button_Logs_ScrollToTop_Click(object sender, EventArgs e)
        {
            try
            {
                CurrentLogBox.SelectionStart = 1;
                CurrentLogBox.ScrollToCaret();
            }
            catch (Exception ex) { Log($"Exception {ex.Message} Trace {ex.StackTrace}"); }    
        }

        private void Button_Logs_ExternalOpen_Click(object sender, EventArgs e)
        {
           Task.Run(() => OpenExternalLog(CurrentLogBox));
        }

        private void Button_Logs_Clear_Click(object sender, EventArgs e)
        {
            CurrentLogBox.Clear();
        }

        private void Button_Logs_Find_Click(object sender, EventArgs e)
        {
            Find(TextBox_Logs_FindBoxInput.Text);
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
            Label_Machine_Name.ForeColor = BlueTextColor;
            Label_ToolTip_DisplayText.Text = "Machine display name. NOTE: Clicking this text will copy contents to the clipboard.";
        }

        private void Label_Machine_Tag_MouseMove(object sender, MouseEventArgs e)
        {
            Label_Machine_Tag.ForeColor = BlueTextColor;
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

        // Copy to clipboard events
        private void Label_Machine_Name_MouseClick(object sender, MouseEventArgs e)
        {
            CopyTextFromLabel(Label_Machine_Name);
        }

        private void Label_Machine_Tag_MouseClick(object sender, MouseEventArgs e)
        {
            CopyTextFromLabel(Label_Machine_Tag);
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
            Label_ToolTip_DisplayText.Text = "";

            if (sender is RadButton)
            {
                RadButton but = sender as RadButton;

                if (but.ForeColor == BlueTextColor)
                    but.ForeColor = GrayButtonTextColor;
            }
            else if (sender is RadLabel)
            {
                RadLabel lab = sender as RadLabel;

                if (lab.ForeColor == BlueTextColor)
                    lab.ForeColor = WhiteTextColor;
            }
        }

        public void Button_MouseMove(object sender, MouseEventArgs e)
        {
            if (sender is RadButton)
            {
                RadButton but = sender as RadButton;

                if (but.ForeColor != BlueTextColor)
                    but.ForeColor = BlueTextColor;
            }
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

        #region Settings toggle switch events


        // Settings toggle switch events
        private async void ToggleSwitch_Settings_StopLock_ValueChangedAsync(object sender, EventArgs e)
        {
            if (StopLockID != 0)
                await Task.Run(() => { StopCaffeineAppAsync(); }); 
            else
                await Task.Run(() => { StartCaffeineApp(); } );
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
                mainform.Size = new System.Drawing.Size(755, 495); // 1000, 495         

            UpdateForm(this);
        }


        #endregion Settings toggle switch events


        private void MainForm_FormClosed(object sender, FormClosedEventArgs e)
        {

        }

        private async void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            DisposePerformanceCounters();
            await StopProcessAtCloseAsync();
        }

        private void GroupBox_Drives_Removable_VisibleChanged(object sender, EventArgs e)
        {

        }

        private void radButton3_Click(object sender, EventArgs e)
        {

        }

        #region QuickTool Events


        // Mouse Over Events
        private void GroupBox_QuickTools_MouseMove(object sender, MouseEventArgs e)
        {
            Label_ToolTip_DisplayText.Text = "Quick access for various tools, fixes, and directories.";
        }

        private void Button_QuickTools_OpenCMD_MouseMove(object sender, MouseEventArgs e)
        {
            Label_ToolTip_DisplayText.Text = "Open an instance of CMD prompt.";
        }

        private void Button_QuickTools_DiskRepair_MouseMove(object sender, MouseEventArgs e)
        {
            Label_ToolTip_DisplayText.Text = "Run Check Disk and DISM.";
        }

        private void Button_QuickTools_CleanDisk_MouseMove(object sender, MouseEventArgs e)
        {
            Label_ToolTip_DisplayText.Text = "Run disk cleanup.";
        }

        private void Button_QuickTools_OpenUserFolder_MouseMove(object sender, MouseEventArgs e)
        {
            Label_ToolTip_DisplayText.Text = "Opens user directory.";
        }

        private void Button_QuickTools_OpenPowerShell_MouseMove(object sender, MouseEventArgs e)
        {
            Label_ToolTip_DisplayText.Text = "Open an instance of PowerShell.";
        }

        private void Button_QuickTools_UnlockDrive_MouseMove(object sender, MouseEventArgs e)
        {
            Label_ToolTip_DisplayText.Text = "Unlock any BitLocked drives.";
        }

        // Click Events
        private void Button_QuickTools_OpenCMD_Click(object sender, EventArgs e)
        {
            StartCMDSessionAsync();
        }

        private void Button_QuickTools_OpenPowerShell_Click(object sender, EventArgs e)
        {
            StartPSSessionAsync();
        }

        private void Button_QuickTools_DiskRepair_Click(object sender, EventArgs e)
        {
           
        }

        private async void Button_QuickTools_CleanDisk_Click(object sender, EventArgs e)
        {
            try { await Task.Run(() => { _ = ProcessManager.RunScript(CleanMgr); }); }
            catch (Exception ex) { Log($"Error running clean manager: Exception {ex.Message} Trace {ex.StackTrace}"); }
        }

        private async void Button_QuickTools_OpenUserFolder_Click(object sender, EventArgs e)
        {
            try { await Task.Run(() => Process.Start(UserProfile)); }
            catch (Exception ex) { Log($"Error opening user directory: Exception {ex.Message} Trace {ex.StackTrace}"); }           
        }

        private async void Button_QuickTools_UnlockDrive_Click(object sender, EventArgs e)
        {
            //try { _ = ProcessManager.RunScript(apppath_UnlockDrive); }
            //catch (Exception ex) { Log($"Error opening Unlock: Exception {ex.Message} Trace {ex.StackTrace}"); }

            //try 
            //{ 
            //    await Task.Run(() => 
            //    {
            //        var startInfo = new ProcessStartInfo()
            //        {
            //            FileName = "powershell.exe",
            //            Arguments = $"-NoProfile -ExecutionPolicy ByPass -File \"{apppath_UnlockDrive}\"",
            //            Verb = "RunAs",
            //            UseShellExecute = false,
            //            CreateNoWindow = true,                  
            //        };
            //        Process.Start(startInfo);
            //    }); 
            //}
            //catch (Exception ex) { Log($"Error opening Unlock: Exception {ex.Message} Trace {ex.StackTrace}"); }


            //string app = "cmd.exe";
            //string com = @"start \\mig\c$\mig\Ant\Resources\Scripts\ModdedScripts\PowerShell\UnlockDrives.ps1";

            //try { await Task.Run(async () => { await RunCommandPrompt($"{app} {com}"); }); }
            //catch (Exception ex) { Log($"Error running GPUpate: Exception {ex.Message} Trace {ex.StackTrace}"); }

         
            try { RunBitunlocker(); }
            catch (Exception ex) { Log($"Error running GPUpdate: Exception {ex.Message} Trace {ex.StackTrace}"); }
        }

        private void Button_QuickTools_GPUpdate_Click(object sender, EventArgs e)
        {
           
            try { RunGpUpdate(); }
            catch (Exception ex) { Log($"Error running GPUpdate: Exception {ex.Message} Trace {ex.StackTrace}"); }
        }

        private void Button_QuickTools_Shortcut_Click(object sender, EventArgs e)
        {
            try { CopyOmnicromToDesktop(); }
            catch (Exception ex) { Log($"Error creating shortcut: Exception {ex.Message} Trace {ex.StackTrace}"); }
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

        // Old OMT Events
        private void Button_Old_OMT_MouseMove(object sender, MouseEventArgs e)
        {
            Label_ToolTip_DisplayText.Text = "Launch the original OMT v3.8 tool.";
        }

        private void Button_Old_USMT_MouseMove(object sender, MouseEventArgs e)
        {
            Label_ToolTip_DisplayText.Text = "Launch the original migration (USMT) GUI.";
        }

        private void Button_Old_AppInstaller_MouseMove(object sender, MouseEventArgs e)
        {
            Label_ToolTip_DisplayText.Text = "Launch the original App Installer GUI.";
        }

        private async void Button_Old_OMT_Click(object sender, EventArgs e)
        {
            try { await Task.Run(() => Process.Start(apppath_OMT)); }
            catch (Exception ex) { Log($"Error opening OMT v3.8: Exception {ex.Message} Trace {ex.StackTrace}"); }
        }

        private async void Button_Old_USMT_Click(object sender, EventArgs e)
        {
            try { await Task.Run(() => Process.Start(apppath_USMT)); }
            catch (Exception ex) { Log($"Error opening USMT: Exception {ex.Message} Trace {ex.StackTrace}"); }
        }

        private async void Button_Old_AppInstaller_Click(object sender, EventArgs e)
        {
            try { await Task.Run(() => Process.Start(apppath_AppInstaller)); }
            catch (Exception ex) { Log($"Error opening App Installer: Exception {ex.Message} Trace {ex.StackTrace}"); }
        }


        #endregion QuickTool Events

        #region Link Events


        // Link Click Events
        private async void Button_Links_DellCommand_Click(object sender, EventArgs e)
        {
            Log("Launching Dell Command webpage ...");
            try { await Task.Run(() => System.Diagnostics.Process.Start(DellCommand)); }
            catch (Exception ex) { Log($"Error opening browser: Exception {ex.Message} Trace {ex.StackTrace}"); }
        }

        private async void Button_Links_TransferOwnership_Click(object sender, EventArgs e)
        {
            Log("Launching Transfer of Ownership webpage ...");
            try { await Task.Run(() => System.Diagnostics.Process.Start(TransferOwner)); }
            catch (Exception ex) { Log($"Error opening browser: Exception {ex.Message} Trace {ex.StackTrace}"); }
        }

        private async void Button_Links_ServiceNow_Click(object sender, EventArgs e)
        {
            Log("Launching Service Now webpage ...");
            try { await Task.Run(() => System.Diagnostics.Process.Start(ServiceNow)); }
            catch (Exception ex) { Log($"Error opening browser: Exception {ex.Message} Trace {ex.StackTrace}"); }
        }

        private async void Button_Links_DellDrivers_Click(object sender, EventArgs e)
        {
            Log("Launching Dell Drivers webpage ...");
            try { await Task.Run(() => System.Diagnostics.Process.Start(DellDrivers)); }
            catch (Exception ex) { Log($"Error opening browser: Exception {ex.Message} Trace {ex.StackTrace}"); }
        }

        private async void Button_Links_HPDrivers_Click(object sender, EventArgs e)
        {
            Log("Launching HP Drivers webpage ...");
            try { await Task.Run(() => System.Diagnostics.Process.Start(HPDrivers)); }
            catch (Exception ex) { Log($"Error opening browser: Exception {ex.Message} Trace {ex.StackTrace}"); }
        }

        private async void Button_Links_DellWarranty_Click(object sender, EventArgs e)
        {
            Log("Launching Dell Warranty webpage ...");
            try { await Task.Run(() => System.Diagnostics.Process.Start(DellWarrantyCheck)); }
            catch (Exception ex) { Log($"Error opening browser: Exception {ex.Message} Trace {ex.StackTrace}"); }
        }

        private async void Button_Links_HPWarranty_Click(object sender, EventArgs e)
        {
            Log("Launching HP Warranty webpage ...");
            try { await Task.Run(() => System.Diagnostics.Process.Start(HPWarrantyCheck)); }
            catch (Exception ex) { Log($"Error opening browser: Exception {ex.Message} Trace {ex.StackTrace}"); }
        }

        private async void Button_Links_AppleWarranty_Click(object sender, EventArgs e)
        {
            Log("Launching Apple Warranty webpage ...");
            try { await Task.Run(() => System.Diagnostics.Process.Start(AppleWarrantyCheck)); }
            catch (Exception ex) { Log($"Error opening browser: Exception {ex.Message} Trace {ex.StackTrace}"); }
        }

        private async void Button_Links_ServiceManagment_Click(object sender, EventArgs e)
        {
            Log("Launching Service Managment webpage ...");
            try { await Task.Run(() => System.Diagnostics.Process.Start(ServiceManagment)); }
            catch (Exception ex) { Log($"Error opening browser: Exception {ex.Message} Trace {ex.StackTrace}"); }
        }

        private async void Button_Links_WSP_Click(object sender, EventArgs e)
        {
            Log("Launching WSP webpage ...");
            try { await Task.Run(() => System.Diagnostics.Process.Start(WSP)); }
            catch (Exception ex) { Log($"Error opening browser: Exception {ex.Message} Trace {ex.StackTrace}"); }
        }

        private async void Button_Links_ModernHardware_Click(object sender, EventArgs e)
        {
            Log("Launching Modern Hardware webpage ...");
            try { await Task.Run(() => System.Diagnostics.Process.Start(ModernHardware)); }
            catch (Exception ex) { Log($"Error opening browser: Exception {ex.Message} Trace {ex.StackTrace}"); }
        }

        private async void Button_Links_PCLookup_Click(object sender, EventArgs e)
        {
            Log("Launching PC Lookup webpage ...");
            try { await Task.Run(() => System.Diagnostics.Process.Start(PCLookup)); }
            catch (Exception ex) { Log($"Error opening browser: Exception {ex.Message} Trace {ex.StackTrace}"); }
        }

        private async void Button_Links_ActiveDirectory_Click(object sender, EventArgs e)
        {
            Log("Launching Active Directory webpage ...");
            try { await Task.Run(() => System.Diagnostics.Process.Start(ActiveDirectory)); }
            catch (Exception ex) { Log($"Error opening browser: Exception {ex.Message} Trace {ex.StackTrace}"); }
        }

        private async void Button_Links_Jamf_Click(object sender, EventArgs e)
        {
            Log("Launching Jamf webpage ...");
            try { await Task.Run(() => System.Diagnostics.Process.Start(JamfSupport)); }
            catch (Exception ex) { Log($"Error opening browser: Exception {ex.Message} Trace {ex.StackTrace}"); }
        }



        #endregion Link Events

        #region USMT Events



        private async void CheckGPUpdate()
        { 
            if (USMT_GPUpdateON)
            {
                try
                {
                    Task t = Task.Run(() => RunGpUpdate().Wait());
                    await t;

                    Log("Finished GP Update ...");
                }
                catch (Exception ex) { Log($"Error opening USMT All: Exception {ex.Message} Trace {ex.StackTrace}"); }      
            }
            else
                return;                   
        }

        // USMT Button Events
        private async void Button_USMT_AutoStart_Click(object sender, EventArgs e)
        {
            string path = @"\\ECNASNA05CIFS\onsite\Migrations\mig\usmt_scripts\USMT_full.ps1";

            CheckGPUpdate();
          
            try
            {
                await Task.Run(() =>
                {
                    var startInfo = new ProcessStartInfo()
                    {
                        FileName = "powershell.exe",
                        Arguments = $"-NoProfile -ExecutionPolicy ByPass -File \"{path}\"",
                        Verb = "RunAs",
                        UseShellExecute = false,                    
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                    };
                    Process pro = new Process();
                    pro.EnableRaisingEvents = true;
                    pro.OutputDataReceived += new DataReceivedEventHandler(CMD_OutputDataReceived);
                    pro.ErrorDataReceived += new DataReceivedEventHandler(CMD_ErrorDataReceived);
                    pro.StartInfo = startInfo;
                    pro.Start();
                });
            }
            catch (Exception ex) { Log($"Error opening Unlock: Exception {ex.Message} Trace {ex.StackTrace}"); }
        }

        private static void CMD_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!(sender is Process) || string.IsNullOrEmpty(e.Data))
                return;

            try
            {
                string data = e.Data;
                data.Trim();
                MigLog(data);
            }
            catch (Exception ex) { Log("Error while displaying output data: \n\n" + ex.Message); }
        }

        private static void CMD_ErrorDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!(sender is Process) || string.IsNullOrEmpty(e.Data))
                return;

            try
            {
                string data = e.Data;
                data.Trim();
                MigLog(data);
            }
            catch (Exception ex) { Log("Error while displaying error data: \n\n" + ex.Message); }
        }

        private async void Button_USMT_Scan_Click(object sender, EventArgs e)
        {
            string path = @"\\ECNASNA05CIFS\onsite\Migrations\mig\usmt.ps1";
            CheckGPUpdate();

            try
            {
                await Task.Run(() =>
                {
                    var startInfo = new ProcessStartInfo()
                    {
                        FileName = "powershell.exe",
                        Arguments = $"-NoProfile -ExecutionPolicy ByPass -File \"{path}\"",
                        Verb = "RunAs",
                        UseShellExecute = false,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                    };
                    Process pro = new Process();
                    pro.EnableRaisingEvents = true;
                    pro.OutputDataReceived += new DataReceivedEventHandler(CMD_OutputDataReceived);
                    pro.ErrorDataReceived += new DataReceivedEventHandler(CMD_ErrorDataReceived);
                    pro.StartInfo = startInfo;
                    pro.Start();
                });
            }
            catch (Exception ex) { Log($"Error opening Unlock: Exception {ex.Message} Trace {ex.StackTrace}"); }
        }

        private void Button_USMT_Load_Click(object sender, EventArgs e)
        {
            try { MigMachine("offlineload"); }
            catch (Exception ex) { Log($"Error opening USMT load: Exception {ex.Message} Trace {ex.StackTrace}"); }
        }

        private void Button_USMT_Up_Click(object sender, EventArgs e)
        {
            try { MigMachine("usmtup"); }
            catch (Exception ex) { Log($"Error opening USMT up: Exception {ex.Message} Trace {ex.StackTrace}"); }
        }

        private void Button_USMT_Down_Click(object sender, EventArgs e)
        {
            try { MigMachine("usmtdown"); }
            catch (Exception ex) { Log($"Error opening USMT down: Exception {ex.Message} Trace {ex.StackTrace}"); }
        }

        // USMT Option Toggle Switch Events
        private void ToggleSwitch_USMT_GPUpdate_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                if (ToggleSwitch_USMT_GPUpdate.Value == true)
                    USMT_GPUpdateON = true;
                else
                    USMT_GPUpdateON = false;
            }
            catch (Exception ex) { Log($"Error toggling switch value: Exception {ex.Message} Trace {ex.StackTrace}"); }
        }

        private void ToggleSwitch_USMT_PostCleanup_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                if (ToggleSwitch_USMT_GPUpdate.Value == true)
                    USMT_PostCleanUpON = true;
                else
                    USMT_PostCleanUpON = false;
            }
            catch (Exception ex) { Log($"Error toggling switch value: Exception {ex.Message} Trace {ex.StackTrace}"); }
        }

        private void ToggleSwitch_USMT_FinishedSound_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                if (ToggleSwitch_USMT_GPUpdate.Value == true)
                    USMT_FinishedSoundON = true;
                else
                    USMT_FinishedSoundON = false;
            }
            catch (Exception ex) { Log($"Error toggling switch value: Exception {ex.Message} Trace {ex.StackTrace}"); }
        }

        private void ToggleSwitch_USMT_PostCompare_ValueChanged(object sender, EventArgs e)
        {
            try
            {
                if (ToggleSwitch_USMT_GPUpdate.Value == true)
                    USMT_PostCompareON = true;
                else
                    USMT_PostCompareON = false;
            }
            catch (Exception ex) { Log($"Error toggling switch value: Exception {ex.Message} Trace {ex.StackTrace}"); }
        }


        #endregion USMT Events

        #region Menu Events





        #endregion Menu Events

        #region Tool Strip Menu Events

        private void refreshToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try { UpdateForm(this); }
            catch (Exception ex) { Log($"Error refreshing form: Exception {ex.Message} Trace {ex.StackTrace}"); }
        }

        private void exitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try { Application.Exit(); }
            catch (Exception ex) { Log($"Error exiting application: Exception {ex.Message} Trace {ex.StackTrace}"); }
        }

        private void restartOmnicromStripMenuItem_Click(object sender, EventArgs e)
        {
            try { Application.Restart(); }
            catch (Exception ex) { Log($"Error exiting application: Exception {ex.Message} Trace {ex.StackTrace}"); }
        }

        private void aboutToolStripMenuItem_Click(object sender, EventArgs e)
        {
            try 
            {
                AboutBoxForm aboutbox = new AboutBoxForm();        
                aboutbox.StartPosition = FormStartPosition.CenterParent;
                aboutbox.TopMost = true;
                aboutbox.Show();
                aboutbox.BringToFront();
                aboutbox.Focus();
                aboutbox.TopMost = false;
            }
            catch (Exception ex) { Log($"Error loading about box: Exception {ex.Message} Trace {ex.StackTrace}"); }
        }

        private void radPanel3_Paint(object sender, PaintEventArgs e)
        {

        }

        #endregion Tool Strip Menu Events

        private void RichTextBox_Logs_USMT_TextChanged(object sender, EventArgs e)
        {

        }
    }


    /// <summary>
    /// Collection of shared utility functions
    /// </summary>
    public static class Utility
    {
        /// <summary>
        /// Check if process current process is run as Administrator.
        /// </summary>
        /// <returns></returns>
        public static bool IsRunAsAdmin()
        {
            try
            {
                using (WindowsIdentity identify = WindowsIdentity.GetCurrent())
                {
                    WindowsPrincipal principal = new WindowsPrincipal(identify);
                    return principal.IsInRole(WindowsBuiltInRole.Administrator);
                }
            }
            catch { return false; }
        }

        /// <summary>
        /// Restart application in adminstrator mode
        /// </summary>
        /// <param name="arguments">Command line argument for restart instance. Leave blank to use current startup arguments</param>
        public static void RestartApplicationAsAdmin(string arguments = null)
        {
            ProcessExecutor executor = new ProcessExecutor();
            executor.RunAsAdministrator = true;
            executor.Application = Environment.GetCommandLineArgs()[0];
            if (string.IsNullOrEmpty(arguments))
            {
                executor.Arguments = string.Join(" ", Environment.GetCommandLineArgs().Skip(1));
            }
            else executor.Arguments = arguments;
            executor.ShowConsoleWindow = true;
            executor.Execute(false);
            Environment.Exit(0);
        }

        private static bool isChecked = false;

        public static bool NullifyUAC()
        {
            try
            {
                RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", true);
                key.SetValue("EnableLUA", "0", RegistryValueKind.DWord);
                key.Close();

                if (!isChecked)
                    Log($"UAC modificaion success."); isChecked = true;

                return true;
            }
            catch 
            {
                if (!isChecked)
                    Log($"Error: UAC modificaion has failed."); isChecked = true;

                return false; 
            }
        }

        public static bool isUACEnabled()
        {
            bool result = false;

            try
            {
                RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Policies\System", true);
                var val = key.GetValue("EnableLUA").ToString();

                if (val == "0")
                    result = true;
                else
                    result = false;

                key.Close();
                return result;
            }
            catch { return false; }
        }

    }
}
