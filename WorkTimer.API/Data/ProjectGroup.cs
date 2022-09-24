using System.Collections.Generic;

namespace WorkTimer4.API.Data
{
    /// <summary>
    /// Represents a group of <see cref="Project"/> items
    /// </summary>
    public class ProjectGroup
    {
        /// <summary>
        /// Gets or sets the group name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Gets or sets the projects in the group
        /// </summary>
        public IList<Project> Projects { get; set; }

        public ProjectGroup()
        {
            this.Name = "Group";
            this.Projects = new List<Project>();
        }
    }
}
