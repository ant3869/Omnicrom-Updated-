using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using Telerik.WinControls.UI;

namespace Omnicrom
{
    public abstract class Global
    {
        #region Paths

        //// Temp Path
        //public static readonly string stoplock_path = Path.Combine(ant_internal_path, @"apps\");

        // Server Paths
        public static readonly string sh_path = Path.Combine(@"\\wonsiteprog", "c$", "tools");
        public static readonly string onsite_path = Path.Combine(@"\\ECNASNA05CIFS", "onsite");
        public static readonly string ant_resources_path = Path.Combine(@"\\mig", "c$", "mig", "Ant", "Resources");
        //public static readonly string ant_internal_path = @"G:\Work\Resources\";
        public static readonly string phont800_path = @"\\phont80020us";
        public static readonly string pcmover_path = Path.Combine(sh_path, @"PC_Mover");
        public static readonly string ant_psscript_path = Path.Combine(ant_resources_path, "Scripts", "ModdedScripts", "PowerShell"); // (UnlockDrives.ps1)
        public static readonly string ant_script_path = Path.Combine(ant_resources_path, "Scripts"); // (bl.ps1)
        public static readonly string ant_app_path = Path.Combine(ant_resources_path, @"Apps");
        public static readonly string phont_path = Path.Combine(onsite_path, @"Migrations");
        public static readonly string ryan_path = Path.Combine(phont_path, @"mig");
        public static readonly string usmt_path = Path.Combine(ryan_path, @"usmt_scripts");
        public static readonly string updown_path = Path.Combine(phont_path, @"USMT_UP_DOWN_WIN10");
        public static readonly string store_path = Path.Combine(phont_path, "Store");
        public static readonly string installers_path = Path.Combine(ryan_path, "installs");
        public static readonly string shares_path = Path.Combine(phont800_path, "shares", "SrvOps_WSOnsite");
        public static readonly string app_shares_path = Path.Combine(phont800_path, @"shares", "Applications", "Workstation", "Third_Party");
        public static readonly string driver_wse_path = Path.Combine(phont800_path, @"wse", "Drivers");


        // App Paths
        public static readonly string apppath_Caffiene = Path.Combine(ant_app_path, @"caffeine.exe");
        public static readonly string apppath_OMT = Path.Combine(ant_app_path, @"OMT.exe");
        public static readonly string apppath_USMT = Path.Combine(ant_app_path, @"USMTApps.exe");
        public static readonly string apppath_AppInstaller = Path.Combine(ant_app_path, @"OMTapp.exe");
        public static readonly string apppath_Mig = Path.Combine(ant_app_path, @"mig.bat");
        public static readonly string apppath_Omnicrom = Path.Combine(ant_app_path, @"Omnicrom.exe");
        //public static readonly string apppath_UnlockDrive = Path.Combine(ant_psscript_path, @"UnlockDrives.ps1"); // ant_psscript_path
        public static readonly string apppath_UnlockDrive = Path.Combine(ant_script_path, @"bl.ps1"); // ant_psscript_path

        // Local Directories
        public static readonly string users_local_path = @"C:\Users\";
        public static readonly string usmt_local_path = @"C:\USMT\";
        public static readonly string AppData_Outlook_path = @"AppData\Local\Microsoft\Outlook";
        public static readonly string Roaming_Outlook_path = @"Roaming\Local\Microsoft\Outlook";
        public static readonly string Old_Outlook_path = @"Local Settings\Application Data\Microsoft\Outlook";

        // Environment Directories
        public static readonly string UserProfile = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);                     // User Profile Folder
        public static readonly string ApplicationData = Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData);             // Current User's Application Data
        public static readonly string CommonApplicationData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData); // All User's Application Data
        public static readonly string CommonProgramFiles = Environment.GetFolderPath(Environment.SpecialFolder.CommonProgramFiles);       // Program Files
        public static readonly string Cookies = Environment.GetFolderPath(Environment.SpecialFolder.Cookies);                             // Internet Cookie
        public static readonly string Desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);                             // Logical Desktop
        public static readonly string DesktopDirectory = Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory);           // Physical Desktop
        public static readonly string Favorites = Environment.GetFolderPath(Environment.SpecialFolder.Favorites);                         // Favorites
        public static readonly string History = Environment.GetFolderPath(Environment.SpecialFolder.History);                             // Internet History
        public static readonly string InternetCache = Environment.GetFolderPath(Environment.SpecialFolder.InternetCache);                 // Internet Cache
        public static readonly string MyComputer = Environment.GetFolderPath(Environment.SpecialFolder.MyComputer);                       // "My Computer" Folder
        public static readonly string MyDocuments = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);                     // "My Documents" Folder
        public static readonly string MyMusic = Environment.GetFolderPath(Environment.SpecialFolder.MyMusic);                             // "My Music" Folder
        public static readonly string MyPictures = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);                       // "My Pictures" Folder
        public static readonly string Personal = Environment.GetFolderPath(Environment.SpecialFolder.Personal);                           // "My Document" Folder
        public static readonly string ProgramFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);                   // Program files Folder
        public static readonly string Programs = Environment.GetFolderPath(Environment.SpecialFolder.Programs);                           // Programs Folder
        public static readonly string Recent = Environment.GetFolderPath(Environment.SpecialFolder.Recent);                               // Recent Folder
        public static readonly string SendTo = Environment.GetFolderPath(Environment.SpecialFolder.SendTo);                               // "Sent to" Folder
        public static readonly string StartMenu = Environment.GetFolderPath(Environment.SpecialFolder.StartMenu);                         // Start Menu
        public static readonly string Startup = Environment.GetFolderPath(Environment.SpecialFolder.Startup);                             // Startup
        public static readonly string SystemFolder = Environment.GetFolderPath(Environment.SpecialFolder.System);                         // System Folder
        public static readonly string Templates = Environment.GetFolderPath(Environment.SpecialFolder.Templates);                         // Document Templates


        #endregion Paths

        #region URLs


        // URLs
        public static readonly string ModernHardware = "https://teams.wal-mart.com/sites/PCHW2";
        public static readonly string ServiceManagment = "https://walmartglobal.service-now.com/nav_to.do?uri=%2Fhome.do";
        public static readonly string WSP = "https://walmartglobal.service-now.com/wm_sp?id=nr_index";
        public static readonly string ActiveDirectory = "https://walmartglobal.service-now.com/wm_sp?id=sc_cat_item_guide&sys_id=b3234c3b4fab8700e4cd49cf0310c7d7";
        public static readonly string DellLink = "https://www.dell.com/support/home/en-us?app=drivers&gacd=9694607-1007-5761040-266781430-0&dgc=st&gclsrc=aw.ds&ds_rl=1286093&gclid=EAIaIQobChMI5vuPg5mb9QIVkRTUAR3YkA4wEAAYASAAEgI7gfD_BwE";
        public static readonly string TransferOwner = "https://walmartglobal.service-now.com/wm_sp?id=sc_cat_item&sys_id=62296ef6db46474015cc777b8c961923";
        public static readonly string PCLookup = "https://fdsreports.wal-mart.com/ReportServer/Pages/ReportViewer.aspx?/CustomerReports/CM_SystemSearch";
        public static readonly string ServiceNow = "https://walmartglobal.service-now.com/navpage.do";
        public static readonly string HPDrivers = "https://support.hp.com/us-en/drivers/laptops";
        public static readonly string DellDrivers = "https://www.dell.com/support/home/en-us?app=drivers&gacd=9694607-1007-5761040-266781430-0&dgc=st&ds_rl=1286093&gclid=EAIaIQobChMI_I3l6NG79QIVzxbUAR2_UQGmEAAYASAAEgIqJfD_BwE&gclsrc=aw.ds&nclid=RjYyVuRJm86N_pfO5LVdl9YvvcGzYTGFqPzBUc4nJfcEryFlsr6ZvGJM7cbQiKwD";
        public static readonly string DellWarrantyCheck = "https://www.dell.com/support/home/en-us?app=warranty";
        public static readonly string HPWarrantyCheck = "https://support.hp.com/us-en/checkwarranty";
        public static readonly string FedEx = "https://www.fedex.com/fcl/?appName=fclfsm&locale=us_en&step3URL=https%3A%2F%2Fwww.fedex.com%2Fshipping%2FshipEntryAction.do%3Fmethod%3DdoRegistration%26link%3D1%26locale%3Den_US%26urlparams%3Dus%26sType%3DF&returnurl=https%3A%2F%2Fwww.fedex.com%2Fshipping%2FshipEntryAction.do%3Fmethod%3DdoEntry%26link%3D1%26locale%3Den_US%26urlparams%3Dus%26sType%3DF&programIndicator=0";
        public static readonly string JamfSupport = "https://www.jamf.com/support/";
        public static readonly string AppleWarrantyCheck = "https://checkcoverage.apple.com/";
        public static readonly string DellCommand = "https://www.dell.com/support/kbdoc/en-us/000177325/dell-command-update";

        // CMD Commands 
        public static readonly string ShowNetworks = "COMMAND > NETSH WLAN SET HOSTEDNETWORK MODE=ALLOW SSID=”The Bad One” KEY=”Badman7*";
        public static readonly string logout = "logoff";

        // Scripts
        public static readonly string CleanMgr = @"\\mig\c$\mig\Ant\Scripts\AntScripts\CleanMgr.ps1";


        #endregion URLs


        // Lists
        public static readonly string[] ExcludeStrings = { "Public", "Administrator", "Default", "Default User", "All Users", "DevToolsUser", "DefaultUser", "DefaultUser0", "TEMP", "TEMP0" };
        public static readonly string[] ElevatedStrings = { "SA-", "AD-", "DA-", "sa-", "ad-", "da-" };
        public static readonly string[] ErrorStrings = { "error", "errors", "fail", "failure", "unable", "exception", "exceptions", "unqualified", "unsuitable",
                                                         "removed", "stopped", "corrupt", "corrupted", "insufficient", "denied", "access denied", "unrecoverable",
                                                         "crash", "fault", "flaw", "glitch", "blunder", "mistake", "inaccuracy", "typo", "wrong", "restricted", "stuck",
                                                         "incorrect", "defect", "unreliable", "unreliability", "irregular", "irregularity", "incapable", "obsolete",
                                                         "inconsistance", "inconsistency", "uncertainty", "uncertain", "weak", "weakness", "vulnerability", "vulnerable",
                                                         "security risk", "risk", "unstable", "instability", "impossible", "blocked", "block", "blocking", "kicked",
                                                         "inappropriate", "unresponsive", "stopped responding", "not responding", "frozen", "lost", "missing", "bug", "bugs",
                                                         "timed out", "limit", "unlikly", "unfortunate", "unfortunately", "disconnected", "improper", "not found", "not suitable",
                                                         "inapt", "unfit", "illegal", "false", "untrue", "invalid", "violation", "volitale", "volatile", "strained", "erratic",
                                                         "unproductive", "dirty", "unpredictable", "full", "overload", "overloaded", "unacceptable", "stagnant", "difficult",
                                                         "on hold", "not allowed", "retained", "limited", "harm", "harmful", "broken", "breaking", "not connected", "outdated",
                                                         "obsolescent", "discarded", "disused", "unusable", "expired", "expiring","defunct", "inoperative", "dormant", "dead", "dying",
                                                         "untrustworthy", "not trusted", "danger", "dangerous", "unworkable", "useless", "not active", "deactivated", "decommissioned",
                                                         "latent", "arrested", "locked", "bitlocked", "encrypted", "suspended", "not responding", "nonresponsive", "exhausted",
                                                         "depleted", "empty", "overheat", "drain", "drained", "decreased", "waste", "cripple", "crippled", "feeble", "dissipate",
                                                         "undetermined", "indeterminable", "unresolved", "flagged", "defective", "defect", "unsuccessful", "cross-thread", "incompatible",
                                                         "conflict", "conflicting", "opposing", "irreconcilable", "incapable", "unsuitable", "incongruous", "poor", "substandard", "inadequate"};
        public static readonly string[] SuccessStrings = { "success", "successful", "successfully", "complete", "completed", "finished", "added", "cleaned", "found", "connected", "gained", "created", "saved",
                                                           "uploaded", "recovered", "granted", "true", "healthy", "active", "activated", "unlocked", "accepted", "approved", "set", "passed",
                                                           "recieved", "gathered"};
        public static List<int> RunningProcessIDs { get; set; }

        // Forms
        public static MainForm mainform { get; set; }


        #region Performance Counters


        // Performance Counters   
        public static PerformanceCounter CPU_Counter;
        public static PerformanceCounter RAM_Counter;
        public static PerformanceCounter DSKAVG_Counter;
        public static PerformanceCounter IDLEAVG_Counter;
        public static PerformanceCounter PGE_Counter;

        public static PerformanceCounter DSK_Counter_Local;
        public static PerformanceCounter IDLE_Counter_Local;
        public static PerformanceCounter DSKR_Counter_Local;
        public static PerformanceCounter DSKW_Counter_Local;

        public static PerformanceCounter DSK_Counter_Removable;
        public static PerformanceCounter IDLE_Counter_Removable;
        public static PerformanceCounter DSKR_Counter_Removable;
        public static PerformanceCounter DSKW_Counter_Removable;


        #endregion Performance Counters


        #region Controls and Objects


        // Controls
        public static RichTextBox CurrentLogBox { get; set; }
        public static RichTextBox omnicromlogbox { get; set; }
        public static RichTextBox USMTLogBox { get; set; }
        public static Stopwatch OmnicromRunTime { get; set; }
        public static Stopwatch IdleTimer { get; set; }

        // Objects   
        public static LogicalDiskObject LocalDrive;
        public static LogicalDiskObject Removable;
        public static LogicalDiskObject UnknownDisk;
        public static DeviceObject LocalMachine;


        #endregion Controls and Objects


        #region Variables


        // Colors
        public static Color BlueTextColor = Color.SkyBlue;
        public static Color GrayButtonTextColor = Color.FromArgb(64, 64, 64);
        public static Color DimGrayTextColor = Color.DimGray;
        public static Color WhiteTextColor = Color.White;

        // Booleons 
        public static bool RemovableDriveFound { get; set; }
        public static bool RemovableBitlocked { get; set; }
        public static bool SoundOn { get; set; }
        public static bool ShowElevated { get; set; }
        public static bool DiagResult { get; set; }
        public static bool USMT_GPUpdateON { get; set; }
        public static bool USMT_PostCleanUpON { get; set; }
        public static bool USMT_FinishedSoundON { get; set; }
        public static bool USMT_PostCompareON { get; set; }

        public static bool NullifyUACAttemptMade = false;

        // Integers
        public readonly static long USED_LIMIT = 140000000000;
        public static int StopLockID { get; set; }

        // Strings
        public static string OmnicromExternalLogPath { get; set; }


        #endregion Variables
    }
}
