using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Microsoft.Toolkit.Mvvm.ComponentModel;
using Microsoft.Toolkit.Mvvm.Input;
using Microsoft.Win32;
using WorkTimer4.API.Connectors;
using WorkTimer4.API.Data;
using WorkTimer4.Connectors;

namespace WorkTimer4.ViewModels
{
    internal class MainWindowViewModel : ObservableRecipient, ISettingsWindow
    {
        private readonly ApplicationConfig applicationConfig;
        private readonly ConnectorCatalogue catalogue;
        private IProjectConnector? selectedProjectConnector;
        private ITimesheetConnector? selectedTimesheetConnector;
        private Project selectedProject;

        public event EventHandler<EventArgs> SettingsApplied;

        public ICommand SourceUpdatedCommand { get; }

        public ICommand ApplySettingsCommand { get; }

        public ICommand SelectedProjectChangedCommand { get; }

        public ICommand ClearIconCommand { get; }

        public ICommand OpenIconCommand { get; }

        public ICommand AddProjectCommand { get; }

        public ICommand DeleteProjectCommand { get; }


        public IEnumerable<ITimesheetConnector> TimesheetConnectors
        {
            get
            {
                return this.catalogue.TimesheetConnectors.OrderBy(pc => pc.Name);//.Select(p => p.Name);
            }
        }

        public ITimesheetConnector? SelectedTimesheetConnector
        {
            get
            {
                return this.selectedTimesheetConnector;
            }
            set
            {
                this.selectedTimesheetConnector = value;
                this.OnPropertyChanged(nameof(SelectedTimesheetConnector));
            }
        }

        public IEnumerable<IProjectConnector> ProjectConnectors
        {
            get
            {
                return this.catalogue.ProjectConnectors.OrderBy(pc => pc.Name);//.Select(p => p.Name);
            }
        }

        public IProjectConnector? SelectedProjectConnector
        {
            get
            {
                return this.selectedProjectConnector;
            }
            set
            {
                this.selectedProjectConnector = value;
                this.OnPropertyChanged(nameof(SelectedProjectConnector));
            }
        }
       
        public ObservableCollection<ProjectGroup> ProjectList { get; }

        public Project SelectedProject
        {
            get
            {
                return selectedProject;
            }
            set
            {
                selectedProject = value;
                this.OnPropertyChanged(nameof(SelectedProject));
            }
        }


        public MainWindowViewModel(ApplicationConfig applicationConfig, ConnectorCatalogue catalogue)
        {
            this.applicationConfig = applicationConfig;
            this.catalogue = catalogue;

            this.ApplySettingsCommand = new RelayCommand(this.OnApplySettings);
            this.SourceUpdatedCommand = new RelayCommand<object>(this.OnSourceUpdated);
            this.SelectedProjectChangedCommand = new RelayCommand<object>(this.OnSelectedProjectChanged);
            this.ClearIconCommand = new RelayCommand<Project>(this.OnClearIcon);
            this.OpenIconCommand = new RelayCommand<Project>(this.OnOpenIcon);
            this.AddProjectCommand = new RelayCommand(this.OnAddProject);
            this.DeleteProjectCommand = new RelayCommand(this.OnDeleteProject);

            this.ProjectList = new ObservableCollection<ProjectGroup>();

            // get current settings from app config
            this.GetCurrentSettings();
        }

        /// <summary>
        /// Adds a new project to the "Ungrouped" group
        /// </summary>
        private void OnAddProject()
        {
            var project = Project.CreateDefault();

            this.AddProjectToGroup(project);

            this.SelectedProject = project;
        }

        /// <summary>
        /// Deletes the selected project 
        /// </summary>
        private void OnDeleteProject()
        {
            if (this.SelectedProject == null)
                return;

            this.RemoveProjectFromGroup(this.SelectedProject);

            this.OnPropertyChanged(nameof(ProjectGroup.Projects));
        }

        /// <summary>
        /// Handles the SelectedItemChanged event of the treeview
        /// </summary>
        /// <param name="obj"></param>
        private void OnSelectedProjectChanged(object? obj)
        {
            var args = obj as RoutedPropertyChangedEventArgs<object>;

            this.SelectedProject = args?.NewValue as Project;           
        }

        /// <summary>
        /// Handles the NotifySourceUpdated event from bindings
        /// </summary>
        /// <param name="obj"></param>
        private void OnSourceUpdated(object? obj)
        {
            var args = obj as DataTransferEventArgs;

            if (args == null)
                return;

            var project = ((FrameworkElement)args.TargetObject).DataContext as Project;
            if (project != null)
            {
                this.ProjectUpdated(project);
            }
        }

        /// <summary>
        /// Initialises this viewmodel based on the current app setttings
        /// </summary>
        private void GetCurrentSettings()
        {
            if (this.applicationConfig.ProjectConnector != null)
            {
                this.selectedProjectConnector = ApplicationConfig.CreateProjectConnector(this.applicationConfig.ProjectConnector);                
            }

            if (this.applicationConfig.TimesheetConnector != null)
            {
                this.selectedTimesheetConnector = ApplicationConfig.CreateTimesheetConnector(this.applicationConfig.TimesheetConnector);
            }

            if (this.applicationConfig.Projects != null)
            {
                foreach (var project in this.applicationConfig.Projects)
                {
                    this.ProjectList.Add(ProjectGroup.Copy(project));
                }
            }
        }

        /// <summary>
        /// Applies the viewmodel to the app settings
        /// </summary>
        private void OnApplySettings()
        {
            try
            {
                // update the current app config
                this.applicationConfig.ProjectConnector = this.SelectedProjectConnector;
                this.applicationConfig.TimesheetConnector = this.SelectedTimesheetConnector;

                // save the app config
                ApplicationConfig.Save(this.applicationConfig);

                // save the project list
                var projects = this.ProjectList.SelectMany(pg => pg.Projects);
                this.applicationConfig.ProjectConnector.WriteProjects(projects);
                
                // raise the event which will close the form
                this.SettingsApplied?.Invoke(this, EventArgs.Empty);
            }
            catch(Exception ex)
            {
                MessageBox.Show("An error occurred applying the new settings.", AssemblyInfo.ProductName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Clears the icon from the selected project
        /// </summary>
        /// <param name="obj"></param>
        private void OnClearIcon(Project? obj)
        {
            if (obj == null)
                return;

            obj.Icon = null;
        }

        /// <summary>
        /// Selects a new icon for the selected project
        /// </summary>
        /// <param name="obj"></param>
        private void OnOpenIcon(Project? obj)
        {
            var project = obj as Project;
            if (project == null)
                return;

            var dialog = new OpenFileDialog()
            {
                CheckPathExists = true,
                AddExtension = true,
                DefaultExt = "png",
                Filter = "PNG files | *.png",
                Multiselect = false,
                Title = "Select Project Icon"
            };

            var result = dialog.ShowDialog();
            if (result != true)
            {
                return;
            }

            project.Icon = Assets.WinFormsAssets.ToEncodedImage(dialog.FileName);
        }


        /// <summary>
        /// Update the project when a binding has been updated
        /// </summary>
        /// <param name="project"></param>
        private void ProjectUpdated(Project project)
        {           
            // move project to new group if required?
            this.MoveToGroup(project);

            this.OnPropertyChanged(nameof(ProjectGroup.Projects));
        }

        /// <summary>
        /// Find the group which currently contains the project
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        private ProjectGroup? FindGroupForProject(Project project)
        {
            foreach (var group in this.ProjectList)
            {
                if (group.Projects.Contains(project))
                {
                    return group;
                }
            }

            return this.ProjectList.FirstOrDefault(g => g.Name.Equals(Project.UNGROUPED));
        }

        /// <summary>
        /// Move project to new group
        /// </summary>
        /// <param name="project"></param>
        /// <param name="currentGroup"></param>
        private void MoveToGroup(Project project)
        {
            this.RemoveProjectFromGroup(project);

            this.AddProjectToGroup(project);
        }

        /// <summary>
        /// Remove a project from it's current group
        /// </summary>
        /// <param name="project"></param>
        /// <param name="currentGroup"></param>
        private void RemoveProjectFromGroup(Project project)
        {
            var currentGroup = this.FindGroupForProject(project);
            if (currentGroup == null)
                return;

            // remove the project from it's group
            currentGroup.Projects.Remove(project);
            this.OnPropertyChanged(nameof(ProjectGroup.Projects));

            if (currentGroup.Projects.Count == 0)
            {
                // no more projects left in that group, remove the group
                this.ProjectList.Remove(currentGroup);
            }
        }

        private void AddProjectToGroup(Project project)
        {
            var newGroupName = project.Group;

            if (string.IsNullOrWhiteSpace(newGroupName))
            {
                newGroupName = Project.UNGROUPED;
            }

            var newGroup = this.ProjectList.FirstOrDefault(g => g.Name.Equals(newGroupName));
            if (newGroup != null)
            {
                newGroup.Projects.Add(project);
                return;
            }

            // create a new group
            newGroup = new ProjectGroup(newGroupName, new List<Project>() { project });
            this.ProjectList.Add(newGroup);
        }
    }
}
