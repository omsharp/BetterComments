using System;
using System.Diagnostics;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Settings;
using System.Linq;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace BetterComments.Options {
    public static class SettingsStore {
        private static readonly WritableSettingsStore store
            = new ShellSettingsManager(ServiceProvider.GlobalProvider).GetWritableSettingsStore(SettingsScope.UserSettings);

        public static event Action SettingsChanged;

        private const String DICO_KEYVALUESEPARATOR = "===";
        private const String DICO_ITEMSEPERATOR = "&&&";

        public static void SaveSettings(ISettings settings) {
            try {
                if (!store.CollectionExists(settings.Key)) {
                    store.CreateCollection(settings.Key);
                }

                if (SaveSettingsIntoStore(settings)) {
                    SettingsChanged?.Invoke();
                }
            } catch (Exception ex) {
                Debug.Fail(ex.Message);
            }
        }

        public static void LoadSettings(ISettings settings) {
            try {
                if (store.CollectionExists(settings.Key)) {
                    LoadSettingsFromStore(settings);
                }
            } catch (Exception ex) {
                Debug.Fail(ex.Message);
            }
        }

        private static bool SaveSettingsIntoStore(ISettings settings) {
            var saved = false;

            foreach (var prop in GetProperties(settings)) {
                switch (prop.GetValue(settings)) {
                    case bool b:
                        store.SetBoolean(settings.Key, prop.Name, b);
                        saved = true;
                        break;

                    case int i:
                        store.SetInt32(settings.Key, prop.Name, i);
                        saved = true;
                        break;

                    case double d:
                        store.SetString(settings.Key, prop.Name, d.ToString());
                        saved = true;
                        break;

                    case string s:
                        store.SetString(settings.Key, prop.Name, s);
                        saved = true;
                        break;

                    case Dictionary<string, string> dico:
                        String collectionName = GetSubCollectionName(settings, prop);
                        if (!store.CollectionExists(collectionName)) {
                            store.CreateCollection(collectionName);
                        } else {
                            store.GetPropertyNames(collectionName).ToList().ForEach(p => store.DeleteProperty(collectionName, p));
                        }

                        foreach (KeyValuePair<string, string> item in dico) {
                            store.SetString(collectionName, item.Key, item.Value);
                        }
                        saved = true;
                        break;
                }
            }

            return saved;
        }

        private static void LoadSettingsFromStore(ISettings settings) {
            foreach (var prop in GetProperties(settings)) {
                if (store.PropertyExists(settings.Key, prop.Name)) {
                    switch (prop.GetValue(settings)) {
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

                        case Dictionary<String, String> dico:
                            String collectionName = GetSubCollectionName(settings, prop);
                            if (store.CollectionExists(collectionName)) {
                                Dictionary<String, String> dicoToLoad = new Dictionary<string, string>();
                                foreach (string property in store.GetPropertyNames(collectionName)) {
                                    dicoToLoad.Add(property, store.GetString(collectionName, property));
                                }
                                prop.SetValue(settings, dicoToLoad);
                            }
                            break;
                    }
                }
            }
        }

        private static String GetSubCollectionName(ISettings settings, PropertyInfo property) {
            return $"{settings.Key}.{property.Name}";
        }

        private static IEnumerable<PropertyInfo> GetProperties(ISettings settings) {
            return settings.GetType()
                           .GetProperties()
                           .Where(p => Attribute.IsDefined(p, typeof(SettingAttribute)));
        }
    }
}