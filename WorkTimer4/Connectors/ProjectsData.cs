using System.Collections.Generic;
using WorkTimer4.API.Data;

namespace WorkTimer4.Connectors
{
    /// <summary>
    /// Project data serialised by the <see cref="DefaultProjectConnector"/>
    /// </summary>
    internal class ProjectsData
    {
        public IEnumerable<Project> Projects { get; set; }

        public ProjectsData()
        {
            this.Projects = new List<Project>();
        }
    }
}
