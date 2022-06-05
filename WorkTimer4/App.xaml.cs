using System.Windows;
using WorkTimer4.ViewModels;

namespace WorkTimer4
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private Connectors.ConnectorCatalogue catalogue;
        private ApplicationConfig? config;
        private Controls.NotifyIconWrapper? notifyIcon;
        private NotifyIconWrapperViewModel? vm;

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // load the connector plugins
            this.catalogue = this.LoadPlugins();

            // load the app configuration
            this.config = ApplicationConfig.Open();
            
            // create a new viewmodel for the notify icon
            this.vm = new NotifyIconWrapperViewModel(config);

            // create the notifyicon (it's a resource declared in NotifyIconResources.xaml)
            this.notifyIcon = (Controls.NotifyIconWrapper)this.FindResource("TaskBarIcon");

            // hook up events and datacontext
            this.notifyIcon.DataContext = this.vm;
            this.notifyIcon.Exit += this.NotifyIcon_OnExit;
            this.notifyIcon.OpenSettings += this.NotifyIcon_OnOpenSettings;
            this.notifyIcon.ProjectSelected += this.NotifyIcon_OnProjectSelected;

            this.vm.RefreshProjects();
        }

        

        protected override void OnExit(ExitEventArgs e)
        {
            if (this.notifyIcon != null)
            {
                this.notifyIcon.Dispose(); //the icon would clean up automatically, but this is cleaner
            }

            base.OnExit(e);
        }


        private void NotifyIcon_OnExit(object? sender, System.EventArgs e)
        {
            if (this.vm == null)
                return;

            this.vm.NotifyIconExitCommand.Execute(null);
        }

        private void NotifyIcon_OnOpenSettings(object? sender, System.EventArgs e)
        {
            var settings = new MainWindow()
            {
                DataContext = new MainWindowViewModel(this.config, this.catalogue),
                WindowStartupLocation = WindowStartupLocation.CenterScreen,
            };

            var  result = settings.ShowDialog();

            if (result == true)
            {
                // reload projects
                this.vm.RefreshProjects();
            }
        }

        private void NotifyIcon_OnProjectSelected(object? sender, Events.ProjectSelectedEventArgs e)
        {
            if (this.vm == null)
                return;

            this.vm.ProjectSelectedCommand.Execute(e);
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
            }

            return catalogue;
        }
    }
}
