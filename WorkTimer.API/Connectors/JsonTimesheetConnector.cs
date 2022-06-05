using System.ComponentModel;
using WorkTimer4.API.Data;

namespace WorkTimer4.API.Connectors
{
    public abstract class JsonTimesheetConnector : JsonProviderBase, ITimesheetConnector
    {
        public abstract string Name { get; }

        [Category("Timesheets")]
        [DisplayName("Data File")]
        [Description("File where timesheet data is read from and saved to")]
        public string DataFile { get; set; }


        public override bool Equals(object? obj)
        {
            return obj is ITimesheetConnector pc && object.Equals(this.Name, pc.Name);
        }

        public override int GetHashCode()
        {
            return this.Name.GetHashCode();
        }

        public abstract void RecordActivity(Activity activity);       
    }
}
