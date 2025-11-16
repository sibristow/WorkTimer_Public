using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Input;
using AdonisUI.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Win32;
using WorkTimer4.API.Data;
using WorkTimer4.Connectors;

namespace WorkTimer4.TimesheetView
{
    internal class TimesheetViewModel : ObservableObject
    {
        private const string DEFAULT_TITLE = "Timesheet Viewer";

        private DateTime from;
        private DateTime to;
        private DateTimeOffset fromFilter;
        private DateTimeOffset toFilter;
        private IEnumerable<TimesheetActivity> recorded;
        private readonly string? dataFile;
        private readonly List<TimesheetActivity>? current;
        private double reportingFraction;
        private double width;
        private double height;
        private string title;

        /// <summary>
        /// Gets or sets the window title
        /// </summary>
        public string Title
        {
            get
            {
                return title;
            }
            set
            {
                this.SetProperty(ref this.title, value);
            }
        }

        /// <summary>
        /// Gets or sets the date to filter the timesheet from
        /// </summary>
        public DateTime From
        {
            get
            {
                return this.from;
            }
            set
            {
                if (value > this.to)
                    value = this.to;

                // set the From date and the UTC offset date used for filtering
                this.from = value;
                this.FromFilter = new DateTimeOffset(this.from.Year, this.from.Month, this.from.Day, 0, 0, 0, TimeSpan.FromHours(0));

                this.OnPropertyChanged(nameof(this.From));
            }
        }

        /// <summary>
        /// Gets or sets the date to filter the timesheet to
        /// </summary>
        public DateTime To
        {
            get
            {
                return this.to;
            }
            set
            {
                if (value < this.from)
                    value = this.from;

                // set the To date and the UTC offset date used for filtering
                this.to = value;
                this.ToFilter = new DateTimeOffset(this.to.Year, this.to.Month, this.to.Day, 23, 59, 59, 999, TimeSpan.FromHours(0));

                this.OnPropertyChanged(nameof(this.To));
            }
        }

        /// <summary>
        /// Gets the date and time to use for filtering activities from
        /// </summary>
        /// <remarks>
        /// This is the start of the day on <see cref="From"/> date
        /// </remarks>
        public DateTimeOffset FromFilter
        {
            get
            {
                return fromFilter;
            }

            private set
            {
                this.SetProperty(ref this.fromFilter, value);
            }
        }

        /// <summary>
        /// Gets the date and time to use for filtering activities to
        /// </summary>
        /// <remarks>
        /// This is the end of the day on <see cref="To"/> date
        /// </remarks>
        public DateTimeOffset ToFilter
        {
            get
            {
                return toFilter;
            }

            private set
            {
                this.SetProperty(ref this.toFilter, value);
            }
        }

        /// <summary>
        /// Gets or sets all the recorded activities
        /// </summary>
        public IEnumerable<TimesheetActivity> Recorded
        {
            get
            {
                return recorded;
            }

            set
            {
                this.SetProperty(ref this.recorded, value);
            }
        }

        /// <summary>
        /// Gets or sets the fraction of an hour which the total aggregated hours should be rounded to, for reporting
        /// </summary>
        public double ReportingFraction
        {
            get
            {
                return reportingFraction;
            }
            set
            {
                this.SetProperty(ref this.reportingFraction, value);
            }
        }

        public double Width
        {
            get
            {
                return width;
            }
            set
            {
                this.SetProperty(ref width, value);
            }
        }

        public double Height
        {
            get
            {
                return height;
            }
            set
            {
                this.SetProperty(ref height, value);
            }
        }

        public ICommand OpenFileCommand { get; }

        public ICommand RevertToCurrentCommand { get; }


        public TimesheetViewModel(List<TimesheetActivity> recorded, WindowSize windowSize, string? dataFile)
        {
            // use the properties instead of the fields to ensure the filters are correctly set as well for the beginning and end of the days
            this.To = DateTime.Today;
            this.From = this.To.GetPreviousMonday();

            this.recorded = recorded ?? Enumerable.Empty<TimesheetActivity>();
            this.dataFile = dataFile;
            this.current = recorded;
            this.reportingFraction = DateAggregation.DEFAULT_REPORTING;
            this.height = windowSize.Height;
            this.width = windowSize.Width;
            this.title = DEFAULT_TITLE;

            this.OpenFileCommand = new Command(this.OnOpenFile);
            this.RevertToCurrentCommand = new Command(this.OnRevertToCurrent);
        }


        private void OnOpenFile(object? obj)
        {
            var window = obj as AdonisWindow;

            var ofd = new OpenFileDialog()
            {
                CheckFileExists = true,
                Title = "Select Timesheet file",
                Filter = "Timesheet Files|*.json|All Files|*.*"
            };

            if (this.dataFile is not null)
            {
                ofd.DefaultExt = System.IO.Path.GetExtension(this.dataFile);
                ofd.DefaultDirectory = System.IO.Path.GetDirectoryName(this.dataFile);
                ofd.InitialDirectory = ofd.DefaultDirectory;
                ofd.FileName = this.dataFile;
            }

            var result = ofd.ShowDialog(window);

            if (result is null || !result.Value)
            {
                return;
            }

            try
            {
                var read = DefaultTimesheetConnector.ReadFile(ofd.FileName);
                this.Recorded = read;
                this.From = this.Recorded.Min(r => r.Start).Date;
                this.To = this.Recorded.Max(r => r.End).Date;
                this.Title = string.Format("{0} - {1}", DEFAULT_TITLE, System.IO.Path.GetFileName(ofd.FileName));
            }
            catch
            {
                MessageBox.Show(window, "Could not open timesheet file.", "Open Timesheet", MessageBoxButton.OK, MessageBoxImage.Error);
                this.Title = DEFAULT_TITLE;
            }
        }

        private void OnRevertToCurrent(object? obj)
        {
            this.Recorded = this.current ?? Enumerable.Empty<TimesheetActivity>();
            this.To = DateTime.Today;
            this.From = this.To.GetPreviousMonday();
            this.Title = DEFAULT_TITLE;
        }
    }
}
