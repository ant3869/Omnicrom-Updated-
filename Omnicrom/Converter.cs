using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Omnicrom
{
    public static class Converter
    {
        static readonly string[] SizeSuffixes = { "bytes", "KB", "MB", "GB", "TB", "PB", "EB", "ZB", "YB" };

        public static string ConvertByteSize(long size)
        {
            string postfix = "Bytes";
            double result = size;

            if (size >= 1099511627776) // 1 TB
            {
                result = (double)size / 1099511627776;
                postfix = "TB";
            }
            if (size >= 1073741824)  //1 GB
            {
                result = (double)size / 1073741824;
                postfix = "GB";
            }
            else if (size >= 1048576)//more that 1 MB
            {
                result = (double)size / 1048576;
                postfix = "MB";
            }
            else if (size >= 1024)//more that 1 KB
            {
                result = (double)size / 1024;
                postfix = "KB";
            }

            result = Math.Round(result, 1);

            return result.ToString() + " " + postfix;
        }

        //public static string GetFileSize(this FileInfo file)
        //{
        //    try
        //    {
        //        double sizeinbytes = file.Length;
        //        double sizeinkbytes = Math.Round((sizeinbytes / 1024));
        //        double sizeinmbytes = Math.Round((sizeinkbytes / 1024));
        //        double sizeingbytes = Math.Round((sizeinmbytes / 1024));

        //        if (sizeingbytes > 1)
        //            return string.Format("{0} GB", sizeingbytes);
        //        else if (sizeinmbytes > 1)
        //            return string.Format("{0} MB", sizeinmbytes);
        //        else if (sizeinkbytes > 1)
        //            return string.Format("{0} KB", sizeinkbytes);
        //        else
        //            return string.Format("{0} B", sizeinbytes);
        //    }
        //    catch { return "Error Getting Size"; }
        //}

        static string SizeSuffix(Int64 value)
        {
            if (value < 0) { return "-" + SizeSuffix(-value); }
            if (value == 0) { return "0.0 bytes"; }

            int mag = (int)Math.Log(value, 1024);
            decimal adjustedSize = (decimal)value / (1L << (mag * 10));

            return string.Format("{0:n1} {1}", adjustedSize, SizeSuffixes[mag]);
        }

        public static string ConvertDriveType(DriveType type)
        {
            switch (type)
            {
                case DriveType.NoRootDirectory: return ("No root directory. Drive type could not be determined.");
                case DriveType.Removable: return ("Removable drive.");
                case DriveType.Fixed: return ("Local hard disk.");
                case DriveType.Network: return ("Network disk.");
                case DriveType.CDRom: return ("Compact disk.");
                case DriveType.Ram: return ("RAM disk.");
                default: return ("Drive type could not be determined.");
            }
        }

        public static string ConvertPhysicalMemomory(int rawnum)
        {
            float num = ((float)(((float)rawnum) / (1024.0 * 1024.0 * 1024.0)));

            switch (num)
            {
                case float _ when (num > 127): return "128 GB";
                case float _ when (num < 125 && num > 33): return "64 GB";
                case float _ when (num < 35 && num > 29): return "32 GB";
                case float _ when (num < 20 && num > 13): return "16 GB";
                case float _ when (num < 10 && num > 12): return "8 GB";
                case float _ when (num < 7 && num > 2): return "4 GB";
                default: return "Physical Memory could not be determined.";
            }
        }

        public static double ConvertFreePercentage(long total, long free)
        {
            double percentage = 100 * (double)(total - free) / total;

            return Math.Round(percentage, 1);
        }

        public static double ConvertUsedPercentage(long total, long used)
        {
            double percentage = 100 * (double)(total - used) / total;

            return Math.Round(percentage, 1);
        }

        public static string ConvertAgeString(TimeSpan span)
        {
            return string.Format("{0:0}", span.Days / 365.25);
        }

        public static string ConvertUpTime(TimeSpan span)
        {
            string formatted = string.Format("{0}{1}{2}{3}",
                span.Duration().Days > 0 ? string.Format("{0:0}d{1}, ", span.Days, span.Days == 1 ? string.Empty : "") : string.Empty,
                span.Duration().Hours > 0 ? string.Format("{0:0}h{1}, ", span.Hours, span.Hours == 1 ? string.Empty : "") : string.Empty,
                span.Duration().Minutes > 0 ? string.Format("{0:0}m{1}, ", span.Minutes, span.Minutes == 1 ? string.Empty : "") : string.Empty,
                span.Duration().Seconds > 0 ? string.Format("{0:0}s{1}", span.Seconds, span.Seconds == 1 ? string.Empty : "") : string.Empty);

            if (formatted.EndsWith(", ")) formatted = formatted.Substring(0, formatted.Length - 2);

            if (string.IsNullOrEmpty(formatted)) formatted = "0 s";

            return formatted;
        }
        public static string ConvertAvailability(int availability)
        {
            switch (availability)
            {
                case 1: return "Other";
                case 2: return "Unknown";
                case 3: return "Running or Full Power";
                case 4: return "Warning";
                case 5: return "In Test";
                case 6: return "Not Applicable";
                case 7: return "Power Off";
                case 8: return "Off Line";
                case 9: return "Off Duty";
                case 10: return "Degraded";
                case 11: return "Not Installed";
                case 12: return "Install Error";
                case 13: return "Power Save - Unknown";
                case 14: return "Power Save - Low Power Mode";
                case 15: return "Power Save - Standby";
                case 16: return "Power Cycle";
                case 17: return "Power Save - Warning";
                default: return "Unknown";
            }
        }
    }
}
