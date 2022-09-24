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
        protected string? dataFile;
        private FileSystemWatcher? watcher;
        private bool reloadOnChanges;

        /// <inheritdoc/>
        public event EventHandler<EventArgs>? ProjectReloadRequest;

        /// <inheritdoc/>
        public abstract string Name { get; }

        /// <summary>
        /// Gets or sets the data file where project information is serialised to
        /// </summary>
        [Category("Projects")]
        [DisplayName("Data File")]
        [Description("File where project data is read from and saved to")]
        public string? DataFile
        {
            get
            {
                return dataFile;
            }
            set
            {
                dataFile = value;
                this.DataFilePathChanged();
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the data file should be reloaded when external changes are detected
        /// </summary>
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

        /// <inheritdoc/>
        public IEnumerable<ProjectGroup> GetProjects()
        {
            if (!File.Exists(this.DataFile))
            {
                return new List<ProjectGroup>();
            }

            var text = File.ReadAllText(this.DataFile);

            return this.Deserialise(text);
        }

        /// <inheritdoc/>
        public void WriteProjects(IEnumerable<ProjectGroup> projectGroups)
        {
            var projectsData = this.Serialise(projectGroups);

            if (this.ReloadOnChanges && this.watcher != null)
            {
                // pause watching when we are writing to the file
                this.watcher.EnableRaisingEvents = false;
            }

            // serialise the project data
            var text = JsonSerializer.Serialize(projectsData, Json.JsonSerialisation.SerialiserOptions);

            // create folder if not exists
            var folderPath = Path.GetDirectoryName(this.DataFile);

            if (string.IsNullOrWhiteSpace(this.DataFile) || string.IsNullOrWhiteSpace(folderPath))
                throw new ConnectorException("Project connector data file is not specified");

            if (!Directory.Exists(folderPath))
            {
                Directory.CreateDirectory(folderPath);
            }

            // write the project data
            File.WriteAllText(this.DataFile, text);

            if (this.ReloadOnChanges && this.watcher != null)
            {
                // resume watching when we are writing to the file
                this.watcher.EnableRaisingEvents = true;
            }
        }

        /// <summary>
        /// Returns an instance of <typeparamref name="T"/> for serialisation which contains the project information
        /// </summary>
        /// <param name="projects"></param>
        /// <returns></returns>
        protected abstract T Serialise(IEnumerable<ProjectGroup> projectGroups);

        /// <summary>
        /// Returns the deserialised project information
        /// </summary>
        /// <param name="json">json text</param>
        /// <returns></returns>
        protected abstract IEnumerable<ProjectGroup> Deserialise(string json);

        /// <summary>
        /// The selected
        /// </summary>
        private void DataFilePathChanged()
        {
            if (File.Exists(this.DataFile))
            {
                // the file exists, load the projects from the file
                this.ProjectReloadRequest?.Invoke(this, EventArgs.Empty);
            }

            // disable the file watcher on the old data file
            this.DisableWatcher();

            if (this.ReloadOnChanges)
                this.EnableWatcher();
        }

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
            if (string.IsNullOrWhiteSpace(this.DataFile) || !Directory.Exists(this.DataFile))
                return;

            if (this.watcher == null)
            {
                this.watcher = new FileSystemWatcher()
                {
                    IncludeSubdirectories = false
                };
            }

            this.watcher.Path = Path.GetDirectoryName(this.DataFile) ?? string.Empty;
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
