using BetterComments.CommentsTagging;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;

namespace BetterComments.Options
{
    public class Settings : ISettings, INotifyPropertyChanged
    {
        #region Fields

        private string font = string.Empty;
        private double size = -1.5;
        private double opacity = 0.8;
        private bool italic = true;
        private bool highlightTaskKeywordOnly = false;
        private bool underlineImportantComments = false;
        private bool strikethroughDoubleComments = false;

        private string tokensString;
        private Dictionary<string, string> tokenValues;

        #endregion Fields

        #region Singleton

        private static volatile Settings instance;
        private static readonly object syncLock = new object();

        public static Settings Instance
        {
            get
            {
                if (instance == null)
                {
                    lock (syncLock)
                    {
                        if (instance == null)
                            instance = new Settings();
                    }
                }

                return instance;
            }
        }

        private Settings()
        {
            tokenValues = GetTokenDefaultValues();
            tokensString = ConvertTokenValuesDictionaryToString();

            SettingsStore.LoadSettings(this);
            SettingsStore.SettingsChanged += OnSettingsChanged;
        }

        #endregion

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
        public string TokensString
        {
            get
            {
                return ConvertTokenValuesDictionaryToString();
            }
            set
            {
                tokensString = value;
                BuildTokenValuesDictionary();
            }
        }

        #endregion Settings Properties

        #region Non-Settings Properties

        public Dictionary<string, string> TokenValues
        {
            get { return tokenValues; }
            set { SetField(ref tokenValues, value); }
        }

        #endregion

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
            var dictionary = new Dictionary<string, string>();
            var keys = Enum.GetNames(typeof(CommentType)).Where(p => ((CommentType)Enum.Parse(typeof(CommentType), p)).GetAttribute<CommentIgnoreAttribute>() == null);

            foreach (var key in keys)
            {
                dictionary.Add(key, ((CommentType)Enum.Parse(typeof(CommentType), key)).GetAttribute<CommentDefaultAttribute>()?.Value);
            }

            return dictionary;
        }

        private void BuildTokenValuesDictionary()
        {
            TokenValues = tokensString.Split('|')
                                      .Select(str => str.Split(','))
                                      .ToDictionary(sa => sa[0].Trim(), sa => sa[1].Trim());
        }

        private string ConvertTokenValuesDictionaryToString()
        {
            return string.Join("|", TokenValues.Select(p => $"{p.Key},{p.Value}"));
        }

        private void OnSettingsChanged()
        {
            SettingsStore.LoadSettings(this);
        }
    }
}