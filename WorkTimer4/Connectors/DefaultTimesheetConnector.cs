using System;
using System.Collections.Generic;
using System.Composition;
using System.IO;
using System.Text.Json;
using WorkTimer4.API.Connectors;
using WorkTimer4.API.Data;
using WorkTimer4.TimesheetView;

namespace WorkTimer4.Connectors
{
    /// <summary>
    /// A default <see cref="ITimesheetConnector"/> implementation which writes timesheet data to JSON
    /// </summary>
    [Export(typeof(ITimesheetConnector))]
    internal sealed class DefaultTimesheetConnector : JsonTimesheetConnector
    {
        public override string Name { get { return "Default JSON Connector"; } }

        /// <summary>
        /// Gets or sets the fraction of an hour which the total aggregated hours should be rounded to, for reporting
        /// </summary>
        public double ReportingFraction { get; set; }


        public DefaultTimesheetConnector()
        {
            this.DataFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "WorkTimer", "timesheet.json");
            this.ReportingFraction = DateAggregation.DEFAULT_REPORTING;
        }

        public override void ViewTimesheet(Activity? currentActivity)
        {
            // read recorded activities
            var recorded = this.ReadFile(this.currentRollingFile);

            // include the current activity (if there is one)
            if (currentActivity != null)
            {
                currentActivity.End = DateTimeOffset.UtcNow;

                recorded.Add(new TimesheetActivity(currentActivity));
            }

            var vm = new TimesheetViewModel(recorded) {  ReportingFraction = this.ReportingFraction };
            var window = new TimesheetViewer() { DataContext = vm };
            window.ShowDialog();
        }

        protected override void OnRecordingActivity(Activity activity, string rolledFile)
        {
            var tsActivity = new TimesheetActivity(activity);

            // read the existing activities
            var activityList = this.ReadFile(rolledFile);

            // insert the activity in chronological order
            this.InsertTimesheetActivity(activityList, tsActivity);

            // save the updated list
            var text = JsonSerializer.Serialize(activityList, API.Json.JsonSerialisation.SerialiserOptions);
            File.WriteAllText(rolledFile, text);
        }


        private List<TimesheetActivity> ReadFile(string? file)
        {
            if (file == null || !File.Exists(file))
            {
                return new List<TimesheetActivity>();
            }

            var text = File.ReadAllText(file);
            return JsonSerializer.Deserialize<List<TimesheetActivity>>(text, API.Json.JsonSerialisation.SerialiserOptions) ?? new List<TimesheetActivity>();
        }

        private void InsertTimesheetActivity(List<TimesheetActivity> activityList, TimesheetActivity timesheetActivity)
        {
            for (var i = 0; i < activityList.Count; i++)
            {
                var l = activityList[i];

                if (l.Start > timesheetActivity.Start)
                {
                    activityList.Insert(i, timesheetActivity);
                    return;
                }
            }

            activityList.Add(timesheetActivity);
        }
    }
}
