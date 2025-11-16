using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using AdonisUI.Controls;

namespace WorkTimer4
{
    /// <summary>
    /// Interaction logic for UnlockWindow.xaml
    /// </summary>
    public partial class UnlockWindow : AdonisWindow
    {
        public static readonly DependencyProperty DialogContentProperty = DependencyProperty.Register("DialogContent", typeof(object), typeof(UnlockWindow), new PropertyMetadata(null));

        public object? DialogContent
        {
            get { return GetValue(DialogContentProperty); }
            set { SetValue(DialogContentProperty, value); }
        }



        public UnlockWindow()
        {
            InitializeComponent();
        }
       
        internal static bool? Show(string? currentActivity, string? previousActivity)
        {
            var window = new UnlockWindow()
            {
                Title = AssemblyInfo.ProductName,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };

            if (string.IsNullOrEmpty(currentActivity))
            {
                window.runCurrent.Text = "(none)";
            }
            else
            {
                window.runCurrent.Text = currentActivity;
            }

            if (string.IsNullOrEmpty(previousActivity))
            {
                window.runPrev.Text = "(none)";
                window.btnSwitch.IsEnabled = false;
            }
            else
            {
                window.runPrev.Text = previousActivity;
            }

            return window.ShowDialog();
        }

        protected override void InitMinimizeButton(Button minimizeButton)
        {
            minimizeButton.Visibility = Visibility.Collapsed;
        }

        protected override void InitMaximizeRestoreButton(Button maximizeRestoreButton)
        {
            maximizeRestoreButton.Visibility = Visibility.Collapsed;
        }

        //protected override void CloseClick(object sender, RoutedEventArgs e)
        //{
        //    base.CloseClick(sender, e);
        //}

        private void Continue_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            base.CloseClick(sender, e);
        }

        private void Switch_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
            base.CloseClick(sender, e);
        }
    }
}
