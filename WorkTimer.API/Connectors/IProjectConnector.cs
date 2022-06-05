using System.Collections.Generic;

namespace WorkTimer4.API.Connectors
{
    public interface IProjectConnector
    {
        /// <summary>
        /// Gets the name of the connector
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the list of projects from the connector's source
        /// </summary>
        /// <returns></returns>
        IEnumerable<Data.Project> GetProjects();

        /// <summary>
        /// Writes the list of projects back to the connector's source
        /// </summary>
        /// <param name="projects"></param>
        void WriteProjects(IEnumerable<Data.Project> projects);       
    }
}
