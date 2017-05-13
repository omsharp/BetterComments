using System;
using System.Diagnostics;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Settings;
using System.Linq;

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
            if (store.CollectionExists(settings.Key) != true)
               store.CreateCollection(settings.Key);

            var anySaved = false;

            var type = settings.GetType();

            foreach (var prop in type.GetProperties().Where(p => Attribute.IsDefined(p, typeof(SettingAttribute))))
            {
               if (prop.PropertyType == typeof(bool))
               {
                  store.SetBoolean(settings.Key, prop.Name, ((bool)(prop.GetValue(settings))));
                  anySaved = true;
               }
               else if (prop.PropertyType == typeof(int))
               {
                  store.SetInt32(settings.Key, prop.Name, ((int)(prop.GetValue(settings))));
                  anySaved = true;
               }
               else if (prop.PropertyType == typeof(double))
               {
                  store.SetString(settings.Key, prop.Name, prop.GetValue(settings).ToString());
                  anySaved = true;
               }
               else if (prop.PropertyType == typeof(string))
               {
                  store.SetString(settings.Key, prop.Name, ((string)(prop.GetValue(settings))));
                  anySaved = true;
               }
            }

            if (anySaved)
               SettingsChanged?.Invoke();
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
            if (store.CollectionExists(settings.Key) != true)
               return;

            var type = settings.GetType();

            foreach (var prop in type.GetProperties().Where(p => Attribute.IsDefined(p, typeof(SettingAttribute))))
            {
               if (prop.PropertyType == typeof(bool))
               {
                  if (store.PropertyExists(settings.Key, prop.Name))
                     prop.SetValue(settings, store.GetBoolean(settings.Key, prop.Name));
               }
               else if (prop.PropertyType == typeof(int))
               {
                  if (store.PropertyExists(settings.Key, prop.Name))
                     prop.SetValue(settings, store.GetInt32(settings.Key, prop.Name));
               }
               else if (prop.PropertyType == typeof(double))
               {
                  if (store.PropertyExists(settings.Key, prop.Name))
                  {
                     double.TryParse(store.GetString(settings.Key, prop.Name), out double value);
                     prop.SetValue(settings, value);
                  }
               }
               else if (prop.PropertyType == typeof(string))
               {
                  if (store.PropertyExists(settings.Key, prop.Name))
                     prop.SetValue(settings, store.GetString(settings.Key, prop.Name));
               }
            }
         }
         catch (Exception ex)
         {
            Debug.Fail(ex.Message);
         }
      }
   }
}