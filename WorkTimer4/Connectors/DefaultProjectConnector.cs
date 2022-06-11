using System;
using System.Collections.Generic;
using System.Composition;
using System.IO;
using WorkTimer4.API.Connectors;
using WorkTimer4.API.Data;

namespace WorkTimer4.Connectors
{
    /// <summary>
    /// A default <see cref="IProjectConnector"/> implementation which reads and writes Project data to JSON
    /// </summary>
    [Export(typeof(IProjectConnector))]
    internal sealed class DefaultProjectConnector : JsonProjectConnector<ProjectsData>
    {
        public override string Name { get { return "Default JSON Connector"; } }


        public DefaultProjectConnector()
        {            
            this.DataFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "WorkTimer", "projects.json");
            this.ReloadOnChanges = true;
        }

        protected override IEnumerable<Project> FromDeserialised(ProjectsData deserialised)
        {
            return deserialised.Projects;
        }

        protected override ProjectsData ForSerialisation(IEnumerable<Project> projects)
        {
            return new ProjectsData() { Projects = projects };
        }
    }
}
