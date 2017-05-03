using System;
using System.Diagnostics;
using System.Globalization;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Settings;

namespace BetterComments.Options
{
   public static class SettingsManager
   {
      private const string COLLECTION_PATH = "BetterComments";

      private static readonly WritableSettingsStore settingsStore
          = new ShellSettingsManager(ServiceProvider.GlobalProvider)
              .GetWritableSettingsStore(SettingsScope.UserSettings);

      public static Settings CurrentSettings { get; } = new Settings();

      public static EventHandler SettingsSaved;

      static SettingsManager()
      {
         Load();
      }

      public static void Save(Settings settings)
      {
         try
         {
            if (!settingsStore.CollectionExists(COLLECTION_PATH))
               settingsStore.CreateCollection(COLLECTION_PATH);

            settingsStore.SetString(COLLECTION_PATH, nameof(Settings.Font), settings.Font);
            settingsStore.SetString(COLLECTION_PATH, nameof(Settings.Size), settings.Size.ToString(CultureInfo.InvariantCulture));
            settingsStore.SetBoolean(COLLECTION_PATH, nameof(Settings.Italic), settings.Italic);
            settingsStore.SetString(COLLECTION_PATH, nameof(Settings.Opacity), settings.Opacity.ToString(CultureInfo.InvariantCulture));
            settingsStore.SetBoolean(COLLECTION_PATH, nameof(Settings.HighlightTaskKeywordOnly), settings.HighlightTaskKeywordOnly);
            settingsStore.SetBoolean(COLLECTION_PATH, nameof(Settings.UnderlineImportantComments), settings.UnderlineImportantComments);
            settingsStore.SetBoolean(COLLECTION_PATH, nameof(Settings.DisableStrikethrough), settings.DisableStrikethrough);

            CurrentSettings.Copy(settings);

            SettingsSaved?.Invoke(null, EventArgs.Empty);
         }
         catch (Exception ex)
         {
            Debug.Fail(ex.Message);
         }
      }

      private static void Load()
      {
         try
         {
            if (!settingsStore.CollectionExists(COLLECTION_PATH))
               return;

            if (settingsStore.PropertyExists(COLLECTION_PATH, nameof(Settings.Font)))
               CurrentSettings.Font = settingsStore.GetString(COLLECTION_PATH, nameof(Settings.Font));

            if (settingsStore.PropertyExists(COLLECTION_PATH, nameof(Settings.Size)))
            {
               var str = settingsStore.GetString(COLLECTION_PATH, nameof(Settings.Size));
               double.TryParse(str, out double size);
               CurrentSettings.Size = size;
            }

            if (settingsStore.PropertyExists(COLLECTION_PATH, nameof(Settings.Italic)))
               CurrentSettings.Italic = settingsStore.GetBoolean(COLLECTION_PATH, nameof(Settings.Italic));

            if (settingsStore.PropertyExists(COLLECTION_PATH, nameof(Settings.Opacity)))
            {
               var str = settingsStore.GetString(COLLECTION_PATH, nameof(Settings.Opacity));
               double.TryParse(str, out double opacity);
               CurrentSettings.Opacity = opacity;
            }

            if (settingsStore.PropertyExists(COLLECTION_PATH, nameof(Settings.HighlightTaskKeywordOnly)))
               CurrentSettings.HighlightTaskKeywordOnly = settingsStore.GetBoolean(COLLECTION_PATH,
                   nameof(Settings.HighlightTaskKeywordOnly));

            if (settingsStore.PropertyExists(COLLECTION_PATH, nameof(Settings.UnderlineImportantComments)))
               CurrentSettings.UnderlineImportantComments = settingsStore.GetBoolean(COLLECTION_PATH,
                   nameof(Settings.UnderlineImportantComments));

            if (settingsStore.PropertyExists(COLLECTION_PATH, nameof(Settings.DisableStrikethrough)))
               CurrentSettings.DisableStrikethrough = settingsStore.GetBoolean(COLLECTION_PATH,
                   nameof(Settings.DisableStrikethrough));
         }
         catch (Exception ex)
         {
            Debug.Fail(ex.Message);
         }
      }
   }
}