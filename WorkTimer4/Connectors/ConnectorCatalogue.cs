using System.Collections.Generic;
using System.Composition;
using System.Composition.Hosting;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Loader;
using WorkTimer4.API.Connectors;

namespace WorkTimer4.Connectors
{
    internal class ConnectorCatalogue
    {
        [ImportMany]
        public IEnumerable<IProjectConnector> ProjectConnectors { get; set; }

        [ImportMany]
        public IEnumerable<ITimesheetConnector> TimesheetConnectors { get; set; }

        public ConnectorCatalogue()
        {
            this.ProjectConnectors = new List<IProjectConnector>();
            this.TimesheetConnectors = new List<ITimesheetConnector>();
        }

        internal void Compose()
        {
            var thisAssembly = typeof(ConnectorCatalogue).Assembly;

            // look in this assembly (to load the default connectors)
            var configuration = new ContainerConfiguration()
                .WithAssembly(thisAssembly);

            // include plugins folder
            configuration = WithPlugins(configuration);

            // build the catalogue
            using (var container = configuration.CreateContainer())
            {
                container.SatisfyImports(this);
            }
        }

        private static ContainerConfiguration WithPlugins(ContainerConfiguration configuration)
        {
            // now look in the Plugins folder
            var executableLocation = Assembly.GetEntryAssembly()?.Location;
            var executablePath = Path.GetDirectoryName(executableLocation);

            if (string.IsNullOrEmpty(executablePath))
            {
                throw new DirectoryNotFoundException("Cannot determine location of plugins folder");
            }

            var pluginPath = Path.Combine(executablePath, "Plugins");

            if (Directory.Exists(pluginPath))
            {
                var assemblies = Directory
                            .GetFiles(pluginPath, "*.dll", SearchOption.AllDirectories)
                            .Select(AssemblyLoadContext.Default.LoadFromAssemblyPath)
                            .ToList();

                configuration = configuration.WithAssemblies(assemblies);
            }

            return configuration;
        }
    }
}
