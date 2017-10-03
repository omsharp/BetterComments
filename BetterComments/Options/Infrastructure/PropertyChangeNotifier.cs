using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace BetterComments.Options
{
    public abstract class PropertyChangeNotifier : INotifyPropertyChanged
    {
        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        protected void SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return;

            field = value;

            OnPropertyChanged(propertyName);
        }

        #endregion INotifyPropertyChanged Members 
    }
}