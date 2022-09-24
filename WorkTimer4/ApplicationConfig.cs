using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Reflection;
using System.Text.Json;
using WorkTimer4.API.Connectors;
using WorkTimer4.API.Data;
using WorkTimer4.API.Json;
using WorkTimer4.Connectors;

namespace WorkTimer4
{
    internal class ApplicationConfig
    {
        private const string CONFIG_FILE = "config.json";

        private IProjectConnector? projectConnector;

        public IProjectConnector? ProjectConnector
        {
            get
            {
                return projectConnector;
            }
            set
            {
                this.DetachProjectConnectorEvents();
                projectConnector = value;
                this.AttachProjectConnectorEvents();
            }
        }

        public ITimesheetConnector? TimesheetConnector { get; set; }

        public ObservableCollection<ProjectGroup> ProjectGroups { get; set; }



        internal ApplicationConfig()
        {
            this.ProjectGroups = new ObservableCollection<ProjectGroup>();
        }


        public static ApplicationConfig Open()
        {
            var config = new ApplicationConfig();

            var configText = File.ReadAllText(CONFIG_FILE);
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
                ProjectConnector = config.ProjectConnector?.GetType().FullName,
                ProjectConnectorOptions = GetOptions(config.ProjectConnector),
                TimesheetConnector = config.TimesheetConnector?.GetType().FullName,
                TimesheetConnectorOptions = GetOptions(config.TimesheetConnector)
            };

            var serialised = JsonSerializer.Serialize(configJson, JsonSerialisation.SerialiserOptions);
            File.WriteAllText(CONFIG_FILE, serialised);
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


        private static IProjectConnector CreateProjectConnector(ApplicationConfigJson? appConfigJson)
        {
            if (appConfigJson == null)
            {
                throw new ArgumentNullException(nameof(appConfigJson));
            }

            var provider = CreateConnector<IProjectConnector, DefaultProjectConnector>(appConfigJson.ProjectConnector, appConfigJson.ProjectConnectorOptions);

            return provider;
        }

        private static ITimesheetConnector CreateTimesheetConnector(ApplicationConfigJson? appConfigJson)
        {
            if (appConfigJson == null)
            {
                throw new ArgumentNullException(nameof(appConfigJson));
            }

            var provider = CreateConnector<ITimesheetConnector, DefaultTimesheetConnector>(appConfigJson.TimesheetConnector, appConfigJson.TimesheetConnectorOptions);

            return provider;
        }

        /// <summary>
        /// Creates a new connector instance from its type string and sets the properties from the options dictionary
        /// </summary>
        /// <typeparam name="TConnector"></typeparam>
        /// <typeparam name="TDefault"></typeparam>
        /// <param name="connectorTypeString"></param>
        /// <param name="connectorOptions"></param>
        /// <returns></returns>
        private static TConnector CreateConnector<TConnector, TDefault>(string? connectorTypeString, Dictionary<string, object?> connectorOptions)
        {
            Type? type = null;

            // string may be empty/null if config json was not deserialisd or property is missing/empty
            if (!string.IsNullOrWhiteSpace(connectorTypeString))
            {
                type = Type.GetType(connectorTypeString);
            }

            if (type == null || !type.IsAssignableTo(typeof(TConnector)))
            {
                type = typeof(TDefault);
            }

            // create an instance of the required connector
            var obj = Activator.CreateInstance(type);

            if (obj == null)
            {
                throw new Exception($"Cannot create instance of {typeof(TConnector)}");
            }

            TConnector connector = (TConnector)obj;

            // set the connector's options
            foreach (var providerOption in connectorOptions)
            {
                var propInfo = type.GetProperty(providerOption.Key, BindingFlags.IgnoreCase | BindingFlags.Public | BindingFlags.Instance);
                if (propInfo == null || propInfo.SetMethod == null)
                    continue;

                var jsonElement = (JsonElement?)providerOption.Value;
                var setValue = jsonElement?.ToObject(propInfo.PropertyType) ?? null;

                propInfo.SetMethod.Invoke(connector, new object[] { setValue! });
            }

            return connector;
        }

        /// <summary>
        /// Creates a new connector instance from an existing connector instance
        /// </summary>
        /// <typeparam name="TConnector"></typeparam>
        /// <param name="sourceConnector"></param>
        /// <returns></returns>
        private static TConnector? CreateConnector<TConnector>(TConnector sourceConnector)
        {
            if (sourceConnector == null)
                return default(TConnector);

            var type = sourceConnector.GetType();

            // create an instance of the required connector
            var obj = Activator.CreateInstance(type);

            if (obj == null)
            {
                throw new Exception($"Cannot create instance of {typeof(TConnector)}");
            }

            TConnector newConnector = (TConnector)obj;

            // set the properties
            var sourceProps = type.GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach(var sourceProp in sourceProps)
            {
                if (sourceProp == null || sourceProp.GetMethod == null || sourceProp.SetMethod == null)
                    continue;

                var sourceVal = sourceProp.GetMethod.Invoke(sourceConnector, null);
                sourceProp.SetMethod.Invoke(newConnector, new object[] { sourceVal! });
            }

            return newConnector;
        }

        private static Dictionary<string, object?> GetOptions(object? connector)
        {
            var dictionary = new Dictionary<string, object?>();

            if (connector == null)
                return dictionary;

            var sourceProps = connector.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance);
            foreach (var sourceProp in sourceProps)
            {
                if (sourceProp == null || sourceProp.GetMethod == null || sourceProp.SetMethod == null)
                    continue;

                object? sourceVal = sourceProp.GetMethod.Invoke(connector, null);

                dictionary.Add(sourceProp.Name, sourceVal);
            }

            return dictionary;
        }


        internal void GetProjects()
        {
            this.ProjectGroups.Clear();

            if (this.ProjectConnector == null)
            {
                return;
            }

            var projects = this.ProjectConnector.GetProjects();

            if (projects == null)
            {
                return;
            }

            foreach(var gp in projects)
            {
                this.ProjectGroups.Add(gp);
            }
        }

        private void AttachProjectConnectorEvents()
        {
            if (this.ProjectConnector == null)
                return;

            this.ProjectConnector.ProjectReloadRequest += this.ProjectConnector_ProjectReloadRequest;
        }

        private void DetachProjectConnectorEvents()
        {
            if (this.ProjectConnector == null)
                return;

            this.ProjectConnector.ProjectReloadRequest -= this.ProjectConnector_ProjectReloadRequest;
        }

        private void ProjectConnector_ProjectReloadRequest(object? sender, EventArgs e)
        {
            this.GetProjects();
        }
    }
}
