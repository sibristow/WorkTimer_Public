using System;
using System.Collections.Generic;

namespace WorkTimer4.API.Data
{
    public sealed class Activity : ViewModelBase, IEquatable<Activity>
    {
        /// <summary>
        /// Gets the time the activity started
        /// </summary>
        public DateTimeOffset Start { get; private set; }

        /// <summary>
        /// Gets or sets the time the activity ended
        /// </summary>
        public DateTimeOffset End { get; set; }

        /// <summary>
        /// Gets the activity's project
        /// </summary>
        public Project Project { get; private set; }


        public Activity(Project project)
            : this(project, DateTimeOffset.UtcNow)
        {
        }

        public Activity(Project project, DateTimeOffset start)
        {
            this.Project = project;
            this.Start = start;
        }


        public override bool Equals(object? obj)
        {
            return this.Equals(obj as Activity);
        }

        public bool Equals(Activity? other)
        {
            return other is not null &&
                   this.Start == other.Start &&
                   this.End == other.End &&
                   this.Project.Equals(other.Project);
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(this.Start, this.End, this.Project);
        }

        public override string ToString()
        {
            return string.Format("{0} ({1} to {2})", this.Project.Name, this.Start, this.End);
        }

        public static bool operator ==(Activity? left, Activity? right)
        {
            return EqualityComparer<Activity>.Default.Equals(left, right);
        }

        public static bool operator !=(Activity? left, Activity? right)
        {
            return !(left == right);
        }
    }
}
