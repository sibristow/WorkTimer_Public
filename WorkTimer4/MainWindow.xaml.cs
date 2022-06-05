using System.Windows;
using WorkTimer4.ViewModels;

namespace WorkTimer4
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.DataContextChanged += this.MainWindow_DataContextChanged;
        }

        private void MainWindow_DataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is ISettingsWindow oldWindow)
            {
                oldWindow.SettingsApplied -= this.DataContext_SettingsApplied;
            }

            if (e.NewValue is ISettingsWindow newWindow)
            {
                newWindow.SettingsApplied += this.DataContext_SettingsApplied;
            }
        }

        private void DataContext_SettingsApplied(object? sender, System.EventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
    }
}
