using System;
using System.Collections.Generic;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using WorkTimer4.API.Data;
using WorkTimer4.Connectors;

namespace WorkTimer4.TimesheetView
{
    internal class TimesheetViewModel : ObservableObject
    {
        private DateTime from;
        private DateTime to;
        private DateTimeOffset fromFilter;
        private DateTimeOffset toFilter;
        private IEnumerable<TimesheetActivity> recorded;
        private double reportingFraction;
        private double width;
        private double height;


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


        public TimesheetViewModel(List<TimesheetActivity> recorded, WindowSize windowSize)
        {
            // use the properties instead of the fields to ensure the filters are correctly set as well for the beginning and end of the days
            this.To = DateTime.Today;
            this.From = this.To.GetPreviousMonday();

            this.recorded = recorded ?? Enumerable.Empty<TimesheetActivity>();
            this.reportingFraction = DateAggregation.DEFAULT_REPORTING;
            this.height = windowSize.Height;
            this.width = windowSize.Width;
        }
    }
}
