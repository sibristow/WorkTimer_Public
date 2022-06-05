using System.Collections.Generic;

namespace WorkTimer4
{
    internal class ApplicationConfigJson
    {
        public string ProjectConnector { get; set; }

        public Dictionary<string, object?> ProjectConnectorOptions { get; set; }

        public string TimesheetConnector { get; set; }

        public Dictionary<string, object?> TimesheetConnectorOptions { get; set; }

        public ApplicationConfigJson()
        {
            this.ProjectConnectorOptions = new Dictionary<string, object?>();
            this.TimesheetConnectorOptions = new Dictionary<string, object?>();
        }
    }
}
