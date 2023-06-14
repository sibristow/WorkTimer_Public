using System;
using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using WorkTimer4.Connectors;

namespace WorkTimer4.TimesheetView
{
    internal class TimeLineDay : ObservableObject
    {
        private DateOnly date = DateOnly.FromDateTime(DateTime.Today);
        private ObservableCollection<TimesheetActivity> activities = new ObservableCollection<TimesheetActivity>();

        public DateOnly Date
        {
            get
            {
                return this.date;

            }
            set
            {
                this.date = value;
                this.OnPropertyChanged(nameof(this.Date));
            }
        }

        public ObservableCollection<TimesheetActivity> Activities
        {
            get
            {
                return activities;
            }
            set
            {
                this.activities = value;
                this.OnPropertyChanged(nameof(this.Activities));
            }
        }

        public TimeLineDay(DateOnly date)
        {
            this.Date = date;
            this.Activities = new ObservableCollection<TimesheetActivity>();
        }

        public override bool Equals(object? obj)
        {
            return obj is TimeLineDay tld && this.Date.Equals(tld.Date);
        }

        public override int GetHashCode()
        {
            return this.Date.GetHashCode();
        }
    }
}
