using System;
using System.Collections.Generic;

namespace WorkTimer4.API.Data
{
    public sealed class Project : ViewModelBase, IEquatable<Project>
    {
        public const string UNGROUPED = "Ungrouped";
        private string projectCode;
        private string activityCode;
        private string name;
        private string category;
        private string group;
        private string colour;
        private string icon;
        private bool active;


        /// <summary>
        /// Identifier to allow matching of items in Settings window when modifying project properties
        /// </summary>
        [System.Text.Json.Serialization.JsonIgnore]
        internal Guid Identifier { get; } = Guid.NewGuid();

        /// <summary>
        /// Gets or sets the project code
        /// </summary>
        public string ProjectCode
        {
            get
            {
                return this.projectCode;
            }
            set
            {
                this.SetProperty(ref this.projectCode, value);
            }
        }

        /// <summary>
        /// Gets or sets the project activity code
        /// </summary>
        public string ActivityCode
        {
            get
            {
                return this.activityCode;
            }
            set
            {
                this.SetProperty(ref this.activityCode, value);
            }
        }

        /// <summary>
        /// Gets or sets the project name
        /// </summary>
        public string Name
        {
            get
            {
                return this.name;
            }
            set
            {
                this.SetProperty(ref this.name, value);
            }
        }

        /// <summary>
        /// Gets or sets the project category
        /// </summary>
        public string Category
        {
            get
            {
                return this.category;
            }
            set
            {
                this.SetProperty(ref this.category, value);
            }
        }

        /// <summary>
        /// Gets or sets the project group
        /// </summary>
        public string Group
        {
            get
            {
                return this.group;
            }
            set
            {
                this.SetProperty(ref this.group, value);
            }
        }

        /// <summary>
        /// Gets or sets a hex code for the project colour
        /// </summary>
        public string Colour
        {
            get
            {
                return this.colour;
            }
            set
            {
                this.SetProperty(ref this.colour, value);
            }
        }

        /// <summary>
        /// Gets or sets a Base-64 encoded image for the project icon
        /// </summary>
        public string Icon
        {
            get
            {
                return this.icon;
            }
            set
            {
                this.SetProperty(ref this.icon, value);
            }
        }

        /// <summary>
        /// Gets or sets a value indicating whether the project is active
        /// </summary>
        public bool Active
        {
            get
            {
                return this.active;
            }
            set
            {
                this.SetProperty(ref this.active, value);
            }
        }


        public override bool Equals(object obj)
        {
            return this.Equals(obj as Project);
        }

        public bool Equals(Project other)
        {
            return other != null && Guid.Equals(this.Identifier, other.Identifier);
        }

        public override int GetHashCode()
        {
            return this.Identifier.GetHashCode();
        }

        public override string ToString()
        {
            return $"{this.Name} ({this.ProjectCode} / {this.ActivityCode})";
        }

        public static bool operator ==(Project left, Project right)
        {
            return EqualityComparer<Project>.Default.Equals(left, right);
        }

        public static bool operator !=(Project left, Project right)
        {
            return !(left == right);
        }

        public static Project Copy(Project sourceProject)
        {
            if (sourceProject == null)
            {
                return new Project() { Name = "New Project" };
            }

            return new Project()
            {
                Name = sourceProject.Name,
                Active = sourceProject.Active,
                ActivityCode = sourceProject.ActivityCode,
                Category = sourceProject.Category,
                Colour = sourceProject.Colour,
                Group = sourceProject.Group,
                Icon = sourceProject.Icon,
                ProjectCode = sourceProject.ProjectCode
            };
        }
    }
}
