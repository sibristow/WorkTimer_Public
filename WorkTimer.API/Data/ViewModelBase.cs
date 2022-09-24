using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace WorkTimer4.API.Data
{
    public class ViewModelBase : INotifyPropertyChanged, INotifyPropertyChanging
    {
        public event PropertyChangingEventHandler? PropertyChanging;
        public event PropertyChangedEventHandler? PropertyChanged;

        internal protected ViewModelBase()
        {
        }

        public void SetProperty<T>(ref T value, T newValue, [CallerMemberName] string? propertyName = null)
        {
            this.PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));

            value = newValue;

            this.PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
