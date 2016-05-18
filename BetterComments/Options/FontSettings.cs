using System.ComponentModel;
using System.Runtime.CompilerServices;
using BetterComments.Annotations;

namespace BetterComments.Options
{
    public class FontSettings : INotifyPropertyChanged
    {
        private string font = string.Empty;
        private bool isBold;
        private bool isItalic;
        private double size;
        private double opacity;

        public event PropertyChangedEventHandler PropertyChanged;

        public string Font
        {
            get { return font; }
            set
            {
                if (value == font)
                    return;
                font = value;
                OnPropertyChanged();
            }
        }

        public double Size
        {
            get { return size; }
            set
            {
                if (value.Equals(size))
                    return;
                size = value;
                OnPropertyChanged();
            }
        }

        public bool IsItalic
        {
            get { return isItalic; }
            set
            {
                if (value == isItalic)
                    return;
                isItalic = value;
                OnPropertyChanged();
            }
        }

        public bool IsBold
        {
            get { return isBold; }
            set
            {
                if (value == isBold)
                    return;
                isBold = value;
                OnPropertyChanged();
            }
        }

        public double Opacity
        {
            get { return opacity; }
            set
            {
                if (value.Equals(opacity)) return;
                opacity = value;
                OnPropertyChanged();
            }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

}
