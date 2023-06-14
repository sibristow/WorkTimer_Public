using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using WorkTimer4.Connectors;

namespace WorkTimer4.TimesheetView
{
    /// <summary>
    /// Interaction logic for TimeLine.xaml
    /// </summary>
    public partial class TimeLine : UserControl
    {
        public static readonly DependencyProperty TimeLineActivitiesProperty = DependencyProperty.Register(nameof(TimeLineActivities), typeof(IEnumerable<TimesheetActivity>), typeof(TimeLine), new PropertyMetadata(null, OnActivities_Changed));

        public static readonly DependencyProperty FromFilterProperty = DependencyProperty.Register("FromFilter", typeof(DateTimeOffset), typeof(TimeLine), new PropertyMetadata(default(DateTimeOffset), OnFilter_Changed));

        public static readonly DependencyProperty ToFilterProperty = DependencyProperty.Register("ToFilter", typeof(DateTimeOffset), typeof(TimeLine), new PropertyMetadata(default(DateTimeOffset), OnFilter_Changed));

        internal static readonly DependencyPropertyKey TimeLineDaysPropertyKey = DependencyProperty.RegisterReadOnly(nameof(TimeLineDays), typeof(ObservableCollection<TimeLineDay>), typeof(TimeLine), new PropertyMetadata(null));


        public DateTimeOffset FromFilter
        {
            get { return (DateTimeOffset)GetValue(FromFilterProperty); }
            set { SetValue(FromFilterProperty, value); }
        }

        public DateTimeOffset ToFilter
        {
            get { return (DateTimeOffset)GetValue(ToFilterProperty); }
            set { SetValue(ToFilterProperty, value); }
        }

        /// <summary>
        /// Gets or sets all the timesheet activities to display on the timeline
        /// </summary>
        internal IEnumerable<TimesheetActivity> TimeLineActivities
        {
            get { return (IEnumerable<TimesheetActivity>)GetValue(TimeLineActivitiesProperty); }
            set { SetValue(TimeLineActivitiesProperty, value); }
        }

        /// <summary>
        /// Gets the collection of timeline day rows containing the relevant timesheet activities
        /// </summary>
        internal ObservableCollection<TimeLineDay> TimeLineDays
        {
            get { return (ObservableCollection<TimeLineDay>)GetValue(TimeLineDaysPropertyKey.DependencyProperty); }
            private set { SetValue(TimeLineDaysPropertyKey, value); }
        }


        public TimeLine()
        {
            InitializeComponent();
            this.TimeLineDays = new ObservableCollection<TimeLineDay>();
        }


        private static void OnActivities_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not TimeLine timeline)
                return;

            if (e.OldValue is not null)
            {
                timeline.TimeLineDays.Clear();

                if (e.OldValue is INotifyCollectionChanged old_ncc)
                {
                    old_ncc.CollectionChanged -= timeline.ActivitiesChanged;
                }
            }

            if (e.NewValue is not null)
            {
                timeline.UpdateTimeLineDays();

                if (e.NewValue is INotifyCollectionChanged new_ncc)
                {
                    new_ncc.CollectionChanged += timeline.ActivitiesChanged;
                }
            }
        }

        private static void OnFilter_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is not TimeLine timeline)
                return;

            timeline.TimeLineDays.Clear();
            timeline.UpdateTimeLineDays();
        }


        private void ActivitiesChanged(object? sender, NotifyCollectionChangedEventArgs? e)
        {
            if (e.Action == NotifyCollectionChangedAction.Reset)
            {
                this.TimeLineDays.Clear();
                return;
            }

            if (e.Action != NotifyCollectionChangedAction.Add || e.NewItems is null)
            {
                return;
            }

            this.UpdateTimeLineDays();
        }

        private void UpdateTimeLineDays()
        {
            foreach (TimesheetActivity a in this.TimeLineActivities)
            {
                if (!this.IsInFilter(a.Start))
                    continue;


                var aDate = DateOnly.FromDateTime(a.Start.Date);
                var tld = this.TimeLineDays.FirstOrDefault(t => t.Date == aDate);

                if (tld is null)
                {
                    tld = new TimeLineDay(aDate);
                    this.TimeLineDays.Add(tld);
                }

                tld.Activities.Add(a);
            }
        }

        /// <summary>
        /// Returns a value indicating whether the specified date time falls within the filtered date range
        /// </summary>
        /// <param name="dateTimeOffset"></param>
        /// <returns></returns>
        private bool IsInFilter(DateTimeOffset dateTimeOffset)
        {
            return dateTimeOffset >= this.FromFilter && dateTimeOffset <= this.ToFilter;
        }
    }
}
