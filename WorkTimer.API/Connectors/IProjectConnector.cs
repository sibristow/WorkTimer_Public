using System;
using System.Collections.Generic;

namespace WorkTimer4.API.Connectors
{
    public interface IProjectConnector
    {
        /// <summary>
        /// Event raised when the IProjectConnector instance requests the app to reload the project
        /// </summary>
        /// <remarks>
        /// Raised e.g. if the project json file is changed externally
        /// </remarks>
        event EventHandler<EventArgs> ProjectReloadRequest;

        /// <summary>
        /// Gets the name of the connector
        /// </summary>
        string Name { get; }

        /// <summary>
        /// Gets the list of project groups from the connector's source
        /// </summary>
        /// <returns></returns>
        IEnumerable<Data.ProjectGroup> GetProjects();

        /// <summary>
        /// Writes the list of projects back to the connector's source
        /// </summary>
        /// <param name="projects"></param>
        void WriteProjects(IEnumerable<Data.ProjectGroup> projectGroups);       
    }
}
