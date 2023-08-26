using System;
using System.Collections.Generic;
using System.Composition;
using System.IO;
using System.Linq;
using System.Text.Json;
using WorkTimer4.API.Connectors;
using WorkTimer4.API.Data;
using WorkTimer4.API.Json;

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
            this.dataFile = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "WorkTimer", "projects.json");
            this.ReloadOnChanges = true;
        }


        protected override ProjectsData Serialise(IEnumerable<ProjectGroup> projectGroups)
        {
            return new ProjectsData() { ProjectGroups = projectGroups };
        }

        /// <inheritdoc/>
        protected override IEnumerable<ProjectGroup> Deserialise(string json)
        {
            var asGroups = this.TryDeserialiseGroups(json, out var projectList);
            var asLegacy = this.TryDeserialiseLegacy(json, out var legacyList);

            if (!asGroups && !asLegacy)
            {
                throw new JsonException("Cannot read json");
            }


            // if one is null return the other
            if (projectList is null && legacyList is not null)
                return legacyList;

            if (projectList is not null && legacyList is null)
                return projectList;


#pragma warning disable CS8604 // Possible null reference argument.
            if (!projectList.Any() && legacyList.Any())
                return legacyList;
#pragma warning restore CS8604 // Possible null reference argument.

            return projectList;
        }

        /// <summary>
        /// Try deserialising text already grouped
        /// </summary>
        /// <param name="text"></param>
        /// <param name="projectGroups"></param>
        /// <returns></returns>
        private bool TryDeserialiseGroups(string text, out IEnumerable<ProjectGroup>? projectGroups)
        {
            var projectList = JsonSerializer.Deserialize<ProjectsData>(text, JsonSerialisation.SerialiserOptions);

            if (projectList is null)
            {
                projectGroups = null;
                return false;
            }

            projectGroups = projectList.ProjectGroups;

            return true;
        }

        /// <summary>
        /// Try deserialising text in the legacy format which is a plain list of projects
        /// </summary>
        /// <param name="text"></param>
        /// <param name="projectGroups"></param>
        /// <returns></returns>
        private bool TryDeserialiseLegacy(string text, out IEnumerable<ProjectGroup>? projectGroups)
        {
            var projectList = JsonSerializer.Deserialize<LegacyProjectsData>(text, JsonSerialisation.SerialiserOptions);

            var dictionary = new Dictionary<string, List<Project>>();

            if (projectList is null)
            {
                projectGroups = null;
                return false;
            }

            foreach (var project in projectList.Projects)
            {
                var groupName = string.IsNullOrWhiteSpace(project.Group) ? Project.UNGROUPED : project.Group;

                if (!dictionary.ContainsKey(groupName))
                {
                    dictionary.Add(groupName, new List<Project>());
                }

                dictionary[groupName].Add(project);
            }

            projectGroups = dictionary
                .OrderBy(d => d.Key != Project.UNGROUPED)
                .ThenBy(d => d.Key)
                .Select(d => new ProjectGroup() { Name = d.Key, Projects = d.Value.OrderBy(v => v.Name).ToList() });

            return true;
        }

    }
}
