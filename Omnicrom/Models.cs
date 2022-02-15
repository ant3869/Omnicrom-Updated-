using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Runtime.CompilerServices;

namespace Omnicrom
{
    class Models
    {
    }

    public abstract class BaseModel : INotifyPropertyChanged
    {
        private string _name;
        private string _description;

        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }

        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected bool SetProperty<T>(ref T field, T newValue, [CallerMemberName] string propertyName = null)
        {
            if (!EqualityComparer<T>.Default.Equals(field, newValue))
            {
                field = newValue;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

                return true;
            }

            return false;
        }
    }

    public class LogicalDiskObject : BaseModel
    {
        private DriveType _mediatype;

        private bool _isBitlocked;

        private long _totalspace;
        private long _freespace;
        private long _usedspace;
        private long _filecountsize;

        private int _directorycount;
        private int _filecount;

        private double _freepercent;
        private double _usedpercent;

        private string _instancetext;
        private string _mediatypetext;
        private string _totalspacetext;
        private string _freespacetext;
        private string _usedspacetext;
        private string _freepercenttext;
        private string _usedpercenttext;
        private string _directorycounttext;
        private string _filecounttext;
        private string _filecountsizetext;

        public LogicalDiskObject(string name, DriveType mediatype)
        {
            Description = "Details of logical disk connected to the device.";

            if (name.Length > 1)
                Name = name[0].ToString();
            else
                Name = name;

            MediaType = mediatype;
        }
        public DriveType MediaType
        {
            get => _mediatype;
            set => SetProperty(ref _mediatype, value);
        }
        public long TotalSpace
        {
            get => Functions.GetTotalDiskSize(Name);
            set => SetProperty(ref _totalspace, value);
        }
        public long FreeSpace
        {
            get => Functions.GetFreeDiskSpace(Name);
            set => SetProperty(ref _freespace, value);
        }
        public long UsedSpace
        {
            get => Functions.GetUsedDiskSpace(Name);
            set => SetProperty(ref _usedspace, value);
        }
        public long FileCountSize
        {
            get => _filecountsize;
            set => SetProperty(ref _filecountsize, value);
        }
        public int DirectoryCount
        {
            get => _directorycount;
            set => SetProperty(ref _directorycount, value);
        }
        public int FileCount
        {
            get => _filecount;
            set => SetProperty(ref _filecount, value);
        }
        public double UsedSpacePercent
        {
            get => Functions.GetUsedSpacePercentage(Name);
            set => SetProperty(ref _usedpercent, value);
        }
        public double FreeSpacePercent
        {
            get => Converter.ConvertUsedPercentage(TotalSpace, UsedSpace);
            set => SetProperty(ref _freepercent, value);
        }
        public bool IsBitLocked
        {
            get => Functions.GetBitlockerStatus(Name);
            set => SetProperty(ref _isBitlocked, value);
        }
        public string MediaTypeText
        {
            get => Converter.ConvertDriveType(MediaType);
            set => SetProperty(ref _mediatypetext, value);
        }
        public string InstanceText
        {
            get => Functions.GetDriveInstanceNumberText(Name);
            set => SetProperty(ref _instancetext, value);
        }
        public string TotalSpaceText
        {
            get => Converter.ConvertByteSize(TotalSpace);
            set => SetProperty(ref _totalspacetext, value);
        }
        public string FreeSpaceText
        {
            get => Converter.ConvertByteSize(FreeSpace);
            set => SetProperty(ref _freespacetext, value);
        }
        public string UsedSpaceText
        {
            get => Converter.ConvertByteSize(UsedSpace);
            set => SetProperty(ref _usedspacetext, value);
        }
        public string FreeSpacePercentText
        {
            get => " (" + FreeSpacePercent.ToString() + "%) ";
            set => SetProperty(ref _freepercenttext, value);
        }
        public string UsedSpacePercentText
        {
            get => " (" + UsedSpacePercent.ToString() + "%) ";
            set => SetProperty(ref _usedpercenttext, value);
        }
        public string DirectoryCountText
        {
            get => _directorycount.ToString();
            set => SetProperty(ref _directorycounttext, value);
        }
        public string FileCountText
        {
            get => _filecount.ToString();
            set => SetProperty(ref _filecounttext, value);
        }
        public string FileCountSizeText
        {
            get => Converter.ConvertByteSize(_filecountsize);
            set => SetProperty(ref _filecountsizetext, value);
        }
    }

    public class DeviceObject : BaseModel
    {
        private TimeSpan _systemuptimespan;

        private string _servicetag;
        private string _systemuptimetext;

        private string _make;
        private string _model;

        public DeviceObject(string name, string model, string tag)
        {
            Description = "Local machine system details as parsed from Win32.";
            Name = name;
            Model = model;
            ServiceTag = tag;
        }
        public string Make
        {
            get => _make;
            set => SetProperty(ref _make, value);
        }
        public string Model
        {
            get => _model;
            set => SetProperty(ref _model, value);
        }
        public string ServiceTag
        {
            get => _servicetag;
            set => SetProperty(ref _servicetag, value);
        }
        public TimeSpan SystemUptimeSpan
        {
            get => _systemuptimespan;
            set => SetProperty(ref _systemuptimespan, value);
        }
        public string SystemUptime
        {
            get => GetTimeSpan();
            set => SetProperty(ref _systemuptimetext, value);
        }

        internal string GetTimeSpan()
        {
            var ticks = Stopwatch.GetTimestamp();
            var uptime = ((double)ticks) / Stopwatch.Frequency;
            var uptimeSpan = TimeSpan.FromSeconds(uptime);

            return Converter.ConvertUpTime(uptimeSpan);
        }
    }
}
