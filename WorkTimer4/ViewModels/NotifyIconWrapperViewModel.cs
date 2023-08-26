using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using WorkTimer4.API.Data;
using WorkTimer4.Assets;

namespace WorkTimer4.ViewModels
{
    internal class NotifyIconWrapperViewModel : ObservableRecipient
    {
        private readonly ApplicationConfig applicationConfig;
        private string? toolTip;
        private Activity? currentActivity;
        private string? activeIcon;

        /// <summary>
        /// Gets a value indicating whether the viewmodel has invoked the Application.Shutdown command
        /// </summary>
        public bool ShutdownCalled { get; private set; }

        public string CurrentActivityName { get { return this.currentActivity?.Project?.Name ?? string.Empty; } }

        public string? ToolTip
        {
            get
            {
                return this.toolTip;
            }
            set
            {
                this.SetProperty(ref this.toolTip, value);
            }
        }

        public string? ActiveIcon
        {
            get
            {
                return activeIcon;
            }
            private set
            {
                this.SetProperty(ref this.activeIcon, value);
            }
        }

        public ObservableCollection<ProjectGroup> Projects { get { return this.applicationConfig.ProjectGroups; } }

        public ICommand NotifyIconExitCommand { get; }

        public ICommand ProjectSelectedCommand { get; }

        public ICommand ViewTimesheetCommand { get; }


        public NotifyIconWrapperViewModel(ApplicationConfig applicationConfig)
        {
            ArgumentNullException.ThrowIfNull(applicationConfig);
            this.applicationConfig = applicationConfig;

            this.SetToolTip(null);

            this.NotifyIconExitCommand = new RelayCommand<bool>(this.Exit);
            this.ProjectSelectedCommand = new RelayCommand<object?>(this.ProjectSelected);
            this.ViewTimesheetCommand = new RelayCommand(this.ViewTimesheet);
        }

        public void RefreshProjects(bool keepSelection = true)
        {
            // reload the projects
            this.applicationConfig.GetProjects();

            if (!keepSelection)
            {
                // end any current selected project
                this.ProjectSelected(null);
                return;
            }
        }

        /// <summary>
        /// Called when a new project is selected
        /// </summary>
        /// <param name="args"></param>
        private void ProjectSelected(object? args)
        {
            var e = args as Events.ProjectSelectedEventArgs;

            var utcDate = DateTimeOffset.UtcNow;

            // end the current activity (if applicable)
            this.EndCurrentActivity(utcDate);

            // start the new activity, if one has been selected
            if (e is not null && e.Project is not null && !e.IsStopped)
            {
                this.StartNewActivity(e.Project, utcDate);
            }
        }

        /// <summary>
        /// Exits the application
        /// </summary>
        private void Exit(bool invokeShutdown)
        {
            // end any current activity first
            this.EndCurrentActivity(DateTimeOffset.UtcNow);

            // then shutdown the app
            if (invokeShutdown)
            {
                this.ShutdownCalled = true;
                Application.Current.Shutdown();
            }
        }

        /// <summary>
        /// Shows the timesheet viewer
        /// </summary>
        private void ViewTimesheet()
        {
            if (this.applicationConfig.TimesheetConnector is not null)
            {
                this.applicationConfig.TimesheetConnector.ViewTimesheet(this.currentActivity);
            }
        }

        /// <summary>
        /// Starts a new activity
        /// </summary>
        /// <param name="project"></param>
        /// <param name="utcStart"></param>
        private void StartNewActivity(Project project, DateTimeOffset utcStart)
        {

            this.currentActivity = new Activity(project, utcStart);
            this.SetToolTip(project.ToString());
            this.ActiveIcon = project.Icon ?? WinFormsAssets.CLOCK_RED;
            this.IsActive = true;
        }

        /// <summary>
        /// Ends and records the current activity
        /// </summary>
        /// <param name="utcEnd"></param>
        private void EndCurrentActivity(DateTimeOffset utcEnd)
        {
            if (this.currentActivity is null)
            {
                return;
            }

            try
            {
                if (this.applicationConfig.TimesheetConnector is not null)
                {
                    this.currentActivity.End = utcEnd;
                    this.applicationConfig.TimesheetConnector.RecordActivity(this.currentActivity);
                }
            }
            catch (Exception ex)
            {
                var logFile = ExceptionLogger.LogException(ex);

                string msg = string.Format("An error occurred recording the activity.\r\n\r\nLog output is at\r\n{0}", logFile);
                MessageBox.Show(msg, AssemblyInfo.ProductName, MessageBoxButton.OK, MessageBoxImage.Error);
            }

            this.currentActivity = null;
            this.SetToolTip(null);
            this.IsActive = false;
        }

        /// <summary>
        /// Updates the tray icon tooltip
        /// </summary>
        /// <param name="text"></param>
        private void SetToolTip(string? text)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                text = "Inactive";
            }

            this.ToolTip = string.Format("{0}\r\n{1}", AssemblyInfo.ProductName, text);
        }
    }
}
