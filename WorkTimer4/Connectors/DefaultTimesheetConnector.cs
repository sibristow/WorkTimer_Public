using System;
using System.Collections.Generic;
using System.Composition;
using System.IO;
using System.Text.Json;
using WorkTimer4.API.Connectors;
using WorkTimer4.API.Data;

namespace WorkTimer4.Connectors
{
    /// <summary>
    /// A default <see cref="ITimesheetConnector"/> implementation which writes timesheet data to JSON
    /// </summary>
    [Export(typeof(ITimesheetConnector))]
    internal sealed class DefaultTimesheetConnector : JsonTimesheetConnector
    {
        public override string Name { get { return "Default JSON Connector"; } }


        public DefaultTimesheetConnector()
        {
            this.DataFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "WorkTimer", "timesheet.json");
        }


        public override void RecordActivity(Activity activity)
        {
            var tsActivity = new TimesheetActivity(activity);

            // read the existing activities
            var activityList = this.ReadFile();

            // insert the activity in chronological order
            this.InsertTimesheetActivity(activityList, tsActivity);

            // save the updated list
            var text = JsonSerializer.Serialize(activityList, API.Json.JsonSerialisation.SerialiserOptions);
            File.WriteAllText(this.DataFile, text);
        }

        private List<TimesheetActivity> ReadFile()
        {
            if (!File.Exists(this.DataFile))
            {
                return new List<TimesheetActivity>();
            }

            var text = File.ReadAllText(this.DataFile);
            return JsonSerializer.Deserialize<List<TimesheetActivity>>(text, API.Json.JsonSerialisation.SerialiserOptions);
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
