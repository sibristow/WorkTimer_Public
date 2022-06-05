using System;

namespace WorkTimer4.ViewModels
{
    internal interface ISettingsWindow
    {
        /// <summary>
        /// Event raised when new settings are applied
        /// </summary>
        event EventHandler<EventArgs> SettingsApplied;
    }
}
