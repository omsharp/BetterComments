using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Settings;

namespace BetterComments.Options
{
    public static class FontSettingsManager
    {
        private const string COLLECTION_PATH = "BetterComments";

        private static readonly WritableSettingsStore settingsStore
            = new ShellSettingsManager(ServiceProvider.GlobalProvider)
                .GetWritableSettingsStore(SettingsScope.UserSettings);

        public static FontSettings CurrentSettings { get; } = new FontSettings();

        public static EventHandler SettingsSaved;

        static FontSettingsManager()
        {
            LoadCurrent();
        }

        public static void SaveCurrent()
        {
            try
            {
                if (!settingsStore.CollectionExists(COLLECTION_PATH))
                    settingsStore.CreateCollection(COLLECTION_PATH);

                settingsStore.SetString(COLLECTION_PATH, nameof(FontSettings.Font), CurrentSettings.Font);
                settingsStore.SetString(COLLECTION_PATH, nameof(FontSettings.Size), CurrentSettings.Size.ToString(CultureInfo.InvariantCulture));
                settingsStore.SetBoolean(COLLECTION_PATH, nameof(FontSettings.Italic), CurrentSettings.Italic);
                settingsStore.SetString(COLLECTION_PATH, nameof(FontSettings.Opacity), CurrentSettings.Opacity.ToString(CultureInfo.InvariantCulture));
                settingsStore.SetBoolean(COLLECTION_PATH, nameof(FontSettings.HighlightKeywordsOnly), CurrentSettings.HighlightKeywordsOnly);
                settingsStore.SetBoolean(COLLECTION_PATH, nameof(FontSettings.UnderlineImportantComments), CurrentSettings.UnderlineImportantComments);

                SettingsSaved?.Invoke(null, EventArgs.Empty);
            }
            catch (Exception ex)
            {
                Debug.Fail(ex.Message);
            }
        }

        public static void LoadCurrent()
        {
            try
            {
                if (!settingsStore.CollectionExists(COLLECTION_PATH))
                    return;

                if (settingsStore.PropertyExists(COLLECTION_PATH, nameof(FontSettings.Font)))
                    CurrentSettings.Font = settingsStore.GetString(COLLECTION_PATH, nameof(FontSettings.Font));

                if (settingsStore.PropertyExists(COLLECTION_PATH, nameof(FontSettings.Size)))
                {
                    var str = settingsStore.GetString(COLLECTION_PATH, nameof(FontSettings.Size));
                    double size;
                    double.TryParse(str, out size);
                    CurrentSettings.Size = size;
                }

                if (settingsStore.PropertyExists(COLLECTION_PATH, nameof(FontSettings.Italic)))
                    CurrentSettings.Italic = settingsStore.GetBoolean(COLLECTION_PATH, nameof(FontSettings.Italic));
                
                if (settingsStore.PropertyExists(COLLECTION_PATH, nameof(FontSettings.Opacity)))
                {
                    var str = settingsStore.GetString(COLLECTION_PATH, nameof(FontSettings.Opacity));
                    double opacity;
                    double.TryParse(str, out opacity);
                    CurrentSettings.Opacity = opacity ;
                }

                if(settingsStore.PropertyExists(COLLECTION_PATH, nameof(FontSettings.HighlightKeywordsOnly)))
                    CurrentSettings.HighlightKeywordsOnly = settingsStore.GetBoolean(COLLECTION_PATH, 
                        nameof(FontSettings.HighlightKeywordsOnly));

                if (settingsStore.PropertyExists(COLLECTION_PATH, nameof(FontSettings.UnderlineImportantComments)))
                    CurrentSettings.UnderlineImportantComments = settingsStore.GetBoolean(COLLECTION_PATH, 
                        nameof(FontSettings.UnderlineImportantComments));

            }
            catch (Exception ex)
            {
                Debug.Fail(ex.Message);
            }
        }
    }
}