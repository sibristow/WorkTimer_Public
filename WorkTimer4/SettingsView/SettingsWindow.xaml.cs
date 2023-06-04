using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using AdonisUI.Controls;

namespace WorkTimer4.SettingsView
{
    /// <summary>
    /// Interaction logic for Window1.xaml
    /// </summary>
    public partial class SettingsWindow : AdonisWindow
    {
        public SettingsWindow()
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
