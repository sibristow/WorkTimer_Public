using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using System.Windows.Forms;
using WorkTimer4.API.Data;
using WorkTimer4.Events;
using WorkTimer4.ViewModels;

namespace WorkTimer4.Controls
{
    /// <summary>
    /// https://github.com/fujieda/SystemTrayApp.WPF/blob/main/SystemTrayApp.WPF/NotifyIconWrapper.cs
    /// </summary>
    public class NotifyIconWrapper : FrameworkElement, IDisposable
    {
        /// <summary>
        /// Tag to identify a fixed (non-project) menu item
        /// </summary>
        private const string FIXED_MENU_ITEM = "fixed";


        public static readonly DependencyProperty IconSourceProperty =
            DependencyProperty.Register("IconSource", typeof(string), typeof(NotifyIconWrapper), new PropertyMetadata(null, IconSource_Changed));

        public static readonly DependencyProperty ToolTipTextProperty =
            DependencyProperty.Register("ToolTipText", typeof(string), typeof(NotifyIconWrapper), new PropertyMetadata(null, ToolTipText_Changed));

        public static readonly DependencyProperty ProjectsProperty =
           DependencyProperty.Register("Projects", typeof(ObservableCollection<ProjectGroup>), typeof(NotifyIconWrapper), new PropertyMetadata(null, Projects_Changed));


        private static void IconSource_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var notifyIcon = ((NotifyIconWrapper)d)._notifyIcon;
            if (notifyIcon == null)
                return;

            notifyIcon.Icon = Assets.WinFormsAssets.GetResourceIcon(e.NewValue?.ToString());
        }

        private static void Projects_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var wrapper = d as NotifyIconWrapper;
            if (wrapper == null)
                return;

            var notifyIcon = wrapper._notifyIcon;
            if (notifyIcon == null)
                return;


            if (e.OldValue is INotifyCollectionChanged old)
            {
                old.CollectionChanged -= wrapper.Projects_CollectionChanged;
            }

            if (e.NewValue is INotifyCollectionChanged newcc)
            {
                newcc.CollectionChanged += wrapper.Projects_CollectionChanged;
                wrapper.Projects_CollectionChanged(e.NewValue, new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
            }

        }

        private static void ToolTipText_Changed(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var notifyIcon = ((NotifyIconWrapper)d)._notifyIcon;
            if (notifyIcon == null)
                return;

            notifyIcon.Text = (string)e.NewValue;
        }




        /// <summary>
        /// The WinForms tray icon
        /// </summary>
        private readonly NotifyIcon? _notifyIcon;

        /// <summary>
        /// Event raised when the Settings menu item is clicked
        /// </summary>
        public event EventHandler<EventArgs> OpenSettings;

        /// <summary>
        /// Event raised when the Exit menu item is clicked
        /// </summary>
        public event EventHandler<EventArgs> Exit;

        /// <summary>
        /// Event raised when a Project menu item is clicked
        /// </summary>
        public event EventHandler<ProjectSelectedEventArgs> ProjectSelected;

        /// <summary>
        /// Event raised when the View Timesheet menu item is clicked
        /// </summary>
        public event EventHandler<EventArgs> ViewTimesheet;


        /// <summary>
        /// Gets or sets the tray icon source
        /// </summary>
        public string IconSource
        {
            get { return (string)GetValue(IconSourceProperty); }
            set { SetValue(IconSourceProperty, value); }
        }

        /// <summary>
        /// Gets or sets the tray icon tooltip text
        /// </summary>
        public string ToolTipText
        {
            get { return (string)GetValue(ToolTipTextProperty); }
            set { SetValue(ToolTipTextProperty, value); }
        }

        /// <summary>
        /// Gets or sets the projects
        /// </summary>
        internal ObservableCollection<ProjectGroup> Projects
        {
            get { return (ObservableCollection<ProjectGroup>)GetValue(ProjectsProperty); }
            set { SetValue(ProjectsProperty, value); }
        }



        public NotifyIconWrapper()
        {
            if (DesignerProperties.GetIsInDesignMode(this))
                return;

            this._notifyIcon = new NotifyIcon
            {
                Icon = Assets.WinFormsAssets.GetApplicationIcon(),
                Visible = true,
                ContextMenuStrip = this.CreateDefaultMenu()
            };

            this._notifyIcon.DoubleClick += OpenSettingsOnClick;           
        }        

        public void Dispose()
        {
            this._notifyIcon?.Dispose();
        }

        /// <summary>
        /// Refreshes the menu items when the projects collection changes
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Projects_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
        {
            this.ClearContextMenuProjects();

            var collection = sender as IList<ProjectGroup>;
            if (collection != null)
            {
                this.CreateContextMenuProjects(collection);
            }
        }

        /// <summary>
        /// Creates the default context menu and items
        /// </summary>
        /// <returns></returns>
        private ContextMenuStrip CreateDefaultMenu()
        {
            var contextMenu = new ContextMenuStrip();

            var exitItem = new ToolStripMenuItem("Exit")
            {
                Tag = FIXED_MENU_ITEM,
                Image = Assets.WinFormsAssets.GetResourceImage("./Assets/cross.png")
            };
            exitItem.Click += this.ExitItemOnClick;
            contextMenu.Items.Add(exitItem);

            
            var openItem = new ToolStripMenuItem("Settings")
            {
                Tag = FIXED_MENU_ITEM,
                Image = Assets.WinFormsAssets.GetResourceImage("./Assets/wrench.png")
            };
            openItem.Click += this.OpenSettingsOnClick;
            contextMenu.Items.Add(openItem);


            contextMenu.Items.Add(new ToolStripSeparator());


            var pauseItem = new ToolStripMenuItem("Stop Activity")
            {
                Tag = FIXED_MENU_ITEM,
                Image = Assets.WinFormsAssets.GetResourceImage("./Assets/control_pause.png"),
                Enabled = false
            };
            pauseItem.Click += this.ProjectItemOnClick;
            contextMenu.Items.Add(pauseItem);


            contextMenu.Items.Add(new ToolStripSeparator());


            var timesheetItem = new ToolStripMenuItem("View Timesheet")
            {
                Tag = FIXED_MENU_ITEM,
                Image = Assets.WinFormsAssets.GetResourceImage("./Assets/time.png")
            };
            timesheetItem.Click += this.ViewTimesheetOnClick;
            contextMenu.Items.Add(timesheetItem);


            return contextMenu;
        }


        private void CreateContextMenuProjects(IList<ProjectGroup> groups)
        {
            if (groups == null)
            {
                return;
            }

            // make sure on UI thread
            App.Current.Dispatcher.Invoke(() =>
            {
                var menustrip = this._notifyIcon?.ContextMenuStrip;

                if (menustrip == null)
                {
                    menustrip = this.CreateDefaultMenu();
                }

                menustrip.Items.Add(new ToolStripSeparator());

                foreach (ProjectGroup group in groups)
                {
                    if (string.IsNullOrWhiteSpace(group.Name) || group.Name.Equals(Project.UNGROUPED))
                    {
                        var ungrouped = this.AddUngroupedProjects(group);
                        menustrip.Items.AddRange(ungrouped);
                        continue;
                    }

                    var groupMenu = this.AddGroupProjects(group);
                    menustrip.Items.Add(groupMenu);
                }
            });
        }

        private ToolStripMenuItem AddGroupProjects(ProjectGroup group)
        {
            var groupItem = new ToolStripMenuItem(group.Name);

            if (group.Projects == null)
            {
                return groupItem;
            }

            foreach (Project project in this.SortProjects(group))
            {
                var projectItem = this.CreateProjectMenuItem(project);
                groupItem.DropDownItems.Add(projectItem);
            }

            return groupItem;
        }


        private ToolStripMenuItem[] AddUngroupedProjects(ProjectGroup group)
        {
            if (group.Projects == null)
            {
                return new ToolStripMenuItem[] { };
            }

            var ungroupedItems = new List<ToolStripMenuItem>();

            foreach (Project project in this.SortProjects(group))
            {
                var projectItem = this.CreateProjectMenuItem(project);
                ungroupedItems.Add(projectItem);
            }

            return ungroupedItems.ToArray();
        }

        private void ClearContextMenuProjects()
        {
            // make sure on UI thread
            App.Current.Dispatcher.Invoke(() =>
            {
                var menustrip = this._notifyIcon?.ContextMenuStrip;
                if (menustrip == null)
                    return;

                for (var i = menustrip.Items.Count; i > 0; i--)
                {
                    var item = menustrip.Items[i - 1];

                    if (string.Equals(FIXED_MENU_ITEM, item.Tag?.ToString()))
                    {
                        continue;
                    }

                    menustrip.Items.RemoveAt(i - 1);
                }
            });
        }


        private DoubleHeightMenuItem CreateProjectMenuItem(Project project)
        {
            var projectItem = new DoubleHeightMenuItem(project);
            projectItem.Click += this.ProjectItemOnClick;

            return projectItem;
        }


        private IEnumerable<Project> SortProjects(ProjectGroup group)
        {
            return group.Projects.OrderBy(p => p.Name).Where(p => p.Active);
        }

        /// <summary>
        /// Handle the <see cref="ToolStripItem.Click"/> event of a Project menu item and route to the <see cref="OpenSettings"/> event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void OpenSettingsOnClick(object? sender, EventArgs eventArgs)
        {
            this.OpenSettings?.Invoke(sender, eventArgs);
        }

        /// <summary>
        /// Handle the <see cref="ToolStripItem.Click"/> event of a Project menu item and route to the <see cref="Exit"/> event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void ExitItemOnClick(object? sender, EventArgs eventArgs)
        {
            this.Exit?.Invoke(sender, eventArgs);
        }

        /// <summary>
        /// Handle the <see cref="ToolStripItem.Click"/> event of a Project menu item and route to the <see cref="ProjectSelected"/> event
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="eventArgs"></param>
        private void ProjectItemOnClick(object? sender, EventArgs eventArgs)
        {           
            var menuItem = sender as DoubleHeightMenuItem;

            this._notifyIcon.ContextMenuStrip.Items[2].Enabled = menuItem != null;

            var projectArgs = new ProjectSelectedEventArgs(menuItem?.Project);
            this.ProjectSelected?.Invoke(sender, projectArgs);
        }

        private void ViewTimesheetOnClick(object? sender, EventArgs eventArgs)
        {
            this.ViewTimesheet?.Invoke(sender, eventArgs);
        }
    }
}
