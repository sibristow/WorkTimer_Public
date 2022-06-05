namespace WorkTimer4.API.Connectors
{
    public interface ITimesheetConnector
    {
        /// <summary>
        /// Gets the name of the connector
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Records a completed activity
        /// </summary>
        /// <param name="activity"></param>
        void RecordActivity(Data.Activity activity);
    }
}
