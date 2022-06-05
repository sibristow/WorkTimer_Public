using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using WorkTimer4.API.Data;

namespace WorkTimer4.ViewModels
{
    /// <summary>
    /// Represents a group of projects in the UI
    /// </summary>
    internal class ProjectGroup : ObservableObject
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


        public ProjectGroup(string name, IEnumerable<Project> orderedEnumerables)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new ArgumentException($"'{nameof(name)}' cannot be null or whitespace", nameof(name));
            }

            this.Name = name;
            this.Projects = new ObservableCollection<Project>(orderedEnumerables);
        }



        public override string? ToString()
        {
            return this.Name;
        }

        public override bool Equals(object? obj)
        {
            return this.Equals(obj as ProjectGroup);
        }

        public bool Equals(ProjectGroup? other)
        {
            return other != null &&
                   EqualityComparer<string>.Default.Equals(this.Name, other.Name);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.Name);
        }

        public static bool operator ==(ProjectGroup? left, ProjectGroup? right)
        {
            return EqualityComparer<ProjectGroup>.Default.Equals(left, right);
        }

        public static bool operator !=(ProjectGroup? left, ProjectGroup? right)
        {
            return !(left == right);
        }


        public static ProjectGroup Copy(ProjectGroup sourceProjectGroup)
        {
            if (sourceProjectGroup == null)
                return new ProjectGroup("New Group", Enumerable.Empty<Project>());

            var newItems = sourceProjectGroup.Projects.Select(p => Project.Copy(p));

            var group = new ProjectGroup(sourceProjectGroup.Name, newItems);

            return group;
        }
    }
}
