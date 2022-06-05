using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Text.Json;
using WorkTimer4.API.Data;

namespace WorkTimer4.API.Connectors
{
    /// <summary>
    /// Base class for implementing a <see cref="IProjectConnector"/> which reads and writes JSON
    /// </summary>
    /// <typeparam name="T">Type which JSON is serialised from and deserialised to</typeparam>
    public abstract class JsonProjectConnector<T> : JsonProviderBase, IProjectConnector where T : class
    {
        [Category("Projects")]
        [DisplayName("Data File")]
        [Description("File where project data is read from and saved to")]
        public string DataFile { get; set; }

        public abstract string Name { get; }


        public override bool Equals(object? obj)
        {
            return obj is IProjectConnector pc && object.Equals(this.Name, pc.Name);
        }

        public override int GetHashCode()
        {
            return this.Name.GetHashCode();
        }


        public IEnumerable<Project> GetProjects()
        {           
            if (!File.Exists(this.DataFile))
            {
                return new List<Project>();
            }

            var text = File.ReadAllText(this.DataFile);

            var projectList = JsonSerializer.Deserialize<T>(text, API.Json.JsonSerialisation.SerialiserOptions);

            if (projectList == null)
            {
                throw new JsonException("Could not read the data file.");
            }

            return this.FromDeserialised(projectList);
        }

        public void WriteProjects(IEnumerable<Project> projects)
        {
            var projectsData = this.ForSerialisation(projects);           

            var text = JsonSerializer.Serialize(projectsData, API.Json.JsonSerialisation.SerialiserOptions);
            File.WriteAllText(this.DataFile, text);
        }

        protected abstract T ForSerialisation(IEnumerable<Project> projects);

        protected abstract IEnumerable<Project> FromDeserialised(T deserialised);        
    }
}
