using Microsoft.WindowsAPICodePack.Shell;
using Microsoft.WindowsAPICodePack.Shell.PropertySystem;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Runtime.InteropServices;
using System.Security.AccessControl;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using Telerik.WinControls.UI;
using Usb.Events;
using static Omnicrom.Global;
using static Omnicrom.RichTextBoxExtensions;

namespace Omnicrom
{
    #region Enums and Structs

    [StructLayout(LayoutKind.Sequential)]
    internal struct WTS_SESSION_INFO
    {
        public Int32 SessionID;
        [MarshalAs(UnmanagedType.LPStr)]
        public String pWinStationName;
        public WTS_CONNECTSTATE_CLASS State;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct Attribute
    {
        public byte AttributeID;
        public ushort Flags;
        public byte Value;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        public byte[] VendorData;
    }

    internal enum WTS_CONNECTSTATE_CLASS
    {
        WTSActive,
        WTSConnected,
        WTSConnectQuery,
        WTSShadow,
        WTSDisconnected,
        WTSIdle,
        WTSListen,
        WTSReset,
        WTSDown,
        WTSInit
    }

    internal enum WTS_INFO_CLASS
    {
        WTSInitialProgram,
        WTSApplicationName,
        WTSWorkingDirectory,
        WTSOEMId,
        WTSSessionId,
        WTSUserName,
        WTSWinStationName,
        WTSDomainName,
        WTSConnectState,
        WTSClientBuildNumber,
        WTSClientName,
        WTSClientDirectory,
        WTSClientProductId,
        WTSClientHardwareId,
        WTSClientAddress,
        WTSClientDisplay,
        WTSClientProtocolType,
        WTSIdleTime,
        WTSLogonTime,
        WTSIncomingBytes,
        WTSOutgoingBytes,
        WTSIncomingFrames,
        WTSOutgoingFrames,
        WTSClientInfo,
        WTSSessionInfo
    }

    #endregion

    public static class Functions
    {
        #region DLL Imports

        [DllImport("wtsapi32.dll", SetLastError = true)]
        static extern bool WTSLogoffSession(IntPtr hServer, int SessionId, bool bWait);

        [DllImport("Wtsapi32.dll")]
        static extern bool WTSQuerySessionInformation(
        System.IntPtr hServer, int sessionId, WTS_INFO_CLASS wtsInfoClass, out System.IntPtr ppBuffer, out uint pBytesReturned);

        [DllImport("wtsapi32.dll", SetLastError = true)]
        static extern IntPtr WTSOpenServer([MarshalAs(UnmanagedType.LPStr)] String pServerName);

        [DllImport("wtsapi32.dll")]
        static extern void WTSCloseServer(IntPtr hServer);

        [DllImport("wtsapi32.dll", SetLastError = true)]
        static extern Int32 WTSEnumerateSessions(IntPtr hServer, [MarshalAs(UnmanagedType.U4)] Int32 Reserved, [MarshalAs(UnmanagedType.U4)] Int32 Version, ref IntPtr ppSessionInfo, [MarshalAs(UnmanagedType.U4)] ref Int32 pCount);

        [DllImport("wtsapi32.dll")]
        static extern void WTSFreeMemory(IntPtr pMemory);

        #endregion

        #region Disk Information

        private static void Button(object sender, RoutedEventArgs e)
        {
            RadButton btn = (RadButton)sender;

            if (btn.Name == "btnCancel")
            {
                // code goes here
            }
            else if (btn.Name == "btnSubmit")
            {
                // sub code go here
            }
        }

        public static void LoadLocalDrive()
        {
            try
            {
                var name = GetLocalDirectoryDrivePath();
                DriveInfo drive = new DriveInfo(name);

                if (drive.IsReady)
                    LocalDrive = new LogicalDiskObject(drive.Name, drive.DriveType);
            }
            catch (Exception e) { Log(string.Format("Exception {0} Trace {1}", e.Message, e.StackTrace)); }
        }

        public static string GetLocalDirectoryDrivePath()
        {
            string name = null;

            try
            {
                var localdrive = Path.GetPathRoot(Environment.SystemDirectory);
                var drivename = localdrive[0].ToString();

                if (drivename != null && drivename.Length > 0)
                    name = drivename;
            }
            catch (Exception ex) { Log($"Error: Exception {ex.Message} Trace {ex.StackTrace}"); }

            return name;
        }


        public static void LoadRemovableDisk()
        {
            RemovableDriveFound = false;
            RemovableBitlocked = false;

            try
            {
                DriveInfo[] alldrives = DriveInfo.GetDrives();

                foreach (var drive in alldrives)
                {
                    if (drive.IsReady && drive.DriveType == DriveType.Removable)
                    {
                        SetDirAccess(drive.Name);

                        if (Directory.Exists(drive.Name + "Users"))
                        {
                            //bool locked = IsFileLocked(Path.Combine(drive.Name, "Users"));

                            Removable = new LogicalDiskObject(drive.Name, drive.DriveType);
                            RemovableDriveFound = true;

                            break;
                        }
                    }
                }
            }
            catch (Exception ex) { Log($"Error: Exception {ex.Message} Trace {ex.StackTrace}"); }

            //GetRemovableDrivesTEST();
        }

        public static void GetRemovableDrivesTEST()
        {
            try
            {
                ManagementScope ms = new ManagementScope();
                ObjectQuery oq = new ObjectQuery("SELECT DeviceID, VolumeName FROM Win32_LogicalDisk WHERE DriveType=2");
                ManagementObjectSearcher mos = new ManagementObjectSearcher(ms, oq);
                ManagementObjectCollection moc = mos.Get();

                foreach (ManagementObject mo in moc)
                {
                    string path = (System.IO.Path.Combine(mo["DeviceID"].ToString(), " "));
                    DirectoryInfo newDI = new DirectoryInfo(path);
                    newDI.Refresh();

                    SetDirAccess(path);
                    FileInfo[] files = newDI.GetFiles();
                    FileSystemInfo[] info = newDI.GetFileSystemInfos();
                    //Now Access/Show/Return details information from "files" and "info" objects

                    LoadRemovableDisk();
                }
            }
            catch (Exception ex) { Log($"Exception {ex.Message} Trace {ex.StackTrace}"); }
        }

        public static void SetDirAccess(string filePath)
        {
            WindowsIdentity identify = WindowsIdentity.GetCurrent();
            string user = identify.Name;

            try { Utility.NullifyUAC(); }
            catch (Exception ex) { Log($"Error: Exception {ex.Message} Trace {ex.StackTrace}"); }

            try
            {
                FileInfo fi = new FileInfo(filePath);
                System.Security.AccessControl.FileSecurity fileSecurity = fi.GetAccessControl();
                fileSecurity.AddAccessRule(new FileSystemAccessRule($"{user}", FileSystemRights.FullControl, AccessControlType.Allow));
                fileSecurity.AddAccessRule(new FileSystemAccessRule("Users", FileSystemRights.FullControl, AccessControlType.Allow));
                fi.SetAccessControl(fileSecurity);
            }
            catch (Exception ex) { Log($"Exception {ex.Message} Trace {ex.StackTrace}"); }     
        }

        // Physical Disk Getters
        public static async void CheckRemovableSatus()
        {
            if (RemovableDriveFound)
                return;

            var statustask = await GetRemovableStatusAsync();
            RemovableDriveFound = statustask;

            if (RemovableDriveFound && DSK_Counter_Removable == null)
                LoadRemovableCounters();
            else if (!RemovableDriveFound && DSK_Counter_Removable != null)
                DSK_Counter_Removable = null;
        }

        private static async Task<bool> GetRemovableStatusAsync()
        {
            try{ var result = Utility.NullifyUAC(); }
            catch (Exception ex) { Log($"Error: Exception {ex.Message} Trace {ex.StackTrace}"); }

            DriveInfo[] drives = DriveInfo.GetDrives();

            foreach (var drive in drives)
            {
                try
                {
                    RemovableBitlocked = GetBitlockerStatus(drive.Name);

                    //if (RemovableBitlocked)
                    //{
                    //    var bitunlockertask = Task.Run(async () => await RunBitunlocker());
                    //    bitunlockertask.Wait();
                    //}
                }
                catch (Exception ex) { Log($"Error: Exception {ex.Message} Trace {ex.StackTrace}"); }

                try
                {
                    if (drive.IsReady && drive.DriveType == DriveType.Removable && !RemovableBitlocked)
                    {
                        if (Directory.Exists(drive.Name + "Users"))
                        {
                            LoadRemovableDisk();
                            //GetRemovableDrivesTEST();
                            return true;
                        }
                    }
                }
                catch (Exception ex) { Log($"Error: Exception {ex.Message} Trace {ex.StackTrace}"); }
            }

            return false;
        }


        public static void InitalizeRemovableEvents()
        {
            UsbEventWatcher usbEventWatcher = new UsbEventWatcher(true, false);

            usbEventWatcher.UsbDriveEjected += (_, path) =>
            {
                Log("Ejected: " + path);

                if (Removable.Name != path[0].ToString())
                    return;
                else
                    RemovableDriveFound = false;
            };

            usbEventWatcher.UsbDriveMounted += (_, path) =>
            {
                Log("Mounted: " + path);

                CheckRemovableSatus();
            };

            usbEventWatcher.Start();
        }

        public static bool GetBitlockerStatus(string drivename)
        {
            RemovableBitlocked = false;

            try
            {
                IShellProperty prop = ShellObject.FromParsingName(drivename).Properties.GetProperty("System.Volume.BitLockerProtection");
                int? bitLockerProtectionStatus = (prop as ShellProperty<int?>).Value;

                if (bitLockerProtectionStatus.HasValue && (bitLockerProtectionStatus == 1 || bitLockerProtectionStatus == 3 || bitLockerProtectionStatus == 5))
                    return true;               
            }
            catch (Exception e) { Log(string.Format("Exception {0} Trace {1}", e.Message, e.StackTrace)); }

            return false;
        }

        public static string GetDriveInstanceNumberText(string drivename)
        {
            var cat = new PerformanceCounterCategory("PhysicalDisk");
            string[] inames = cat.GetInstanceNames();
            string name = "";

            foreach (string iname in inames)
            {
                if (iname.Contains(drivename))
                    name = iname;
            }

            return name;
        }

        public static int GetUsedSpacePercentage(string drivename)
        {
            DriveInfo Drive = new DriveInfo(drivename);
            int percentage = 0;

            if (Drive.IsReady)
            {
                long usedspace = GetUsedDiskSpace(drivename);
                float UsedSpacePerc = (usedspace / (float)Drive.TotalSize) * 100;
                percentage = (int)UsedSpacePerc;
            }

            return percentage;
        }

        public static int GetFreeSpacePercentage(string drivename)
        {
            DriveInfo Drive = new DriveInfo(drivename);
            int percentage = 0;

            if (Drive.IsReady)
            {
                float freeSpacePerc = (Drive.AvailableFreeSpace / (float)Drive.TotalSize) * 100;
                percentage = (int)freeSpacePerc;
            }

            return percentage;
        }

        public static long GetTotalDiskSize(string drivename)
        {
            DriveInfo Drive = new DriveInfo(drivename);
            long totalsize = 0;

            if (Drive.IsReady)
                totalsize = Drive.TotalSize;

            return totalsize;
        }

        public static long GetFreeDiskSpace(string drivename)
        {
            DriveInfo Drive = new DriveInfo(drivename);
            long availablefreespace = 0;

            if (Drive.IsReady)
                availablefreespace = Drive.AvailableFreeSpace;

            return availablefreespace;
        }

        public static long GetUsedDiskSpace(string drivename)
        {
            DriveInfo Drive = new DriveInfo(drivename);
            long usedspace = 0;

            if (Drive.IsReady)
                usedspace = (Drive.TotalSize - Drive.AvailableFreeSpace);

            return usedspace;
        }

        public static List<string> GetSMARTinfo()
        {
            List<string> attributes = new List<string>();

            try
            {
                Attribute AtributeInfo;
                ManagementScope Scope = new ManagementScope(String.Format("\\\\{0}\\root\\WMI", "localhost"), null);
                Scope.Connect();
                ObjectQuery Query = new ObjectQuery("SELECT VendorSpecific FROM MSStorageDriver_ATAPISmartData");
                ManagementObjectSearcher Searcher = new ManagementObjectSearcher(Scope, Query);
                byte SpinRetryCount = 0x0A;
                int Delta = 12;

                foreach (ManagementObject WmiObject in Searcher.Get())
                {
                    byte[] VendorSpecific = (byte[])WmiObject["VendorSpecific"];

                    for (int offset = 2; offset < VendorSpecific.Length;)
                    {
                        if (VendorSpecific[offset] == SpinRetryCount)
                        {
                            IntPtr buffer = IntPtr.Zero;
                            try
                            {
                                buffer = Marshal.AllocHGlobal(Delta);
                                Marshal.Copy(VendorSpecific, offset, buffer, Delta);
                                AtributeInfo = (Attribute)Marshal.PtrToStructure(buffer, typeof(Attribute));
                                attributes.Add("AttributeID: " + AtributeInfo.AttributeID.ToString());
                                attributes.Add("Flags: " + AtributeInfo.Flags.ToString());
                                attributes.Add("Value: " + AtributeInfo.Value.ToString());
                                attributes.Add("Data: " + (BitConverter.ToInt32(AtributeInfo.VendorData, 0)).ToString());
                            }
                            finally
                            {
                                if (buffer != IntPtr.Zero)
                                    Marshal.FreeHGlobal(buffer);
                            }
                        }
                        offset += Delta;
                    }
                }
            }
            catch (Exception e) { Log(string.Format("Exception {0} Trace {1}", e.Message, e.StackTrace)); }

            return attributes;
        }

        public static async Task RunBitunlocker()
        {
            try
            {
                await Task.Run(() =>
                {
                    var pro = new Process();
 
                    pro.StartInfo = new ProcessStartInfo()
                    {
                        FileName = "powershell.exe",
                        Arguments = $"-NoProfile -ExecutionPolicy ByPass -File \"{apppath_UnlockDrive}\"",
                        Verb = "runas",
                        UseShellExecute = false,
                        RedirectStandardInput = true,
                        RedirectStandardOutput = true,
                        RedirectStandardError = true,
                        CreateNoWindow = true,
                    };

                    pro.EnableRaisingEvents = true;
                    pro.OutputDataReceived += new DataReceivedEventHandler(CMD_OutputDataReceived);
                    pro.ErrorDataReceived += new DataReceivedEventHandler(CMD_ErrorDataReceived);
                    pro.Exited += new EventHandler(RemoveNumExitEvent);

                    StartCMDProcess(pro);
                });
            }
            catch (Exception ex) { Log($"Error opening Unlock: Exception {ex.Message} Trace {ex.StackTrace}"); }
        }


        #endregion Disk Information

        #region Disk Compare Methods


        public static string GetStatusDescriptionText()
        {
            string result = "No issues found.";
            int dskidle = GetDiskIdleAvg();

            if (!RemovableDriveFound)
                result = "Removable drive not found.";
            else if (Removable.UsedSpace > USED_LIMIT)
                result = "Removable disk space over transfer limit.";
            else if (Removable.TotalSpace > LocalDrive.TotalSpace)
                result = "Local disk available space size insufficient.";
            else if (LocalDrive.UsedSpacePercent >= 99)
                result = "Local disk storage is full.";
            else if (LocalDrive.UsedSpacePercent >= 90)
                result = "Local disk space used is above 90%.";

            if (dskidle >= 80)
            {
                if (IdleTimer.IsRunning)
                {
                    if (IdleTimer.ElapsedMilliseconds >= (1000 * 60 * 10))
                        result = "Average disk idle time 10 or more minutes.";
                }
                else
                    IdleTimer.Start();
            }
            else if (dskidle < 80)
            {
                IdleTimer.Stop();
                IdleTimer.Reset();
            }

            return result;
        }

        public static string GetStatusIndicatorText(string text)
        {
            string result;
            switch (text)
            {
                case "Removable drive not found.": result = "Warning"; break;
                case "Removable disk space over transfer limit.": result = "Error"; break;
                case "Local disk available space size insufficient.": result = "Error"; break;
                case "Local disk storage is full.": result = "Error"; break;
                case "Local disk space used is above 90%.": result = "Warning"; break;
                case "Average disk idle time 10 or more minutes.": result = "Warning"; break;
                case "No issues found.": result = "OK"; break;
                default: result = "OK"; break;
            }

            return result;
        }

        #endregion Disk Compare Methods

        #region Machine Information


        public static void LoadDeviceObject()
        {
            LocalMachine = GetLocalDeviceObjectAsync();
        }

        private static DeviceObject GetLocalDeviceObjectAsync()
        {
            DeviceObject _device = null;
            
            try
            {
                string hostname = GetHostName();
                string servicetag = GetServiceTag();
                //string make = MotherboardInfo.Manufacturer;
                string model = MotherboardInfo.Model;

                _device = new DeviceObject(hostname, model, servicetag);
            }
            catch (Exception e) { Log("Error. Unable to create Device Object.\n\n" + e.ToString()); }
           
            return _device;
        }

        public static string GetHostName()
        {
            return Environment.MachineName;
        }

        public static string GetServiceTag()
        {
            string dellServiceTag = "N/A";

            try
            {
                ManagementClass wmi = new ManagementClass("Win32_Bios");

                foreach (ManagementObject bios in wmi.GetInstances())
                    dellServiceTag = bios.Properties["Serialnumber"].Value.ToString().Trim();
            }
            catch (Exception e) { Log("Error. Unable to parse service tag.\n\n" + e.ToString()); }

            return dellServiceTag;
        }

        public static string GetSystemUpTime()
        {
            var ticks = Stopwatch.GetTimestamp();
            var uptime = ((double)ticks) / Stopwatch.Frequency;
            var uptimeSpan = TimeSpan.FromSeconds(uptime);

            return Converter.ConvertUpTime(uptimeSpan);
        }

        #endregion Machine Information

        #region Motherboard Info

        static public class MotherboardInfo
        {
            private static readonly ManagementObjectSearcher systemsearcher = new ManagementObjectSearcher("root\\WMI", "SELECT * FROM MS_SystemInformation");
            private static readonly ManagementObjectSearcher baseboardSearcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_BaseBoard");
            private static readonly ManagementObjectSearcher motherboardSearcher = new ManagementObjectSearcher("root\\CIMV2", "SELECT * FROM Win32_MotherboardDevice");
            static public string Availability
            {
                get
                {
                    try
                    {
                        foreach (ManagementObject queryObj in motherboardSearcher.Get())
                            return Converter.ConvertAvailability(int.Parse(queryObj["Availability"].ToString()));

                        return "";
                    }
                    catch (Exception e) { return "" + e; }
                }
            }
            static public string InstallDate
            {
                get
                {
                    try
                    {
                        foreach (ManagementObject queryObj in baseboardSearcher.Get())
                            return queryObj["InstallDate"].ToString();

                        return "";
                    }
                    catch (Exception e) { return "" + e.Message; }
                }
            }
            static public string Manufacturer
            {
                get
                {
                    try
                    {
                        foreach (ManagementObject queryObj in baseboardSearcher.Get())
                            return queryObj["Manufacturer"].ToString();

                        return "";
                    }
                    catch (Exception e) { return "" + e.Message; }
                }
            }
            static public string Model
            {
                get
                {
                    try
                    {
                        foreach (ManagementObject queryObj in systemsearcher.Get())
                            return queryObj["SystemProductName"].ToString();

                        return "";
                    }
                    catch (ManagementException e) { return "An error occurred while querying for WMI data: " + e.Message; }
                }
            }
        }

        #endregion Motherboard Info

        #region Performance Getters


        private static List<PerformanceCounter> pcounters;
        public static void LoadPerformanceCounters()
        {
            string LocalInstance = LocalDrive.InstanceText;        
            pcounters = new List<PerformanceCounter>();

            try
            {
                CPU_Counter = new PerformanceCounter("Processor", "% Processor Time", "_Total"); pcounters.Add(CPU_Counter);
                RAM_Counter = new PerformanceCounter("Memory", "% Committed Bytes In Use", null); pcounters.Add(RAM_Counter);
                DSKAVG_Counter = new PerformanceCounter("PhysicalDisk", "% Disk Time", "_Total"); pcounters.Add(DSKAVG_Counter);
                IDLEAVG_Counter = new PerformanceCounter("PhysicalDisk", "% Idle Time", "_Total"); pcounters.Add(IDLEAVG_Counter);
                PGE_Counter = new PerformanceCounter("Paging File", "% Usage", "_Total"); pcounters.Add(PGE_Counter);

                DSK_Counter_Local = new PerformanceCounter("PhysicalDisk", "% Disk Time", LocalInstance); pcounters.Add(DSK_Counter_Local);
                IDLE_Counter_Local = new PerformanceCounter("PhysicalDisk", "% Idle Time", LocalInstance); pcounters.Add(IDLE_Counter_Local);
                DSKR_Counter_Local = new PerformanceCounter("PhysicalDisk", "Disk Read Bytes/sec", LocalInstance); pcounters.Add(DSKR_Counter_Local);
                DSKW_Counter_Local = new PerformanceCounter("PhysicalDisk", "Disk Write Bytes/sec", LocalInstance); pcounters.Add(DSKW_Counter_Local);
            }
            catch (Exception ex) { Log($"Error Loading performance counters: Exception {ex.Message} Trace {ex.StackTrace}"); }

            if (RemovableDriveFound)
                LoadRemovableCounters();
        }

        public static void LoadRemovableCounters()
        {
            string RemovableInstance = Removable.InstanceText;
            DSK_Counter_Removable = null;
            IDLE_Counter_Removable = null;
            DSKR_Counter_Removable = null;
            DSKW_Counter_Removable = null;

            try
            {
                var DSK_Counter = new PerformanceCounter("PhysicalDisk", "% Disk Time", RemovableInstance);
                var IDLE_Counter = new PerformanceCounter("PhysicalDisk", "% Idle Time", RemovableInstance);
                var DSKR_Counter = new PerformanceCounter("PhysicalDisk", "Disk Read Bytes/sec", RemovableInstance);
                var DSKW_Counter = new PerformanceCounter("PhysicalDisk", "Disk Write Bytes/sec", RemovableInstance);

                if (DSK_Counter != null)
                    DSK_Counter_Removable = DSK_Counter;

                if (IDLE_Counter != null)
                    IDLE_Counter_Removable = IDLE_Counter;

                if (DSKR_Counter != null)
                    DSKR_Counter_Removable = DSKR_Counter;

                if (DSKW_Counter != null)
                    DSKW_Counter_Removable = DSKW_Counter;
            }
            catch (Exception ex) { Log($"Error: Exception {ex.Message} Trace {ex.StackTrace}"); }
            finally
            {
                if (DSK_Counter_Removable != null)
                    pcounters.Add(DSK_Counter_Removable);

                if (IDLE_Counter_Removable != null)
                    pcounters.Add(IDLE_Counter_Removable);

                if (DSKR_Counter_Removable != null)
                    pcounters.Add(DSKR_Counter_Removable);

                if (DSKW_Counter_Removable != null)
                    pcounters.Add(DSKW_Counter_Removable);
            }  
        }

        public static void DisposePerformanceCounters()
        {
            try
            {
                if (pcounters != null)
                {
                    foreach (PerformanceCounter counter in pcounters)
                    { counter.Dispose(); }
                }
            }
            finally
            { PerformanceCounter.CloseSharedResources(); }
        }

        /// <summary>
        /// Gets the average percentage of all physical disk activity. Return value is screened to ensure it remains in bounds.
        /// </summary>
        /// <returns>Returns an interger.</returns>
        public static int GetDiskUseAvg()
        {
            float result = DSKAVG_Counter.NextValue();

            return GetCorrectedProgressValue(result);
        }

        /// <summary>
        /// Gets the average percentage of all physical disk activity. Return value is screened to ensure it remains in bounds.
        /// </summary>
        /// <returns>Returns an interger.</returns>
        public static int GetDiskIdleAvg()
        {
            float result = IDLEAVG_Counter.NextValue();

            return GetCorrectedProgressValue(result);
        }

        /// <summary>
        /// Gets the average percentage of all processor activity. Return value is screened to ensure it remains in bounds.
        /// </summary>
        /// <returns>Returns an interger.</returns>
        public static int GetCPUUse()
        {
            float result = CPU_Counter.NextValue();

            return GetCorrectedProgressValue(result);
        }

        /// <summary>
        /// Gets the average percentage of memory bytes committed to use. Return value is screened to ensure it remains in bounds.
        /// </summary>
        /// <returns>Returns an interger.</returns>
        public static int GetRAMUse()
        {
            float result = RAM_Counter.NextValue();

            return GetCorrectedProgressValue(result);
        }

        /// <summary>
        /// Gets the average percentage of all physical disks read speed per second (bytes/sec). Return value is screened to ensure it remains in bounds.
        /// </summary>
        /// <returns>Returns an interger.</returns>
        public static int GetDiskReadAvg()
        {
            PerformanceCounter myCounter = new PerformanceCounter("PhysicalDisk", "Avg. Disk sec/Read", "_Total");
            float result = myCounter.NextValue();

            return (int)result;
        }

        /// <summary>
        /// Gets the average percentage of all physical disks write speed per second (bytes/sec). Return value is screened to ensure it remains in bounds.
        /// </summary>
        /// <returns>Returns an interger.</returns>
        public static int GetDiskWriteAvg()
        {
            PerformanceCounter myCounter = new PerformanceCounter("PhysicalDisk", "Avg. Disk sec/Write", "_Total");
            float result = myCounter.NextValue();

            return (int)result;
        }

        /// <summary>
        /// Gets the average percentage of all physical disks write speed per second (bytes/sec). Return value is screened to ensure it remains in bounds.
        /// </summary>
        /// <returns>Returns an interger.</returns>
        public static int GetPGEUse()
        {
            float result = PGE_Counter.NextValue();

            return GetCorrectedProgressValue(result);
        }

        /// <summary>
        /// Gets the percentage of a physical disk's usage. Return value is screened to ensure it remains in bounds.
        /// </summary>
        /// <param name="iname">
        /// Name of instance to parse disk use for.
        /// </param>
        /// <returns>Returns an interger.</returns>
        public static int GetDiskUse(PerformanceCounter myCounter)
        {
            float result = 0;

            try { result = myCounter.NextValue(); }
            catch (Exception ex) { Log($"Error: Exception {ex.Message} Trace {ex.StackTrace}"); }       
            
            return GetCorrectedProgressValue((int)result);
        }

        /// <summary>
        /// Gets the percentage of a physical disk's idle time. Return value is screened to ensure it remains in bounds.
        /// </summary>
        /// <param name="iname">
        /// Name of instance to parse disk idle time for.
        /// </param>
        /// <returns>Returns an interger.</returns>
        public static int GetDiskIdlePercentage(PerformanceCounter myCounter)
        {
            float result = 0;

            try { result = myCounter.NextValue(); }
            catch (Exception ex) { Log($"Error: Exception {ex.Message} Trace {ex.StackTrace}"); }

            return GetCorrectedProgressValue((int)result);
        }

        /// <summary>
        /// Gets the rate of bytes read per second for a physical disk.
        /// </summary>
        /// <param name="iname">
        /// Name of instance to parse disk read rate for.
        /// </param>
        /// <returns>Returns an interger.</returns>
        public static int GetDiskReadPerSec(string drivename)
        {
            var iname = GetDriveInstanceNumberText(drivename);
            PerformanceCounter myCounter = new PerformanceCounter("PhysicalDisk", "Disk Read Bytes/sec", iname);
            float fcounter = myCounter.NextValue();
            int result = (int)fcounter;

            return result;
        }

        /// <summary>
        /// Gets the rate of bytes written per second for a physical disk.
        /// </summary>
        /// <param name="iname">
        /// Name of instance to parse disk write rate for.
        /// </param>
        /// <returns>Returns an interger.</returns>
        public static int GetDiskWritePerSec(string drivename)
        {
            var iname = GetDriveInstanceNumberText(drivename);
            PerformanceCounter myCounter = new PerformanceCounter("PhysicalDisk", "Disk Write Bytes/sec", iname);
            float fcounter = myCounter.NextValue();
            int result = (int)fcounter;

            return result;
        }

        /// <summary>
        /// Evaluates amount and adjust accordingly to adhere to proper performance counter bounds.
        /// </summary>
        /// <param name="dvalue">
        /// Value to screen and adjust to proper bounds for a performance counter (0-100).
        /// </param>
        /// <returns>Returns an interger.</returns>
        public static int GetCorrectedProgressValue(float dvalue)
        {
            var value = (int)dvalue;
            int result = 0;

            switch (value)
            {
                case int _ when (value >= 100): result = 100; break;
                case int _ when (value <= 0): result = 0; break;
                default: result = value; break;
            }

            return result;
        }


        #endregion Performance Getters

        #region Text Functions


        public static void UpdateSplashScreenText(string text)
        {
            Program.splashform.Label_Splash_UpdateText.InvokeIfRequired(() =>
            { 
                Program.splashform.Label_Splash_UpdateText.Text = text; 
            });
        }

        public static bool WriteLog(string message)
        {
            try
            {
                var name = $"{DateTime.Now.ToString("MMddyyyy")}_OmniLog.txt";
                string filename = Path.Combine(MyDocuments, name);
                OmnicromExternalLogPath = MyDocuments;

                if (File.Exists(filename))
                {
                    using (StreamWriter writer = File.AppendText(filename))
                    {
                        writer.WriteLine($"{DateTime.Now} : {message}");
                    }
                }
                else
                {
                    using (StreamWriter writer = File.CreateText(filename))
                    {
                        writer.WriteLine($"{DateTime.Now} : {message}");
                    }
                }
            }
            catch (Exception ex) { Log($"Error at WriteLog: Exception {ex.Message} Trace {ex.StackTrace}"); }
 
            return true;
        }

        private static string GetTextBoxText(TextBox textbox)
        {
            string returnValue = null;

            if (textbox.InvokeRequired)
                textbox.Invoke((MethodInvoker)
                               delegate { returnValue = GetTextBoxText(textbox); });
            else
                return textbox.Text;

            return returnValue;
        }

        public static void Find(string text, bool matchCase = false, RichTextBox rtb = null)
        {
            try
            {
                if (rtb == null)
                    rtb = CurrentLogBox;

                int startPos;

                StringComparison type;
                type = matchCase == true ? StringComparison.Ordinal : StringComparison.OrdinalIgnoreCase;

                startPos = rtb.Text.IndexOf(text, type);

                if (!(startPos > 0)) { return; }
                else
                {
                    rtb.Select(startPos, text.Length);
                    rtb.ScrollToCaret();
                    //rtb.Focus();      
                }
            }
            catch (Exception e) { Log($"Exception {e.Message} Trace {e.StackTrace}"); }
        }

        public static void CopyTextFromLabel(RadLabel label)
        {
            string labeltext = label.Text;

            System.Windows.Clipboard.Clear();
            System.Windows.Clipboard.SetText(labeltext);
        }

        #endregion Text Functions

        #region Windows User Accounts

        public static async Task<List<string>> GetUserProfilesListAsync()
        {
            // Retereiving list of users to determine which profile will be migrated.
            List<string> userlist = new List<string>();
            List<string> localusers = GetLocalWindowsAccounts();
            List<string> removableusers = GetRemovableWindowsAccounts();
            localusers.AddRange(removableusers);

            await Task.Run(() =>
            {
                foreach (string user in localusers)
                {
                    string name = user.Substring(user.LastIndexOf(@"\") + 1); name.Trim();
                    string tag = name.Substring(0, 3); tag.Trim();
                    string id = (user.Substring(0, 2) + " " + name);

                    bool elevated;
                    if (Global.ElevatedStrings.Contains(tag) && !Global.ShowElevated) { elevated = true; }
                    else elevated = false;

                    bool excluded;
                    if (Global.ExcludeStrings.Contains(name)) { excluded = true; }
                    else excluded = false;

                    if (!elevated && !excluded && !userlist.Contains(id)) { userlist.Add(id); }
                    else continue;
                }
            });

            return userlist;
        }

        public static List<string> GetLocalWindowsAccounts()
        {
            List<string> accounts = new List<string>();
            string drivename = GetLocalDirectoryDrivePath();

            string[] dirs = Directory.GetDirectories(drivename + "Users");

            foreach (string name in dirs)
                accounts.Add(name);

            return accounts;
        }

        public static List<string> GetRemovableWindowsAccounts()
        {
            List<string> accounts = new List<string>();
            string[] dirs;

            DriveInfo[] drives = DriveInfo.GetDrives();

            foreach (var drive in drives)
            {
                if (drive.IsReady && drive.DriveType == DriveType.Removable)
                {
                    if (Directory.Exists(drive.Name + "Users"))
                    {
                        dirs = Directory.GetDirectories(drive.Name + "Users");

                        foreach (string name in dirs)
                            accounts.Add(name);
                    }
                }
            }

            return accounts;
        }

        public static string GetWorkgroupOrDomainName()
        {
            string name = string.Empty;

            SelectQuery searchQuery = new SelectQuery("Win32_ComputerSystem");
            ManagementObjectSearcher searchObj = new ManagementObjectSearcher(searchQuery);

            foreach (ManagementObject obj in searchObj.Get())
            {
                if ((bool)obj["partofdomain"] != true)
                    name = string.Format("{0}", obj["workgroup"]);
                else
                    name = string.Format("{0}", obj["workgroup"]);
            }

            return name;
        }


        internal static List<int> GetSessionIDs(IntPtr server)
        {
            List<int> sessionIds = new List<int>();
            IntPtr buffer = IntPtr.Zero;
            int count = 0;
            int retval = WTSEnumerateSessions(server, 0, 1, ref buffer, ref count);
            int dataSize = Marshal.SizeOf(typeof(WTS_SESSION_INFO));
            Int64 current = (int)buffer;

            if (retval != 0)
            {
                for (int i = 0; i < count; i++)
                {
                    WTS_SESSION_INFO si = (WTS_SESSION_INFO)Marshal.PtrToStructure((IntPtr)current, typeof(WTS_SESSION_INFO));
                    current += dataSize;
                    sessionIds.Add(si.SessionID);
                }
                WTSFreeMemory(buffer);
            }
            return sessionIds;
        }

        internal static bool LogOffUser(string userName, IntPtr server)
        {
            userName = userName.Trim().ToUpper();
            List<int> sessions = GetSessionIDs(server);
            Dictionary<string, int> userSessionDictionary = GetUserSessionDictionary(server, sessions);

            if (userSessionDictionary.ContainsKey(userName))
                return WTSLogoffSession(server, userSessionDictionary[userName], true);
            else
                return false;
        }

        private static Dictionary<string, int> GetUserSessionDictionary(IntPtr server, List<int> sessions)
        {
            Dictionary<string, int> userSession = new Dictionary<string, int>();

            foreach (var sessionId in sessions)
            {
                string uName = GetUserName(sessionId, server);

                if (!string.IsNullOrWhiteSpace(uName))
                    userSession.Add(uName, sessionId);
            }

            return userSession;
        }

        internal static string GetUserName(int sessionId, IntPtr server)
        {
            IntPtr buffer = IntPtr.Zero;
            string userName = string.Empty;

            try
            {
                uint count;
                WTSQuerySessionInformation(server, sessionId, WTS_INFO_CLASS.WTSUserName, out buffer, out count);
                userName = Marshal.PtrToStringAnsi(buffer).ToUpper().Trim();
            }
            catch (Exception ex) { Log($"Error: Exception {ex.Message} Trace {ex.StackTrace}"); }
            finally { WTSFreeMemory(buffer); }

            return userName;
        }

        public static async Task<List<string>> GetInstalledProgramListAsync()
        {
            // GUID taken from https://docs.microsoft.com/en-us/windows/win32/shell/knownfolderid
            var FODLERID_AppsFolder = new Guid("{1e87508d-89c2-42f0-8a7e-645a0f50ca58}");
            ShellObject appsFolder = (ShellObject)KnownFolderHelper.FromKnownFolderId(FODLERID_AppsFolder);
            List<string> applist = new List<string>();
            int i = 0;

            await Task.Run(() =>
            {
                foreach (var app in (IKnownFolder)appsFolder)
                {
                    i++;

                    string name = app.Name;
                    string appUserModelID = app.ParsingName; // or app.Properties.System.AppUserModel.ID
                                                             // You can even get the Jumbo icon in one shot
                    string ver = app.Properties.System.FileVersion.Value;
                    ImageSource icon = app.Thumbnail.ExtraLargeBitmapSource;

                    applist.Add(name);
                }
            });

            return applist;
        }

        public class CustomSearcher
        {
            public static async Task<List<string>> GetDirectories(string path, string searchPattern = "*",
                System.IO.SearchOption searchOption = System.IO.SearchOption.AllDirectories)
            {
                List<string> directories = null;

                await Task.Run(() =>
                {
                    if (searchOption == System.IO.SearchOption.TopDirectoryOnly)
                        directories = Directory.GetDirectories(path, searchPattern).ToList();

                    directories = new List<string>(GetDirectories(path, searchPattern));

                    for (var i = 0; i < directories.Count; i++)
                        directories.AddRange(GetDirectories(directories[i], searchPattern));
                });


                return directories;
            }

            private static List<string> GetDirectories(string path, string searchPattern)
            {
                try
                {
                    return Directory.GetDirectories(path, searchPattern).ToList();
                }
                catch (Exception e) { Log(string.Format("Exception {0} Trace {1}", e.Message, e.StackTrace)); }
                {
                    return new List<string>();
                }
            }
        }


        #endregion Windows User Accounts

        #region File Management

        public static bool IsFileLocked(string filepath)
        {
            FileInfo file = new FileInfo(filepath);

            try
            {
                FileStream stream = file.Open(FileMode.Open, FileAccess.Read, FileShare.None);
                stream.Close();
            }
            catch (IOException)
            {
                //the file is unavailable because it is:
                //still being written to
                //or being processed by another thread
                //or does not exist (has already been processed)
                return true;
            }

            //file is not locked
            return false;
        }

        public static void KillRunningProcess(string name)
        {
            Process[] runingProcess = Process.GetProcesses();

            for (int i = 0; i < runingProcess.Length; i++)
            {
                if (runingProcess[i].ProcessName == name)
                    runingProcess[i].Kill();
            }
        }

        public static bool FindAndKillProcess(string name)
        {
            foreach (Process clsProcess in Process.GetProcesses())
            {
                if (clsProcess.ProcessName.StartsWith(name))
                {
                    clsProcess.Kill();

                    return true;
                }
            }

            return false;
        }

        public static async void SimpleFileCopyAsync(string sourcePath, string fileName, string targetPath)
        {
            await Task.Run(() =>
            {
                // Use Path class to manipulate file and directory paths.
                string sourceFile = Path.Combine(sourcePath, fileName);
                string destFile = Path.Combine(targetPath, fileName);

                // Create a new target folder if the directory does not exists.
                Directory.CreateDirectory(targetPath);

                if (Directory.Exists(sourcePath))
                {
                    // overwrite the destination file if it already exists.
                    File.Copy(sourceFile, destFile, true);
                }
                else
                    Log("Source path does not exist!");
            });
        }

        public static async void SimpleDirectoryCopyAsync(string sourcePath, string targetPath)
        {
            await Task.Run(() =>
            {
                // Create a new target folder if the directory does not exists.
                Directory.CreateDirectory(targetPath);

                // Get the files in the source folder.
                if (Directory.Exists(sourcePath))
                {
                    string[] files = Directory.GetFiles(sourcePath);

                    // Copy the files and overwrite destination files if they already exist.
                    foreach (string s in files)
                    {
                        // Use static Path methods to extract only the file name from the path.
                        string fileName = Path.GetFileName(s);
                        var destFile = Path.Combine(targetPath, fileName);
                        File.Copy(s, destFile, true);
                    }
                }
                else
                    Log("Source path does not exist!");
            });
        }

        public static async void SimpleFileDeleteAsync(string filepath)
        {
            await Task.Run(() =>
            {
                if (System.IO.File.Exists(filepath))
                {
                    try { System.IO.File.Delete(filepath); }
                    catch (Exception e) { Log(string.Format("Exception {0} Trace {1}", e.Message, e.StackTrace)); }
                }
            });
        }

        public static async void SimpleDirectoryDeleteAsync(string directorypath)
        {
            await Task.Run(() =>
            {
                // Delete a directory. Must be writable or empty.
                try { System.IO.Directory.Delete(directorypath); }
                catch (Exception e) { Log(string.Format("Exception {0} Trace {1}", e.Message, e.StackTrace)); }

                // Delete a directory and all subdirectories with Directory static method...
                if (System.IO.Directory.Exists(directorypath))
                {
                    try { System.IO.Directory.Delete(directorypath, true); }
                    catch (Exception e) { Log(string.Format("Exception {0} Trace {1}", e.Message, e.StackTrace)); }
                }

                // ...or with DirectoryInfo instance method.
                System.IO.DirectoryInfo di = new System.IO.DirectoryInfo(directorypath);

                // Delete this dir and all subdirs.
                try { di.Delete(true); }
                catch (Exception e) { Log(string.Format("Exception {0} Trace {1}", e.Message, e.StackTrace)); }
            });
        }

        /// <summary>
        /// Count the number of lines in the file specified.
        /// </summary>
        /// <param name="filepath">The filename to count lines in.</param>
        /// <returns>The number of lines in the file.</returns>
        public static async Task<int> CountLinesInFileAsync(string filepath)
        {
            int count = 0;

            await Task.Run(() =>
            {
                StreamReader r = new StreamReader(filepath);
                string line;

                while ((line = r.ReadLine()) != null)
                    count++;

                r.Close();
            });
            return count;
        }

        #endregion File Management

        #region System Controls

        /// <summary>
        /// Logout of current Windows user profile, after prompting a confirmation diaglog box.
        /// </summary>
        /// <returns>Returns void.</returns>
        public static async void LogoutCurrentUser()
        {
            try { _ = await Task.Run(() => Process.Start("logout")); }
            catch (Exception e) { Log(string.Format("Exception {0} Trace {1}", e.Message, e.StackTrace)); }
        }

        /// <summary>
        /// Restarts machine after prompting a confirmation diaglog box.
        /// </summary>
        /// <returns>Returns void.</returns>
        public static async void SystemRestart()
        {
            try { _ = await Task.Run(() => Process.Start("restart", "/s /t 0")); }
            catch (Exception e) { Log(string.Format("Exception {0} Trace {1}", e.Message, e.StackTrace)); }    
        }

        /// <summary>
        /// Shutdowns machine after prompting a confirmation diaglog box.
        /// </summary>
        /// <returns>Returns void.</returns>
        public static async void SystemShutDown()
        {
            try { _ = await Task.Run(() => Process.Start("shutdown", "/s /t 0")); }
            catch (Exception e) { Log(string.Format("Exception {0} Trace {1}", e.Message, e.StackTrace)); }
        }

        /// <summary>
        /// Launches Window's Task Manager.
        /// </summary>
        /// <returns>Returns void.</returns>
        public static async void StartTaskManager()
        {
            try { _ = await Task.Run(() => Process.Start("taskmgr.exe")); }
            catch (Exception e) { Log(string.Format("Exception {0} Trace {1}", e.Message, e.StackTrace)); } 
        }

        /// <summary>
        /// Creates and opens a notepad file on the Desktop containg the text from a RichTextBox.
        /// </summary>
        /// <param name="logbox">
        /// The RichTextBox containing the text to parse to notepad.
        /// </param>
        /// <returns>Returns void.</returns>
        public static async void OpenExternalLog(RichTextBox logbox)
        {
            if (logbox == omnicromlogbox)
            {
                try { await Task.Run(() => Process.Start(OmnicromExternalLogPath)); }
                catch (Exception e) { Log($"Error opening omnicrom log: Exception {e.Message} Trace {e.StackTrace}"); }
            }
        }

        /// <summary>
        /// Updates the MainForm controls.
        /// </summary>
        /// <returns>Returns void.</returns>
        public static void UpdateForm(MainForm form)
        {
            form.Refresh();
            System.Windows.Forms.Application.DoEvents();
        }

        public static async Task RunGpUpdate()
        {
            string app = "cmd.exe";
            string com = "/c GPUPdate/force";

            Log("Running GP Update ...");

            try { await Task.Run(async () => { await RunCommandPrompt($"{app} {com}"); }); }
            catch (Exception e) { Log($"Error running GPUpate: Exception {e.Message} Trace {e.StackTrace}"); }
        }

        public static async void CopyOmnicromToDesktop()
        {
            string app = "cmd.exe";
            string com = $"/c copy {apppath_Omnicrom} {Desktop}";

            try { await Task.Run(async () => { await RunCommandPrompt($"{app} {com}"); }); }
            catch (Exception e) { Log($"Error running GPUpate: Exception {e.Message} Trace {e.StackTrace}"); }
        }

        public static bool GetExecutableRunStatus(string appname)
        {
            var result = true;

            try { result = Process.GetProcessesByName(appname).Any(); }
            catch (Exception e) { Log($"Error getting executable run status: Exception {e.Message} Trace {e.StackTrace}"); }

            return result;
        }

        public static async void StartCaffeineApp()
        {
            try
            {
                bool isRunning = GetExecutableRunStatus("caffeine.exe");

                if (isRunning)
                    return;                   

                await Task.Run(() => 
                {
                    Process process = new Process();
                    process.StartInfo.FileName = apppath_Caffiene;
                    process.Start();

                    if (process.Id != 0)
                    {
                        StopLockID = process.Id;
                        Log($"Stop-Lock started successfully.");
                        AddRunningProcessNumToList(StopLockID);
                    }
                    else
                        Log($"Stop-Lock failed to start.");
                });       
            }
            catch (Exception e) { Log($"Error starting Stop-Lock: Exception {e.Message} Trace {e.StackTrace}"); }
        }

        public static async void StopCaffeineAppAsync()
        {
            try
            {
                var num = await KillProcessByID(StopLockID);

                if (num == 0)
                    StopLockID = num;
                else
                    Log("Failed to stop Stop-Lock process");
            }
            catch (Exception e) { Log($"Error stopping Stop-Lock: Exception {e.Message} Trace {e.StackTrace}"); }
        }

        public static async void StartStopLockAsync()
        {
            try { await Task.Run(() => RunProgramAsync("mig.bat", "1")); }
            catch (Exception e) { Log($"Error starting Stop-Lock: Exception {e.Message} Trace {e.StackTrace}"); }
        }

        public static async void StartBitlockUnlockerAsync()
        {
            //try{ await Task.Run(() => RunProgramAsync("mig.bat", "unlock q"));}
            //catch (Exception e) { Log($"Error starting BitUnlocker: Exception {e.Message} Trace {e.StackTrace}"); }

            string path = $@"{apppath_Mig}" + " unlock q";


            try { await Task.Run(() => Process.Start(path)); }
            catch (Exception e) { Log($"Error starting BitUnlocker: Exception {e.Message} Trace {e.StackTrace}"); }
        }

        private static async Task<int> KillProcessByID(int id)
        {
            int incid = id;

            try
            {
                Log($"Stopping process by ID: {id}");

                await Task.Run(() =>
                {
                    ExecutableExtensions.KillProcessAndChildren(incid);

                    if (RunningProcessIDs.Contains(incid))
                        RunningProcessIDs.Remove(incid);

                    incid = 0;
                });
            }
            catch (Exception e) { Log($"Error stopping proccess: Exception {e.Message} Trace {e.StackTrace}"); }

            return incid;
        }

        public static async Task StopProcessAtCloseAsync()
        {
            if (RunningProcessIDs.Count > 0)
            {
                try
                {
                    foreach (var process in RunningProcessIDs)
                        await KillProcessByID(process);
                }
                catch (Exception e) { Log(string.Format("Exception {0} Trace {1}", e.Message, e.StackTrace)); }       
            }
        }

        #endregion System Controls

        #region CMD Command Processor

        public static async Task<int> RunProgramAsync(string app, string arguments = "")
        {
            int idnum = 0;

            if (arguments != "")
                Log($"Running application: {app} with the arguments: {arguments}.");
            else
                Log($"Running application: {app}.");

            try
            {
                await Task.Run(() =>
                {
                    var process = new Process
                    {
                        StartInfo = new ProcessStartInfo()
                        {
                            WorkingDirectory = ant_app_path,
                            UseShellExecute = false,
                            FileName = app,
                            Arguments = arguments,
                            Verb = "RunAs",
                            CreateNoWindow = true,
                        }
                    };

                    process.EnableRaisingEvents = true;
                    process.Exited += new EventHandler(RemoveNumExitEvent);

                    idnum = StartCMDProcess(process);
                });
            }
            catch (Exception e) { Log(string.Format("Exception {0} Trace {1}", e.Message, e.StackTrace)); }

            return idnum;
        }

        // Variables
        private static int CmdID;
        public static bool CmdRunning;
        private static Stopwatch exetime;
        public static int TIME_OUT = 120000;
        public static Process CMDprocess { get; set; }
        public static string CMDapp { get; set; }
        public static string CMDworkdir { get; set; }

        public static RadWaitingBar CMDProcessWaitingBar;
        public static Label CMDProcessLabel;

        public static async void MigMachine(string command)
        {
            StringBuilder sb = new StringBuilder();
    

            switch (command)
            {
                case "all": sb.Append("0"); break;
                case "unlock": sb.Append("unlock"); break;
                case "stoplock": sb.Append("1"); break;
                case "offlinescan": sb.Append("offlinescan"); break;
                case "offlineload": sb.Append("6"); break;
                case "usmtup": sb.Append("7"); break;
                case "usmtdown": sb.Append("8"); break;
                case "usmt": sb.Append("2"); break;
                case "arug": sb.Append("arug"); break;
                case "pcmover": sb.Append("P"); break;
                case "gpupdate": sb.Append("9"); break;
                case "unlockremote": sb.Append("unlockremote"); break;
                case "yubiadd": sb.Append("yubiadd"); break;
                case "yubiremove": sb.Append("yubiremove"); break;
                case "yubibypass": sb.Append("yubibypass"); break;
                case "offscrub": sb.Append("offscrub"); break;
                case "shortcut": sb.Append("shortcut"); break;
                case "gpupdater": sb.Append("gpupdater"); break;
                case "gpupdates": sb.Append("gpupdates"); break;
                case "nero": sb.Append("nero"); break;
                case "sccminstall": sb.Append("sccminstall"); break;
                case "spaceplanning": sb.Append("spaceplanning"); break;
                case "office365": sb.Append("office365"); break;
                case "dragon": sb.Append("dragon"); break;
                case "WOnsiteprog": sb.Append("WOnsiteprog"); break;
                case "Migration": sb.Append("M"); break;
                case "USMT Store": sb.Append("S"); break;
                case "App Installers": sb.Append("7"); break;
            }
            //sb.Append(" q");
            var com = sb.ToString();

            await Task.Run(async () => { await RunCommandPrompt($"{apppath_Mig} {com}"); });

            if (command == "all" || command == "offlinescan" || command == "usmtup")
                StartLogMonitor(@"C:\USMT\ScanState.log");

            if (command == "offlineload" || command == "usmtdown")
                StartLogMonitor(@"C:\USMT\LoadState.log");
        }

        public static void StartLogMonitor(string logpath)
        {
            if (!File.Exists(logpath))
            {
                Log($"Log file {logpath} does not exists");
                return;
            }

            try
            {
                //var monitor = new MonitorLogFile(logpath, "\r\n");
                var monitor = new LiveLogViewer(logpath);
 
                //monitor.OnLine += (s, e) =>
                //{
                //    if (e.Line != null)
                //        MigLog(e.Line);
                //};

                //monitor.Start();


            }
            catch (Exception ex) { Log($"Error starting log monitor: Exception {ex.Message} Trace {ex.StackTrace}"); }           
        }



        public static void SendCMDInput(string input)
        {
            CMDprocess.StandardInput.WriteLine(input);
        }

        public static async Task RunCommandPrompt(string CMDapp, string input = null)
        {
            await Task.Run(() =>
            {
                Log($"Running cmd process [{CMDapp}] ...");

                try
                {
                    CMDprocess = CreateCMDProcess();
                    StartCMDProcess(CMDprocess);

                    //SendCMDInput("pushd " + $"{ant_app_path}");
                    SendCMDInput(CMDapp);

                    if (input != null)
                        SendCMDInput(input);

                    CMDprocess.BeginOutputReadLine();
                    CMDprocess.BeginErrorReadLine();
                    WaitForExitAsync(CMDprocess, 120000);
                }
                catch (Exception ex) { Log("Error running process: \n\n" + ex.Message); }
            });
        }

        private static Process CreateCMDProcess()
        {
            Process pro = new Process();
            exetime = new Stopwatch();
            CmdRunning = false;
            CmdID = 0;

            try
            {
                pro.StartInfo = new ProcessStartInfo()
                {
                    FileName = @"cmd.exe",
                    //Arguments = @"\c ",
                    Verb = "runas",
                    UseShellExecute = false,
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true,
                };

                pro.EnableRaisingEvents = true;
                pro.OutputDataReceived += new DataReceivedEventHandler(CMD_OutputDataReceived);
                pro.ErrorDataReceived += new DataReceivedEventHandler(CMD_ErrorDataReceived);
                pro.Exited += new EventHandler(RemoveNumExitEvent);
            }
            catch (Exception ex) { Log("Error creating process. \n\n" + ex.Message); }

            return pro;
        }

        private static Task<bool> WaitForExitAsync(Process process, int timeout)
        {
            return Task.Run(() => process.WaitForExit(timeout));
        }

        private static void CMD_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            if (!(sender is Process) || string.IsNullOrEmpty(e.Data))
                return;

            try
            {
                string data = e.Data;
                data.Trim();
                Log(data);
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
                Log(data);
            }
            catch (Exception ex) { Log("Error while displaying error data: \n\n" + ex.Message); }
        }

        private static void CMD_FinishProcess(object sender, EventArgs e)
        {
            if (!(sender is Process) || !(sender as Process).HasExited)
                return;

            try
            {
                exetime.Stop();
                CmdRunning = false;
                UpdateWaitingBar();
                if (exetime.ElapsedMilliseconds >= 120000)
                    Log($"Process {CmdID} exited. The process has timed-out.");
                else
                    Log($"Process {CmdID} finished. Elasped time: {Converter.ConvertUpTime(exetime.Elapsed)}");

                CmdID = 0;
            }
            catch (Exception ex) { Log("Error exiting event: \n\n" + ex.Message); }
        }

        private static int StartCMDProcess(Process p)
        {
            try
            {
                int IDnum = 0;
                bool Running = p.Start();

                if (Running)
                {
                    IDnum = p.Id;
                    AddRunningProcessNumToList(IDnum);
                    Log($"Process started successfully.");
                }
                else
                    Log("Error: Process failed to start.");

                return IDnum;
            }
            catch (Exception ex) { Log("Error starting process: \n\n" + ex.Message); }

            return 0;
        }

        private static void RemoveNumExitEvent(object sender, EventArgs e)
        {
            try
            {
                var p = sender as Process;
                var num = p.Id;

                if (p != null)
                {
                    if (p.Id != 0)
                        RemoveStoppedProcessNumFromList(p.Id);
                }
                else
                    Log($"Error: Exit Event failed.).");
            }
            catch (Exception ex) { Log($"Error: Exception {ex.Message} Trace {ex.StackTrace}"); }
        }

        private static void AddRunningProcessNumToList(int num)
        {
            try
            {
                if (RunningProcessIDs.Contains(num) != true)
                {
                    RunningProcessIDs.Add(num);
                    //Log($"{num} added to running applications.");
                }
                else
                    Log($"Error: List of running applications already contains {num}.");
            }
            catch (Exception e) { Log($"Error: Exception {e.Message} Trace {e.StackTrace}"); }
        }

        private static void RemoveStoppedProcessNumFromList(int num)
        {
            try
            {
                if (RunningProcessIDs.Contains(num) == true)
                {
                    RunningProcessIDs.Remove(num);
                    Log($"{num} removed from running applications.");
                }
                else
                    Log($"Error: List of running applications does not contain {num}.");
            }
            catch (Exception e) { Log($"Error: Exception {e.Message} Trace {e.StackTrace}"); }
        }



        private static void UpdateWaitingBar()
        {
            try
            {

            }
            catch (Exception ex) { Log("Error invoking form label: \n\n" + ex.Message); }
        }

        public static bool AbortCMDprocess()
        {
            if (CMDprocess != null)
                CMDprocess.Kill();

            if (!CMDprocess.HasExited)
                return false;

            return true;
        }

        public static bool DisposeCMDprocess()
        {
            if (CMDprocess != null)
                CMDprocess.Dispose();
            else if (!CMDprocess.HasExited)
                return false;

            return true;
        }

        public static async void StartCMDSessionAsync()
        {
            try
            { 
                await Task.Run(() => 
                {
                    Process pro = new Process();
                    pro.StartInfo.FileName = @"cmd.exe";
                    pro.StartInfo.Verb = "RunAs";
                    pro.EnableRaisingEvents = true;
                    pro.Exited += new EventHandler(RemoveNumExitEvent);
                    StartCMDProcess(pro);
                }); 
            }
            catch (Exception e) { Log($"Exception {e.Message} Trace {e.StackTrace}"); }
        }

        public static async void StartPSSessionAsync()
        {
            try
            {
                await Task.Run(() =>
                {
                    Process pro = new Process();
                    pro.StartInfo.FileName = @"powershell.exe";
                    pro.StartInfo.Verb = "RunAs";
                    pro.EnableRaisingEvents = true;
                    pro.Exited += new EventHandler(RemoveNumExitEvent);
                    StartCMDProcess(pro);
                });
            }
            catch (Exception e) { Log($"Exception {e.Message} Trace {e.StackTrace}"); }
        }

        #endregion CMD.EXE Command Processor
    }
}
