using System.Collections.Generic;

namespace WorkTimer4
{
    internal class ApplicationConfigJson
    {
        public string? ProjectConnector { get; set; }

        public Dictionary<string, object?> ProjectConnectorOptions { get; set; }

        public string? TimesheetConnector { get; set; }

        public Dictionary<string, object?> TimesheetConnectorOptions { get; set; }

        /// <summary>
        /// Gets or sets a value indicating whether to notify the user of the active project when the Windows session is unlocked
        /// </summary>
        public bool NotifyOnUnlock { get; set; }


        public ApplicationConfigJson()
        {
            this.ProjectConnectorOptions = new Dictionary<string, object?>();
            this.TimesheetConnectorOptions = new Dictionary<string, object?>();
            this.NotifyOnUnlock = true;
        }
    }
}
