using System;
using System.Diagnostics;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Settings;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;

namespace BetterComments.Options
{
   public static class SettingsStore
   {
      private static readonly WritableSettingsStore store
          = new ShellSettingsManager(ServiceProvider.GlobalProvider).GetWritableSettingsStore(SettingsScope.UserSettings);

      public static event Action SettingsChanged;

      public static void SaveSettings(ISettings settings)
      {
         try
         {
            if (!store.CollectionExists(settings.Key))
            {
               store.CreateCollection(settings.Key);
            }

            if (SaveSettingsIntoStore(settings))
            {
               SettingsChanged?.Invoke();
            }
         }
         catch (Exception ex)
         {
            Debug.Fail(ex.Message);
         }
      }

      public static void LoadSettings(ISettings settings)
      {
         try
         {
            if (store.CollectionExists(settings.Key))
            {
               LoadSettingsFromStore(settings);
            }
         }
         catch (Exception ex)
         {
            Debug.Fail(ex.Message);
         }
      }

      private static bool SaveSettingsIntoStore(ISettings settings)
      {
         foreach (var prop in GetProperties(settings))
         {
            switch (prop.GetValue(settings))
            {
               case bool value:
                  store.SetBoolean(settings.Key, prop.Name, value);
                  return true;

               case int value:
                  store.SetInt32(settings.Key, prop.Name, value);
                  return true;

               case double value:
                  store.SetString(settings.Key, prop.Name, value.ToString());
                  return true;


               case string value:
                  store.SetString(settings.Key, prop.Name, value);
                  return true;
            }
         }

         return false;
      }

      private static void LoadSettingsFromStore(ISettings settings)
      {
         foreach (var prop in GetProperties(settings))
         {
            if (store.PropertyExists(settings.Key, prop.Name))
            {
               switch (prop.GetValue(settings))
               {
                  case bool b:
                     prop.SetValue(settings, store.GetBoolean(settings.Key, prop.Name));
                     break;

                  case int i:
                     prop.SetValue(settings, store.GetInt32(settings.Key, prop.Name));
                     break;

                  case double d when double.TryParse(store.GetString(settings.Key, prop.Name), out double value):
                     prop.SetValue(settings, value);
                     break;

                  case string s:
                     prop.SetValue(settings, store.GetString(settings.Key, prop.Name));
                     break;
               }
            }
         }
      }

      private static IEnumerable<PropertyInfo> GetProperties(ISettings settings)
      {
         return settings.GetType()
                        .GetProperties()
                        .Where(p => Attribute.IsDefined(p, typeof(SettingAttribute)));
      }
   }
}