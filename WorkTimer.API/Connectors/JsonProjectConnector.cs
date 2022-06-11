using System;
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
        FileSystemWatcher watcher;
        private bool reloadOnChanges;

        public event EventHandler<EventArgs> ProjectReloadRequest;

        public abstract string Name { get; }

        [Category("Projects")]
        [DisplayName("Data File")]
        [Description("File where project data is read from and saved to")]
        public string DataFile { get; set; }

        [Category("Projects")]
        [DisplayName("Reload")]
        [Description("Reload file when external changes are detected")]
        public bool ReloadOnChanges
        {
            get
            {
                return reloadOnChanges;
            }
            set
            {
                reloadOnChanges = value;
                this.ReloadOnChanges_Changed();
            }
        }


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

            if (this.ReloadOnChanges && this.watcher != null)
            {
                // pause watching when we are writing to the file
                this.watcher.EnableRaisingEvents = false;
            }

            // write the project data
            var text = JsonSerializer.Serialize(projectsData, API.Json.JsonSerialisation.SerialiserOptions);
            File.WriteAllText(this.DataFile, text);

            if (this.ReloadOnChanges && this.watcher != null)
            {
                // resume watching when we are writing to the file
                this.watcher.EnableRaisingEvents = true;
            }
        }

        protected abstract T ForSerialisation(IEnumerable<Project> projects);

        protected abstract IEnumerable<Project> FromDeserialised(T deserialised);

        /// <summary>
        /// Called when the <see cref="ReloadOnChanges"/> property is changed
        /// </summary>
        private void ReloadOnChanges_Changed()
        {
            if (this.ReloadOnChanges)
            {
                this.EnableWatcher();
                return;
            }

            this.DisableWatcher();
        }

        /// <summary>
        /// Enables the filewatcher looking at the <see cref="DataFile"/>
        /// </summary>
        private void EnableWatcher()
        {
            if (this.watcher == null)
            {
                this.watcher = new FileSystemWatcher()
                {
                    IncludeSubdirectories = false
                };
            }

            this.watcher.Path = Path.GetDirectoryName(this.DataFile);
            this.watcher.Filter = Path.GetFileName(this.DataFile);
            this.watcher.NotifyFilter = NotifyFilters.LastWrite;
            this.watcher.Changed += this.Watcher_Changed;
            this.watcher.EnableRaisingEvents = true;
        }

        /// <summary>
        /// Disables the filewatcher looking at the <see cref="DataFile"/>
        /// </summary>
        private void DisableWatcher()
        {
            if (this.watcher == null)
                return;

            this.watcher.EnableRaisingEvents = false;
            this.watcher.Changed -= this.Watcher_Changed;
        }

        /// <summary>
        /// Raises the <see cref="IProjectConnector.ProjectReloadRequest"/> event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Watcher_Changed(object sender, FileSystemEventArgs e)
        {
            this.ProjectReloadRequest?.Invoke(this, e);
        }
    }
}
