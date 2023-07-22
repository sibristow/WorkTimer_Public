using System;
using System.Windows;
using Microsoft.Win32;
using WorkTimer4.SettingsView;
using WorkTimer4.ViewModels;

namespace WorkTimer4
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private Connectors.ConnectorCatalogue? catalogue;
        private ApplicationConfig? config;
        private Controls.NotifyIconWrapper? notifyIcon;
        private NotifyIconWrapperViewModel? vm;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            SystemEvents.SessionEnding += this.SystemEvents_SessionEnding;

            // fix date formatting
            FrameworkElement.LanguageProperty.OverrideMetadata(typeof(FrameworkElement), new FrameworkPropertyMetadata(System.Windows.Markup.XmlLanguage.GetLanguage(System.Globalization.CultureInfo.CurrentCulture.IetfLanguageTag)));

            // load the connector plugins
            this.catalogue = this.LoadPlugins();

            // load the app configuration
            this.config = ApplicationConfig.Open();

            // create a new viewmodel for the notify icon
            this.vm = new NotifyIconWrapperViewModel(config);

            // create the notifyicon (it's a resource declared in Resources.xaml)
            this.notifyIcon = (Controls.NotifyIconWrapper)this.FindResource("TaskBarIcon");

            // hook up events and datacontext
            this.notifyIcon.DataContext = this.vm;
            this.notifyIcon.Exit += this.NotifyIcon_OnExit;
            this.notifyIcon.OpenSettings += this.NotifyIcon_OnOpenSettings;
            this.notifyIcon.ProjectSelected += this.NotifyIcon_OnProjectSelected;
            this.notifyIcon.ViewTimesheet += this.NotifyIcon_ViewTimesheet;
        }

        protected override void OnExit(ExitEventArgs e)
        {
            if (this.vm is not null && !this.vm.ShutdownCalled)
            {
                this.vm.NotifyIconExitCommand.Execute(false);
            }

            if (this.notifyIcon is not null)
            {
                this.notifyIcon.Dispose(); //the icon would clean up automatically, but this is cleaner
            }

            SystemEvents.SessionEnding -= this.SystemEvents_SessionEnding;

            base.OnExit(e);
        }

        private void SystemEvents_SessionEnding(object sender, SessionEndingEventArgs e)
        {
            // system is shutting down or user logoff
            this.NotifyIcon_OnExit(sender, e);
        }

        /// <summary>
        /// Handles clicking of the Exit menu item
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void NotifyIcon_OnExit(object? sender, EventArgs e)
        {
            if (this.vm is null)
                return;

            this.vm.NotifyIconExitCommand.Execute(true);
        }

        private void NotifyIcon_OnOpenSettings(object? sender, EventArgs e)
        {
            var settings = new SettingsWindow()
            {
                DataContext = new SettingsViewModel(this.config, this.catalogue),
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
            };

            bool? result = settings.ShowDialog();

            if (result == true && this.vm is not null)
            {
                // reload projects
                this.vm.RefreshProjects();
            }
        }

        private void NotifyIcon_OnProjectSelected(object? sender, Events.ProjectSelectedEventArgs e)
        {
            if (this.vm is null)
                return;

            this.vm.ProjectSelectedCommand.Execute(e);
        }

        private void NotifyIcon_ViewTimesheet(object? sender, EventArgs e)
        {
            if (this.vm is null)
                return;

            this.vm.ViewTimesheetCommand.Execute(e);
        }

        private Connectors.ConnectorCatalogue LoadPlugins()
        {
            var catalogue = new Connectors.ConnectorCatalogue();

            try
            {
                catalogue.Compose();
            }
            catch
            {
                // do nothing, returns empty catalogue
            }

            return catalogue;
        }
    }
}
