using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using WorkTimer4.API.Connectors;
using WorkTimer4.API.Data;
using WorkTimer4.Connectors;
using WorkTimer4.SettingsView.Pages;
using WorkTimer4.ViewModels;

namespace WorkTimer4.SettingsView
{
    internal class SettingsViewModel : ObservableRecipient, ISettingsWindow
    {
        private readonly ApplicationConfig applicationConfig;
        private readonly ConnectorCatalogue catalogue;
        private IProjectConnector? selectedProjectConnector;
        private ITimesheetConnector? selectedTimesheetConnector;
        private Project? selectedProject;
        private ISettingsPage? selectedPage;
        private SettingsProjectGroup? selectedProjectGroup;

        public event EventHandler<EventArgs>? SettingsApplied;

        public ObservableCollection<ISettingsPage> Pages { get; }

        public ISettingsPage? SelectedPage
        {
            get => this.selectedPage;
            set
            {
                value ??= this.Pages.FirstOrDefault();

                SetProperty(ref this.selectedPage, value);
            }
        }



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
                if (this.selectedProjectConnector is not null)
                    this.selectedProjectConnector.ProjectReloadRequest -= this.SelectedProjectConnector_ProjectReloadRequest;

                this.selectedProjectConnector = value;

                if (this.selectedProjectConnector is not null)
                    this.selectedProjectConnector.ProjectReloadRequest += this.SelectedProjectConnector_ProjectReloadRequest;

                this.OnPropertyChanged(nameof(SelectedProjectConnector));
            }
        }



        public ObservableCollection<SettingsProjectGroup> ProjectList { get; }


        public SettingsProjectGroup? SelectedProjectGroup
        {
            get
            {
                return selectedProjectGroup;
            }
            set
            {
                this.SetProperty(ref selectedProjectGroup, value);
            }
        }

        public Project? SelectedProject
        {
            get
            {
                return selectedProject;
            }
            set
            {
                this.SetProperty(ref selectedProject, value);
            }
        }


        public SettingsViewModel(ApplicationConfig? applicationConfig, ConnectorCatalogue? catalogue)
        {
            this.applicationConfig = applicationConfig ?? throw new ArgumentNullException(nameof(applicationConfig));
            this.catalogue = catalogue ?? throw new ArgumentNullException(nameof(catalogue));

            this.ApplySettingsCommand = new RelayCommand(this.OnApplySettings);
            this.SourceUpdatedCommand = new RelayCommand<object>(this.OnSourceUpdated);
            this.SelectedProjectChangedCommand = new RelayCommand<object>(this.OnSelectedProjectChanged);
            this.ClearIconCommand = new RelayCommand<Project>(this.OnClearIcon);
            this.OpenIconCommand = new RelayCommand<Project>(this.OnOpenIcon);
            this.AddProjectCommand = new RelayCommand(this.OnAddProject);
            this.DeleteProjectCommand = new RelayCommand(this.OnDeleteProject);

            this.Pages = new ObservableCollection<ISettingsPage>(this.CreatePages());
            this.selectedPage = this.Pages.FirstOrDefault();

            this.ProjectList = new ObservableCollection<SettingsProjectGroup>();

            // get current settings from app config
            this.GetCurrentSettings();
        }

        /// <summary>
        /// Adds a new project to the "Ungrouped" group
        /// </summary>
        private void OnAddProject()
        {
            var currentGroup = this.SelectedProjectGroup?.Name;

            var project = Project.CreateDefault(currentGroup);

            var group = this.AddProjectToGroup(project);

            this.SelectedProject = project;
            this.SelectedProjectGroup = group;
        }

        /// <summary>
        /// Deletes the selected project
        /// </summary>
        private void OnDeleteProject()
        {
            if (this.SelectedProject is null)
                return;

            this.RemoveProjectFromGroup(this.SelectedProject);

            this.OnPropertyChanged(nameof(SettingsProjectGroup.Projects));
        }

        /// <summary>
        /// Handles the SelectedItemChanged event of the treeview
        /// </summary>
        /// <param name="obj"></param>
        private void OnSelectedProjectChanged(object? obj)
        {
            var args = obj as RoutedPropertyChangedEventArgs<object>;

            if (args?.NewValue is Project p)
            {
                this.SelectedProject = p;
                this.SelectedProjectGroup = this.ProjectList.FirstOrDefault(pg => pg.Name.Equals(p.Group));

                return;
            }

            if (args?.NewValue is SettingsProjectGroup group)
            {
                this.SelectedProjectGroup = group;
            }
        }

        /// <summary>
        /// Handles the NotifySourceUpdated event from bindings
        /// </summary>
        /// <param name="obj"></param>
        private void OnSourceUpdated(object? obj)
        {
            var args = obj as DataTransferEventArgs;

            if (args is null)
                return;

            var project = ((FrameworkElement)args.TargetObject).DataContext as Project;
            if (project is not null)
            {
                this.ProjectUpdated(project);
            }
        }

        /// <summary>
        /// Initialises this viewmodel based on the current app setttings
        /// </summary>
        private void GetCurrentSettings()
        {
            if (this.applicationConfig.ProjectConnector is not null)
            {
                this.selectedProjectConnector = ApplicationConfig.CreateProjectConnector(this.applicationConfig.ProjectConnector);

                if (this.selectedProjectConnector is not null)
                    this.selectedProjectConnector.ProjectReloadRequest += this.SelectedProjectConnector_ProjectReloadRequest;
            }

            if (this.applicationConfig.TimesheetConnector is not null)
            {
                this.selectedTimesheetConnector = ApplicationConfig.CreateTimesheetConnector(this.applicationConfig.TimesheetConnector);
            }

            if (this.applicationConfig.ProjectGroups is not null)
            {
                foreach (var projectGroup in this.applicationConfig.ProjectGroups)
                {
                    this.ProjectList.Add(new SettingsProjectGroup(projectGroup));
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
                var projects = this.ProjectList.Select(pg => (ProjectGroup)pg);
                this.applicationConfig?.ProjectConnector?.WriteProjects(projects);

                // raise the event which will close the form
                this.SettingsApplied?.Invoke(this, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                var logFile = ExceptionLogger.LogException(ex);

                string msg = string.Format("An error occurred applying the new settings.\r\n\r\nLog output is at\r\n{0}", logFile);
                MessageBox.Show(msg, AssemblyInfo.ProductName, MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        /// <summary>
        /// Clears the icon from the selected project
        /// </summary>
        /// <param name="obj"></param>
        private void OnClearIcon(Project? obj)
        {
            if (obj is null)
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
            if (project is null)
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

            this.OnPropertyChanged(nameof(SettingsProjectGroup.Projects));
        }

        /// <summary>
        /// Find the group which currently contains the project
        /// </summary>
        /// <param name="project"></param>
        /// <returns></returns>
        private SettingsProjectGroup? FindGroupForProject(Project project)
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
            if (currentGroup is null)
                return;

            // remove the project from it's group
            currentGroup.Projects.Remove(project);
            this.OnPropertyChanged(nameof(SettingsProjectGroup.Projects));

            if (currentGroup.Projects.Count == 0)
            {
                // no more projects left in that group, remove the group
                this.ProjectList.Remove(currentGroup);
            }
        }

        private SettingsProjectGroup AddProjectToGroup(Project project)
        {
            var newGroupName = project.Group;

            if (string.IsNullOrWhiteSpace(newGroupName))
            {
                newGroupName = Project.UNGROUPED;
            }

            var newGroup = this.ProjectList.FirstOrDefault(g => g.Name.Equals(newGroupName));
            if (newGroup is null)
            {
                // create a new group
                newGroup = new SettingsProjectGroup(newGroupName);
                this.ProjectList.Add(newGroup);
            }

            newGroup.Projects.Add(project);
            return newGroup;
        }



        private void SelectedProjectConnector_ProjectReloadRequest(object? sender, EventArgs e)
        {
            if (this.SelectedProjectConnector is null)
                return;

            this.SelectedProjectConnector.GetProjects();
        }


        private IEnumerable<ISettingsPage> CreatePages()
        {
            yield return new ProjectSettingsPage();
            yield return new ProjectsPage();
            yield return new TimesheetsPage();
            yield return new AboutPage();
        }
    }
}
