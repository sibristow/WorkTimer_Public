using System;
using System.ComponentModel;
using System.IO;
using WorkTimer4.API.Data;

namespace WorkTimer4.API.Connectors
{
    public abstract class JsonTimesheetConnector : JsonProviderBase, ITimesheetConnector
    {
        private string? dataFile;
        private RollingInterval rollingInterval;
        protected string? currentRollingFile;
        protected DateTimeOffset? currentTimestamp;
        protected DateTimeOffset? nextTimestamp;

        public abstract string Name { get; }

        [Category("Timesheets")]
        [DisplayName("Data File")]
        [Description("File where timesheet data is read from and saved to")]
        public string? DataFile
        {
            get
            {
                return dataFile;
            }
            set
            {
                dataFile = value;
                this.UpdateRollingFile();
            }
        }


        [Category("Timesheets")]
        [DisplayName("Rolling Interval")]
        [Description("Specifies the frequency at which the timesheet data file should roll.")]
        public RollingInterval RollingInterval
        {
            get
            {
                return rollingInterval;
            }
            set
            {
                rollingInterval = value;
                this.UpdateRollingFile();
            }
        }


        public JsonTimesheetConnector()
        {
            this.RollingInterval = RollingInterval.Never;
        }


        public override bool Equals(object? obj)
        {
            return obj is ITimesheetConnector pc && object.Equals(this.Name, pc.Name);
        }

        public override int GetHashCode()
        {
            return this.Name.GetHashCode();
        }

        public abstract void ViewTimesheet(Activity? currentActivity);

        /// <summary>
        /// Called when the activity is ready to be recorded
        /// </summary>
        /// <param name="activity"></param>
        protected abstract void OnRecordingActivity(Activity activity, string rollingFile);


        ///<inheritdoc/>
        public void RecordActivity(Activity activity)
        {
            this.RollFile(activity);

            if (this.currentRollingFile is null)
            {
                throw new ConnectorException("Timesheet connector output file is not specified");
            }

            this.OnRecordingActivity(activity, this.currentRollingFile);
        }


        protected void UpdateRollingFile()
        {
            if (string.IsNullOrWhiteSpace(this.DataFile))
                return;

            if (this.RollingInterval == RollingInterval.Never)
            {
                this.currentRollingFile = this.DataFile;
            }

            // get the directory
            this.currentTimestamp = this.RollingInterval.GetCurrentTimestamp(DateTime.Now);
            this.nextTimestamp = this.RollingInterval.GetNextTimestamp(DateTime.Now);
            this.currentRollingFile = this.GetFilename(currentTimestamp);
        }

        /// <summary>
        /// Checks and rolls the data file if necessary
        /// </summary>
        /// <param name="activity"></param>
        /// <returns>the rolled data file name</returns>
        private void RollFile(Activity activity)
        {
            if (this.nextTimestamp is null || activity.Start < this.nextTimestamp)
            {
                // never roll, or not reached next timestamp point
                // so leave with current rolling file
                return;
            }

            // reached next timestamp point
            // so data file should be rolled
            this.currentTimestamp = this.nextTimestamp;
            this.currentRollingFile = this.GetFilename(this.currentTimestamp);

            // increment the next timestamp point according to the current rolling interval
            this.nextTimestamp = this.RollingInterval.GetNextTimestamp(activity.Start);
        }

        /// <summary>
        /// Get the next rolled formatted filename
        /// </summary>
        /// <param name="timestamp"></param>
        /// <returns></returns>
        private string? GetFilename(DateTimeOffset? timestamp)
        {
            // never roll, or no timestamp provided, so just use the data file
            if (timestamp is null || this.RollingInterval == RollingInterval.Never)
                return this.DataFile;

            // get the date format for the rolling interval
            var dateFormat = this.RollingInterval.GetDateFormat();

            // generate a string from the timestamp to add to the data filename
            var filetimestamp = timestamp.Value.ToString(dateFormat);

            var directory = Path.GetDirectoryName(this.DataFile) ?? string.Empty;
            var filenamePrefix = Path.GetFileNameWithoutExtension(this.DataFile);
            var extension = Path.GetExtension(this.DataFile) ?? ".json";

            // should return e.g. "c:\folder\file_20220809.json"
            return Path.Combine(directory, filenamePrefix + "_" + filetimestamp + extension);
        }
    }
}
