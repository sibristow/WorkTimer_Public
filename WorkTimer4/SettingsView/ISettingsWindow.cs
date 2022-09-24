using System;

namespace WorkTimer4.SettingsView
{
    internal interface ISettingsWindow
    {
        /// <summary>
        /// Event raised when new settings are applied
        /// </summary>
        event EventHandler<EventArgs> SettingsApplied;
    }
}
