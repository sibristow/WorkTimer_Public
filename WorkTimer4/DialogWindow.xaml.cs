using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using AdonisUI.Controls;

namespace WorkTimer4
{
    /// <summary>
    /// Interaction logic for DialogWindow.xaml
    /// </summary>
    public partial class DialogWindow : AdonisWindow
    {
        public static readonly DependencyProperty DialogContentProperty = DependencyProperty.Register("DialogContent", typeof(object), typeof(DialogWindow), new PropertyMetadata(null));

        public object? DialogContent
        {
            get { return GetValue(DialogContentProperty); }
            set { SetValue(DialogContentProperty, value); }
        }


        public DialogWindow()
        {
            InitializeComponent();
        }


        internal static bool? Show(string? caption, string? content)
        {
            var window = new DialogWindow()
            {
                Title = caption,
                DialogContent = content,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };

            return window.ShowDialog();
        }

        internal static bool? Show(string? caption, params string[] content)
        {
            var window = new DialogWindow()
            {
                Title = caption,
                WindowStartupLocation = WindowStartupLocation.CenterScreen
            };

            var textblock = new TextBlock();

            for(var i=0; i<content.Length; i++)
            {
                if (content[i] is null)
                    continue;

                if (content[i] == Environment.NewLine)
                {
                    textblock.Inlines.Add(new LineBreak());
                    continue;
                }

                if (content[i][0] == '\b')
                {
                    var boldRun = new Run(content[i].Substring(1));
                    boldRun.FontWeight = FontWeights.SemiBold;
                    textblock.Inlines.Add(boldRun);
                    continue;
                }

                var run = new Run(content[i]);
                textblock.Inlines.Add(run);
            }

            window.DialogContent = textblock;

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

        protected override void CloseClick(object sender, RoutedEventArgs e)
        {
            base.CloseClick(sender, e);
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = true;
            this.Close();
        }
    }
}
