using System;
using System.IO;
using System.Text;
using System.Timers;
using Omnicrom.Properties;

/* 
    A simple log file monitor class for .NET

    Uses a threaded timer to check for changes in the file, if the file length has changed then the unread
    section of the file is read and parsed into lines before being passed back to the event handler.

    Sample Usage:

     var monitor = new MonitorLogFile("C:\USMT\loadstate.log", "\r\n");

     monitor.OnLine += (s, e) =>
     {
         WARNING.. this will be a different thread...
         Console.WriteLine(e.Line);
     };

     monitor.Start(); 

*/

using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using System.Windows.Data;
using Timer = System.Threading.Timer;
using System.Threading.Tasks;
using System.Globalization;

namespace Omnicrom
{


    /// <summary>
    ///     Interaction logic for MainWindow.xaml
    /// </summary>
    public class LiveLogViewer : INotifyPropertyChanged
    {
        private readonly IEnumerable<EncodingInfo> _encodings = Encoding.GetEncodings().OrderBy(e => e.DisplayName);
        private readonly ObservableCollection<FileMonitor> _fileMonitors = new ObservableCollection<FileMonitor>();
        private readonly System.Threading.Timer _refreshTimer;
        private string _font;
        private DateTime? _lastUpdateDateTime;
        private string _lastUpdated;
        private FileMonitor _lastUpdatedViewModel;
        private FileMonitor _selectedItem;

        public LiveLogViewer(string dirpath)
        {
            DirectoryInfo dirinfo = new DirectoryInfo(dirpath);
            _refreshTimer = CreateRefreshTimer();

            BeginViewer(dirinfo);
        }

        private void BeginViewer(DirectoryInfo dirinfo)
        {
            try
            {
                foreach (var fileName in dirinfo.GetFiles())
                {
                    AddFileMonitor(fileName.FullName);
                }
            }
            catch (Exception ex) { RichTextBoxExtensions.Log(string.Format("Exception {0} Trace {1}", ex.Message, ex.StackTrace)); }
        }

        /// <summary>
        ///     Gets the available encodings.
        /// </summary>
        /// <value>
        ///     The encodings.
        /// </value>
        public IEnumerable<EncodingInfo> Encodings
        {
            get { return _encodings; }
        }

        public string LastUpdated
        {
            get { return _lastUpdated; }
            set
            {
                if (value == _lastUpdated) return;
                _lastUpdated = value;
                OnPropertyChanged();
            }
        }

        public FileMonitor SelectedItem
        {
            get { return _selectedItem; }
            set
            {
                if (Equals(value, _selectedItem)) return;
                _selectedItem = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<FileMonitor> FileMonitors
        {
            get { return _fileMonitors; }
        }

        public string Font
        {
            get { return _font; }
            set
            {
                if (value == _font) return;
                _font = value;
                OnPropertyChanged();
            }
        }

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        private Timer CreateRefreshTimer()
        {
            var timer = new Timer(state => RefreshLastUpdatedText());
            timer.Change((DateTime.Now.Date.AddDays(1) - DateTime.Now), TimeSpan.FromDays(1));
            //this.Closing += DisposeTimer;
            
            return timer;
        }

        private void DisposeTimer(object s, CancelEventArgs e)
        {
            _refreshTimer.Change(Timeout.InfiniteTimeSpan, Timeout.InfiniteTimeSpan);
            _refreshTimer.Dispose();
        }

        private void AddFileMonitor(string filepath)
        {
            var existingMonitor = FileMonitors.FirstOrDefault(m => string.Equals(m.FilePath, filepath, StringComparison.CurrentCultureIgnoreCase));

            if (existingMonitor != null)
            {
                // Already being monitored
                SelectedItem = existingMonitor;
                return;
            }

            var monitorViewModel = new FileMonitor(filepath);
            //var monitorViewModel = new FileMonitor(filepath, GetFileNameForPath(filepath), Settings.Default.DefaultEncoding, Settings.Default.BufferedRead);
            //monitorViewModel.Renamed += MonitorViewModelOnRenamed;
            //monitorViewModel.Updated += MonitorViewModelOnUpdated;

            FileMonitors.Add(monitorViewModel);
            SelectedItem = monitorViewModel;
        }

        private void MonitorViewModelOnUpdated(FileMonitor obj)
        {
            _lastUpdateDateTime = DateTime.Now;
            _lastUpdatedViewModel = obj;
            RefreshLastUpdatedText();
        }

        private void MonitorViewModelOnRenamed(FileMonitor renamedViewModel)
        {
            var filepath = renamedViewModel.FilePath;

            //renamedViewModel.FileName = GetFileNameForPath(filepath);
        }

        private static string GetFileNameForPath(string filepath)
        {
            return Path.GetFileName(filepath);
        }

        //[NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            var handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
        }

        //private void AddButton_OnClick(object sender, RoutedEventArgs e)
        //{
        //    PromptForFile();
        //}

        //private void PromptForFile()
        //{
        //    var openFileDialog = new OpenFileDialog { CheckFileExists = false, Multiselect = true };

        //    if (openFileDialog.ShowDialog() == true)
        //    {
        //        try
        //        {
        //            foreach (var fileName in openFileDialog.FileNames)
        //            {
        //                AddFileMonitor(fileName);
        //            }
        //        }
        //        catch (Exception exception) {}
        //        {
        //            MessageBox.Show("Error: " + exception.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        //        }
        //    }
        //}

        private void ClearButton_OnClick(object sender, RoutedEventArgs e)
        {
            //if (SelectedItem != null)
            //{
            //    SelectedItem.Contents = string.Empty;
            //}
        }

        private void FreezeButton_OnClick(object sender, RoutedEventArgs e)
        {
            //if (SelectedItem != null)
            //{
            //    SelectedItem.IsFrozen = !SelectedItem.IsFrozen;
            //}
        }

        private void RemoveButton_OnClick(object sender, RoutedEventArgs e)
        {
            if (SelectedItem != null)
            {
                SelectedItem.Dispose();
                FileMonitors.Remove(SelectedItem);
            }
        }

        private void TextBoxBase_OnTextChanged(object sender, TextChangedEventArgs e)
        {
            var textBox = (RichTextBox)(sender);
            var fileMonitorViewModel = ((FileMonitor)textBox.DataContext);

            if (fileMonitorViewModel != null)
            {
               
                    textBox.ScrollToEnd();
               
            }
        }

        private void RefreshLastUpdatedText()
        {
            if (_lastUpdateDateTime != null)
            {
                var dateTime = _lastUpdateDateTime.Value;
                var datestring = dateTime.Date != DateTime.Now.Date ? " on " + dateTime : " at " + dateTime.ToLongTimeString();
                LastUpdated = _lastUpdatedViewModel.FilePath + datestring;
            }
        }


        public class VisibilityBoolConverter : IValueConverter
    {
        /// <summary>
        /// Converts a value. 
        /// </summary>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        /// <param name="value">The value produced by the binding source.</param><param name="targetType">The type of the binding target property.</param><param name="parameter">The converter parameter to use.</param><param name="culture">The culture to use in the converter.</param>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool && !(bool)value)
            {
                return Visibility.Visible;
            }

            return Visibility.Collapsed;
        }

        /// <summary>
        /// Converts a value. 
        /// </summary>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        /// <param name="value">The value that is produced by the binding target.</param><param name="targetType">The type to convert to.</param><param name="parameter">The converter parameter to use.</param><param name="culture">The culture to use in the converter.</param>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class NullVisibilityConverter : IValueConverter
    {
        /// <summary>
        /// Converts a value. 
        /// </summary>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        /// <param name="value">The value produced by the binding source.</param><param name="targetType">The type of the binding target property.</param><param name="parameter">The converter parameter to use.</param><param name="culture">The culture to use in the converter.</param>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value == null ? Visibility.Hidden : Visibility.Visible;
        }

        /// <summary>
        /// Converts a value. 
        /// </summary>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        /// <param name="value">The value that is produced by the binding target.</param><param name="targetType">The type to convert to.</param><param name="parameter">The converter parameter to use.</param><param name="culture">The culture to use in the converter.</param>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class NullBooleanConverter : IValueConverter
    {
        /// <summary>
        /// Converts a value. 
        /// </summary>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        /// <param name="value">The value produced by the binding source.</param><param name="targetType">The type of the binding target property.</param><param name="parameter">The converter parameter to use.</param><param name="culture">The culture to use in the converter.</param>
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value != null;
        }

        /// <summary>
        /// Converts a value. 
        /// </summary>
        /// <returns>
        /// A converted value. If the method returns null, the valid null value is used.
        /// </returns>
        /// <param name="value">The value that is produced by the binding target.</param><param name="targetType">The type to convert to.</param><param name="parameter">The converter parameter to use.</param><param name="culture">The culture to use in the converter.</param>
        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    /// <summary>
    ///     Provides static helper methods for validating preconditions.
    /// </summary>
    //public static class Preconditions
    //{
    //    public static T CheckNotNull<T>(T value, [InvokerParameterName] string paramName = null)
    //        where T : class
    //    {
    //        if (value == null)
    //        {
    //            throw new ArgumentNullException(paramName ?? "value");
    //        }

    //        return value;
    //    }

    //    public static string CheckNotEmptyOrNull(string value, [InvokerParameterName] string paramName = null)
    //    {
    //        if (string.IsNullOrWhiteSpace(value))
    //        {
    //            throw new ArgumentNullException(paramName ?? "value");
    //        }

    //        return value;
    //    }

    //    public static void CheckArgumentRange([InvokerParameterName] string paramName, long value, long minInclusive, long maxInclusive)
    //    {
    //        if (value < minInclusive || value > maxInclusive)
    //        {
    //            throw new ArgumentOutOfRangeException(paramName, string.Format(Resources.Preconditions_CheckArgumentRange, minInclusive, maxInclusive));
    //        }
    //    }

    //    public static void CheckArgumentRange([InvokerParameterName] string paramName, int value, int minInclusive, int maxInclusive)
    //    {
    //        if (value < minInclusive || value > maxInclusive)
    //        {
    //            throw new ArgumentOutOfRangeException(paramName, string.Format(Resources.Preconditions_CheckArgumentRange, minInclusive, maxInclusive));
    //        }
    //    }

    //    public static void CheckArgument(bool expression, [InvokerParameterName] string parameter, string message)
    //    {
    //        if (!expression)
    //        {
    //            throw new ArgumentException(message, parameter);
    //        }
    //    }

    //    public static void CheckNotDefault<T>(T value, [InvokerParameterName] string paramName = null)
    //    {
    //        if (Equals(value, default(T)))
    //        {
    //            throw new ArgumentException(Resources.Preconditions_CheckNotDefault_DefaultValue, paramName ?? "value");
    //        }
    //    }
    //}

    /// <summary>
    /// Implemented by classes that monitor a file for changes and report them.
    /// </summary>
    public interface IFileMonitor : IDisposable
    {
        /// <summary>
        /// Occurs when the file being monitored is updated.
        /// </summary>
        event Action<IFileMonitor, string> FileUpdated;

        /// <summary>
        /// Occurs when the file being monitored is deleted.
        /// </summary>
        event Action<IFileMonitor> FileDeleted;

        /// <summary>
        /// Occurs when the file being monitored is recreated.
        /// </summary>
        event Action<IFileMonitor> FileCreated;

        /// <summary>
        /// Occurs when the file being monitored is renamed.
        /// </summary>
        event Action<IFileMonitor, string> FileRenamed;

        /// <summary>
        /// Gets the path of the file being monitored.
        /// </summary>
        /// <value>
        /// The file path.
        /// </value>
        string FilePath { get; }

        /// <summary>
        /// Gets or sets the length of the read buffer.
        /// </summary>
        /// <value>
        /// The length of the read buffer.
        /// </value>
        int ReadBufferSize { get; set; }

        /// <summary>
        /// Refreshes the <see cref="IFileMonitor"/> checking for any changes.
        /// </summary>
        /// <returns></returns>
        Task RefreshAsync();

        /// <summary>
        /// Updates the encoding used to read the file.
        /// </summary>
        /// <param name="encoding">The encoding.</param>
        void UpdateEncoding(Encoding encoding);

        /// <summary>
        /// Gets or sets a value indicating whether a buffer is used read the changes in blocks.
        /// </summary>
        /// <value>
        ///   <c>true</c> if a buffer is used; otherwise, <c>false</c>.
        /// </value>
        bool BufferedRead { get; set; }
    }
        public class FileMonitor : IFileMonitor, IDisposable
        {
            private const int DefaultBufferSize = 1048576;
            private readonly object _syncRoot = new object();
            private bool _bufferedRead = true;
            private Encoding _encoding;
            private bool _fileExists;
            protected string _filePath;
            private bool _isDisposed;
            private int _readBufferSize = DefaultBufferSize;
            private Stream _stream;
            private StreamReader _streamReader;

            public FileMonitor(string filePath, Encoding encoding = null)
            {
                encoding = encoding ?? Encoding.UTF8;

                //Preconditions.CheckNotEmptyOrNull(filePath, "filePath");

                _filePath = filePath;
                _encoding = encoding;

                // Track file existence for the delete/replace events
                _fileExists = GetFileExists();

                if (_fileExists)
                {
                    OpenFile(filePath);
                }
            }

            #region IFileMonitor Members

            /// <summary>
            ///     Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
            /// </summary>
            public void Dispose()
            {
                Dispose(true);
                GC.SuppressFinalize(this);
            }

            /// <summary>
            ///     Occurs when the file being monitored is updated.
            /// </summary>
            public event Action<IFileMonitor, string> FileUpdated;

            /// <summary>
            ///     Occurs when the file being monitored is deleted.
            /// </summary>
            public event Action<IFileMonitor> FileDeleted;

            /// <summary>
            ///     Occurs when the file being monitored is recreated.
            /// </summary>
            public event Action<IFileMonitor> FileCreated;

            /// <summary>
            ///     Occurs when the file being monitored is renamed.
            /// </summary>
            public event Action<IFileMonitor, string> FileRenamed;

            /// <summary>
            ///     Gets the path of the file being monitored.
            /// </summary>
            /// <value>
            ///     The file path.
            /// </value>
            public string FilePath
            {
                get { return _filePath; }
            }

            /// <summary>
            ///     Gets or sets the length of the read buffer.
            /// </summary>
            /// <value>
            ///     The length of the read buffer.
            /// </value>
            public int ReadBufferSize
            {
                get { return _readBufferSize; }
                set
                {
                    // Buffer cannot be 0 or negative
                    //Preconditions.CheckArgumentRange("value", value, 1, int.MaxValue);
                    _readBufferSize = value;
                }
            }

            /// <summary>
            ///     Refreshes the <see cref="IFileMonitor" /> checking for any changes.
            /// </summary>
            /// <returns></returns>
            public Task RefreshAsync()
            {
                return Task.Run(() => CheckForChanges());
            }

            /// <summary>
            ///     Updates the encoding used to read the file.
            /// </summary>
            /// <param name="encoding">The encoding.</param>
            public void UpdateEncoding(Encoding encoding)
            {
                lock (_syncRoot)
                {
                    _encoding = encoding;
                    OpenFile(_filePath);
                }
            }

            /// <summary>
            ///     Gets or sets a value indicating whether a buffer is used read the changes in blocks.
            /// </summary>
            /// <value>
            ///     <c>true</c> if a buffer is used; otherwise, <c>false</c>.
            /// </value>
            public bool BufferedRead
            {
                get { return _bufferedRead; }
                set { _bufferedRead = value; }
            }

            #endregion

            private void OpenFile(string filePath)
            {
                // Dispose existing stream
                DisposeStream();

                // File is opened for read only, and shared for read, write and delete
                _stream = File.Open(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite | FileShare.Delete);
                _streamReader = new StreamReader(_stream, _encoding);

                // Start at the end of the file
                _streamReader.BaseStream.Seek(0, SeekOrigin.End);
            }

            protected virtual void OnFileRenamed(string newName)
            {
                var handler = FileRenamed;
                if (handler != null)
                {
                    handler(this, newName);
                }
            }

            protected virtual void OnFileUpdated(string updatedContent)
            {
                var handler = FileUpdated;

                if (handler != null)
                {
                    handler(this, updatedContent);
                }
            }

            protected virtual void OnFileDeleted()
            {
                var handler = FileDeleted;

                if (handler != null)
                {
                    handler(this);
                }
            }

            protected virtual void OnFileCreated()
            {
                var handler = FileCreated;

                if (handler != null)
                {
                    handler(this);
                }
            }

            private bool GetFileExists()
            {
                return File.Exists(_filePath);
            }

            /// <summary>
            ///     Checks for changes to the file.
            /// </summary>
            protected virtual void CheckForChanges()
            {
                lock (_syncRoot)
                {
                    if (_fileExists && !GetFileExists())
                    {
                        // File has been deleted
                        _fileExists = false;
                        OnFileDeleted();
                    }
                    else if (!_fileExists && GetFileExists())
                    {
                        // File has been created
                        OpenFile(_filePath);
                        _fileExists = true;
                        OnFileCreated();
                    }

                    if (_streamReader == null)
                    {
                        // File is not open
                        return;
                    }

                    var baseStream = _streamReader.BaseStream;

                    if (baseStream.Position > baseStream.Length)
                    {
                        // File is smaller than the current position
                        // Seek to the end
                        baseStream.Seek(0, SeekOrigin.End);
                    }

                    if (_streamReader.EndOfStream)
                        return;

                    if (BufferedRead)
                    {
                        var buffer = new char[ReadBufferSize];
                        int charCount;

                        while ((charCount = _streamReader.Read(buffer, 0, ReadBufferSize)) > 0)
                        {
                            var appendedContent = new string(buffer, 0, charCount);

                            if (!string.IsNullOrEmpty(appendedContent))
                            {
                                OnFileUpdated(appendedContent);
                                RichTextBoxExtensions.MigLog(appendedContent);
                            }
                        }
                    }
                    else
                    {
                        var appendedContent = _streamReader.ReadToEnd();

                        if (!string.IsNullOrEmpty(appendedContent))
                        {
                            OnFileUpdated(appendedContent);
                            RichTextBoxExtensions.MigLog(appendedContent);
                        }
                    }
                }
            }


            /// <summary>
            ///     Releases unmanaged and - optionally - managed resources.
            /// </summary>
            /// <param name="disposing">
            ///     <c>true</c> to release both managed and unmanaged resources; <c>false</c> to release only
            ///     unmanaged resources.
            /// </param>
            protected virtual void Dispose(bool disposing)
            {
                if (_isDisposed)
                {
                    return;
                }

                if (disposing)
                {
                    DisposeStream();
                }

                _isDisposed = true;
            }

            private void DisposeStream()
            {
                if (_streamReader != null)
                {
                    _streamReader.Dispose();
                }

                if (_stream != null)
                {
                    _stream.Dispose();
                }
            }

            /// <summary>
            ///     Finalizes an instance of the <see cref="FileMonitor" /> class.
            /// </summary>
            ~FileMonitor()
            {
                Dispose(false);
            }



        }
        }
    }













































    //    public class MonitorLogFileLineEventArgs : EventArgs
    //{
    //    public string Line { get; set; }
    //}

    //public class MonitorLogFile
    //{
    //    public EventHandler<MonitorLogFileLineEventArgs> OnLine;

    //    // file path + delimiter internals
    //    string _path = String.Empty;
    //    string _delimiter = String.Empty;

    //    // timer object
    //    Timer _checktimer = null;

    //    // buffer for storing data at the end of the file that does not yet have a delimiter
    //    string _buffer = String.Empty;

    //    // get the current size
    //    long _currentSize = 0;

    //    // are we currently checking the log (stops the timer going in more than once)
    //    bool _isCheckingLog = false;

    //    protected bool StartCheckingLog()
    //    {
    //        lock (_checktimer)
    //        {
    //            if (_isCheckingLog)
    //                return true;

    //            _isCheckingLog = true;
    //            return false;
    //        }
    //    }

    //    protected void DoneCheckingLog()
    //    {
    //        lock (_checktimer)
    //            _isCheckingLog = false;
    //    }

    //    public MonitorLogFile(string path, string delimiter = "\n")
    //    {
    //        _path = path;
    //        _delimiter = delimiter;
    //    }

    //    public void Start()
    //    {
    //        // get the current size
    //        _currentSize = new FileInfo(_path).Length;

    //        // start the timer
    //        _checktimer = new Timer();
    //        _checktimer.Elapsed += new ElapsedEventHandler(CheckLog);
    //        _checktimer.AutoReset = false;
    //        _checktimer.Start();
    //    }

    //    private void CheckLog(object s, ElapsedEventArgs e)
    //    {
    //        if (StartCheckingLog())
    //        {
    //            try
    //            {
    //                // get the new size
    //                var newSize = new FileInfo(_path).Length;

    //                // if they are the same then continue.. if the current size is bigger than the new size continue
    //                if (_currentSize >= newSize)
    //                    return;

    //                // read the contents of the file
    //                using (var stream = File.Open(_path, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
    //                using (StreamReader sr = new StreamReader(stream))
    //                {
    //                    // seek to the current file position
    //                    sr.BaseStream.Seek(_currentSize, SeekOrigin.Begin);

    //                    // read from current position to the end of the file
    //                    var newData = _buffer + sr.ReadToEnd();

    //                    // if we don't end with a delimiter we need to store some data in the buffer for next time
    //                    if (!newData.EndsWith(_delimiter))
    //                    {
    //                        // we don't have any lines to process so save in the buffer for next time
    //                        if (newData.IndexOf(_delimiter) == -1)
    //                        {
    //                            _buffer += newData;
    //                            newData = String.Empty;
    //                        }
    //                        else
    //                        {
    //                            // we have at least one line so store the last section (without lines) in the buffer
    //                            var pos = newData.LastIndexOf(_delimiter) + _delimiter.Length;
    //                            _buffer = newData.Substring(pos);
    //                            newData = newData.Substring(0, pos);
    //                        }
    //                    }

    //                    // split the data into lines
    //                    var lines = newData.Split(new string[] { _delimiter }, StringSplitOptions.RemoveEmptyEntries);

    //                    // send back to caller, NOTE: this is done from a different thread!
    //                    foreach (var line in lines)
    //                    {
    //                        if (OnLine != null)
    //                            OnLine(this, new MonitorLogFileLineEventArgs { Line = line });
    //                    }
    //                }

    //                // set the new current position
    //                _currentSize = newSize;
    //            }
    //            catch (Exception ex) { System.Windows.Forms.MessageBox.Show(ex.ToString()); }

    //            // we done..
    //            DoneCheckingLog();
    //        }
    //    }

    //    public void Stop()
    //    {
    //        if (_checktimer == null)
    //            return;

    //        _checktimer.Stop();
    //        _isCheckingLog = false;
    //    }
    //}
