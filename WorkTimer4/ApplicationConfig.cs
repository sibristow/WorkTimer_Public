using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text.Json;
using WorkTimer4.API.Connectors;
using WorkTimer4.API.Data;
using WorkTimer4.API.Json;
using WorkTimer4.Connectors;
using WorkTimer4.ViewModels;

namespace WorkTimer4
{
    internal class ApplicationConfig
    {
        public IProjectConnector? ProjectConnector { get; set; }

        public ITimesheetConnector? TimesheetConnector { get; set; }

        public ObservableCollection<ProjectGroup> Projects { get; set; }



        internal ApplicationConfig()
        {
            this.Projects = new ObservableCollection<ProjectGroup>();
        }


        public static ApplicationConfig Open()
        {
            var config = new ApplicationConfig();

            var configText = File.ReadAllText("config.json");
            var appConfigJson = JsonSerializer.Deserialize<ApplicationConfigJson>(configText, JsonSerialisation.SerialiserOptions);

            if (appConfigJson == null)
            {
                return config;
            }

            // create a project provider
            config.ProjectConnector = CreateProjectConnector(appConfigJson);

            // get the provider to load in its projects
            config.GetProjects();

            // create a timesheet provider
            config.TimesheetConnector = CreateTimesheetConnector(appConfigJson);

            return config;
        }

        public static void Save(ApplicationConfig config)
        {
            if (config == null)
                throw new ArgumentNullException("Cannot save null config");

            var configJson = new ApplicationConfigJson()
            {
                ProjectConnector = config.ProjectConnector.GetType().FullName,
                ProjectConnectorOptions = GetOptions(config.ProjectConnector),
                TimesheetConnector = config.ProjectConnector.GetType().FullName,
                TimesheetConnectorOptions = GetOptions(config.TimesheetConnector)
            };

            var serialised = JsonSerializer.Serialize(configJson);
            File.WriteAllText("config.json", serialised);
        }


        internal static IProjectConnector? CreateProjectConnector(IProjectConnector sourceConnector)
        {
            if (sourceConnector == null)
                return null;

            return CreateConnector(sourceConnector);
        }

        internal static ITimesheetConnector? CreateTimesheetConnector(ITimesheetConnector sourceConnector)
        {
            if (sourceConnector == null)
                return null;

            return CreateConnector(sourceConnector);
        }


        private static IProjectConnector CreateProjectConnector(ApplicationConfigJson appConfigJson)
        {
            var provider = CreateConnector<IProjectConnector, DefaultProjectConnector>(appConfigJson.ProjectConnector, appConfigJson.ProjectConnectorOptions);

            return provider;
        }

        private static ITimesheetConnector CreateTimesheetConnector(ApplicationConfigJson appConfigJson)
        {
            var provider = CreateConnector<ITimesheetConnector, DefaultTimesheetConnector>(appConfigJson.TimesheetConnector, appConfigJson.TimesheetConnectorOptions);

            return provider;
        }

        private static TConnector CreateConnector<TConnector, TDefault>(string connectorTypeString, Dictionary<string, object> connectorOptions)
        {
            var type = System.Type.GetType(connectorTypeString);

            if (type == null || !type.IsAssignableTo(typeof(TConnector)))
            {
                type = typeof(TDefault);
            }

            // create an instance of the required connector
            TConnector connector = (TConnector)System.Activator.CreateInstance(type);

            if (connector == null)
            {
                throw new System.Exception($"Cannot create instance of {typeof(TConnector)}");
            }

            // set the connector's options
            foreach (var providerOption in connectorOptions)
            {
                var propInfo = type.GetProperty(providerOption.Key, System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
                if (propInfo == null || propInfo.SetMethod == null)
                    continue;

                var jsonElement = (JsonElement)providerOption.Value;
                var setValue = jsonElement.ToObject(propInfo.PropertyType);

                propInfo.SetMethod.Invoke(connector, new object[] { setValue });
            }

            return connector;
        }

        private static TConnector? CreateConnector<TConnector>(TConnector sourceConnector)
        {
            if (sourceConnector == null)
                return default(TConnector);

            var type = sourceConnector.GetType();

            // create an instance of the required connector
            TConnector newConnector = (TConnector)System.Activator.CreateInstance(type);

            if (newConnector == null)
            {
                throw new System.Exception($"Cannot create instance of {typeof(TConnector)}");
            }

            var sourceProps = type.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            foreach(var sourceProp in sourceProps)
            {
                if (sourceProp.SetMethod == null)
                    continue;

                var sourceVal = sourceProp.GetMethod.Invoke(sourceConnector, null);
                sourceProp.SetMethod.Invoke(newConnector, new object[] { sourceVal });
            }

            return newConnector;
        }

        private static Dictionary<string, object?> GetOptions(object? connector)
        {
            var dictionary = new Dictionary<string, object?>();

            if (connector == null)
                return dictionary;

            var sourceProps = connector.GetType().GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            foreach (var sourceProp in sourceProps)
            {
                if (sourceProp.SetMethod == null)
                    continue;

                object? sourceVal = sourceProp.GetMethod.Invoke(connector, null);

                dictionary.Add(sourceProp.Name, sourceVal);
            }

            return dictionary;
        }


        private void GetProjects()
        {
            this.Projects.Clear();

            if (this.ProjectConnector == null)
            {
                return;
            }

            var projects = this.ProjectConnector.GetProjects();

            var dictionary = new Dictionary<string, List<Project>>();

            foreach (var project in projects)
            {
                var groupName = string.IsNullOrWhiteSpace(project.Group) ? Project.UNGROUPED : project.Group;

                if (!dictionary.ContainsKey(groupName))
                {
                    dictionary.Add(groupName, new List<Project>());
                }

                dictionary[groupName].Add(project);
            }

            var groupedProjects = dictionary.OrderBy(d => d.Key).Select(d => new ProjectGroup(d.Key, d.Value.OrderBy(v => v.Name)));
            this.Projects = new ObservableCollection<ProjectGroup>(groupedProjects);
        }
    }
}
