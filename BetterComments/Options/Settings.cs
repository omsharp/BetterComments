using BetterComments.CommentsTagging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace BetterComments.Options
{
    public class Settings : ISettings, INotifyPropertyChanged
    {
        private static Lazy<Settings> instance = new Lazy<Settings>(() => new Settings());

        public static Settings Instance
        {
            get { return instance.Value; }
        }

        #region Fields

        private string font = string.Empty;
        private double size = -1.5;
        private double opacity = 0.8;
        private bool italic = true;
        private bool highlightTaskKeywordOnly = false;
        private bool underlineImportantComments = false;
        private bool strikethroughDoubleComments = false;
        private Dictionary<string, string> tokenValues = GetTokenDefaultValues();

        #endregion Fields

        #region Settings Properties

        [Setting]
        public string Font
        {
            get { return font; }
            set { SetField(ref font, value); }
        }

        [Setting]
        public double Size
        {
            get { return size; }
            set { SetField(ref size, value); }
        }

        [Setting]
        public bool Italic
        {
            get { return italic; }
            set { SetField(ref italic, value); }
        }

        [Setting]
        public double Opacity
        {
            get { return opacity; }
            set { SetField(ref opacity, value); }
        }

        [Setting]
        public bool HighlightTaskKeywordOnly
        {
            get { return highlightTaskKeywordOnly; }
            set { SetField(ref highlightTaskKeywordOnly, value); }
        }

        [Setting]
        public bool UnderlineImportantComments
        {
            get { return underlineImportantComments; }
            set { SetField(ref underlineImportantComments, value); }
        }

        [Setting]
        public bool StrikethroughDoubleComments
        {
            get { return strikethroughDoubleComments; }
            set { SetField(ref strikethroughDoubleComments, value); }
        }

        [Setting]
        public Dictionary<string, string> TokenValues
        {
            get { return tokenValues; }
            set { SetField(ref tokenValues, value); }
        }

        #endregion Settings Properties

        private Settings()
        {
            SettingsStore.LoadSettings(this);
            SettingsStore.SettingsChanged += OnSettingsChanged;
        }

        #region ISettings Members

        public string Key => "BetterComments";

        #endregion ISettings Members

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return;

            field = value;

            OnPropertyChanged(propertyName);
        }

        #endregion INotifyPropertyChanged Members

        public static Dictionary<string, string> GetTokenDefaultValues()
        {
            var dictionary = new Dictionary<String, String>();
            var keys = Enum.GetNames(typeof(CommentType)).Where(p => ((CommentType)Enum.Parse(typeof(CommentType), p)).GetAttribute<CommentIgnoreAttribute>() == null);

            foreach (var key in keys)
            {
                dictionary.Add(key, ((CommentType)Enum.Parse(typeof(CommentType), key)).GetAttribute<CommentDefaultAttribute>()?.Value);
            }

            return dictionary;
        }

        private void OnSettingsChanged()
        {
            SettingsStore.LoadSettings(this);
        }
    }
}