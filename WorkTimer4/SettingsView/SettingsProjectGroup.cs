using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using WorkTimer4.API.Data;

namespace WorkTimer4.SettingsView
{
    /// <summary>
    /// Represents a group of projects in the UI
    /// </summary>
    internal class SettingsProjectGroup : ObservableObject
    {
        private string name;

        /// <summary>
        /// Gets the group name
        /// </summary>
        public string Name
        {
            get
            {
                return name;
            }
            set
            {
                name = value;
                this.OnPropertyChanged(nameof(Name));
            }
        }

        /// <summary>
        /// Gets the projects in the group
        /// </summary>
        public ObservableCollection<Project> Projects { get; }



        public SettingsProjectGroup(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException($"'{nameof(name)}' cannot be null or whitespace", nameof(name));
            }

            this.name = name;
            this.Projects = new ObservableCollection<Project>();
        }

        public SettingsProjectGroup(string name, IEnumerable<Project> orderedEnumerables)
            : this(name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException($"'{nameof(name)}' cannot be null or whitespace", nameof(name));
            }

            this.name = name;
            this.Projects = new ObservableCollection<Project>(orderedEnumerables);
        }

        public SettingsProjectGroup(ProjectGroup projectGroup)
            : this (projectGroup.Name, projectGroup.Projects.Select(p => Project.Copy(p)))
        {
        }

       

        public override string? ToString()
        {
            return this.Name;
        }

        public override bool Equals(object? obj)
        {
            return this.Equals(obj as SettingsProjectGroup);
        }

        public bool Equals(SettingsProjectGroup? other)
        {
            return other is not null &&
                   EqualityComparer<string>.Default.Equals(this.Name, other.Name);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.Name);
        }

        public static bool operator ==(SettingsProjectGroup? left, SettingsProjectGroup? right)
        {
            return EqualityComparer<SettingsProjectGroup>.Default.Equals(left, right);
        }

        public static bool operator !=(SettingsProjectGroup? left, SettingsProjectGroup? right)
        {
            return !(left == right);
        }

        public static implicit operator ProjectGroup(SettingsProjectGroup settingsProjectGroup)
        {
            return new ProjectGroup()
            {
                Name = settingsProjectGroup.Name,
                Projects = settingsProjectGroup.Projects.Select(p => Project.Copy(p)).ToList()
            };
        }

        public static implicit operator SettingsProjectGroup(ProjectGroup projectGroup)
        {
            return new SettingsProjectGroup(projectGroup);
        }
    }
}
