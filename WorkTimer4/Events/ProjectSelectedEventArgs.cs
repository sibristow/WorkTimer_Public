using System;
using WorkTimer4.API.Data;

namespace WorkTimer4.Events
{
    public class ProjectSelectedEventArgs : EventArgs
    {
        public bool IsStopped { get { return this.Project == null; } }

        public Project? Project { get; }


        public ProjectSelectedEventArgs(Project? project)
        {            
            this.Project = project;
        }
    }
}
