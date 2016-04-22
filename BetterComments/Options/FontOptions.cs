using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using BetterComments.Annotations;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Settings;

namespace BetterComments.Options
{
    public class FontOptions : INotifyPropertyChanged
    {
        private const string SETTINGS_PATH = "BetterComments";

        private readonly WritableSettingsStore writableSettingsStore;

        private string font = string.Empty;
        private bool isBold;
        private bool isItalic;
        private double size;

        public event PropertyChangedEventHandler PropertyChanged;

        public FontOptions()
        {
            writableSettingsStore = new ShellSettingsManager(ServiceProvider.GlobalProvider)
                                        .GetWritableSettingsStore(SettingsScope.UserSettings);
        }

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

        public void SaveSettings()
        {
            try
            {
                if (!writableSettingsStore.CollectionExists(SETTINGS_PATH))
                    writableSettingsStore.CreateCollection(SETTINGS_PATH);

                writableSettingsStore.SetString(SETTINGS_PATH, nameof(Font), Font);
                writableSettingsStore.SetInt32(SETTINGS_PATH, nameof(Size), (int)Size);
                writableSettingsStore.SetBoolean(SETTINGS_PATH, nameof(IsItalic), IsItalic);
                writableSettingsStore.SetBoolean(SETTINGS_PATH, nameof(IsBold), IsBold);
            }
            catch (Exception ex)
            {
                Debug.Fail(ex.Message);
            }
        }

        public void LoadSettings()
        {
            try
            {
                if (!writableSettingsStore.CollectionExists(SETTINGS_PATH))
                    return;

                if (writableSettingsStore.PropertyExists(SETTINGS_PATH, nameof(Font)))
                    Font = writableSettingsStore.GetString(SETTINGS_PATH, nameof(Font));

                if (writableSettingsStore.PropertyExists(SETTINGS_PATH, nameof(Size)))
                    Size = writableSettingsStore.GetInt32(SETTINGS_PATH, nameof(Size));

                if (writableSettingsStore.PropertyExists(SETTINGS_PATH, nameof(IsItalic)))
                    IsItalic = writableSettingsStore.GetBoolean(SETTINGS_PATH, nameof(IsItalic));

                if (writableSettingsStore.PropertyExists(SETTINGS_PATH, nameof(IsBold)))
                    IsBold = writableSettingsStore.GetBoolean(SETTINGS_PATH, nameof(IsBold));
            }
            catch (Exception ex)
            {
                Debug.Fail(ex.Message);
            }
        }

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}