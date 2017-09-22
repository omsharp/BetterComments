using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.Generic;
using System;
using BetterComments.CommentsTagging;
using System.Linq;

namespace BetterComments.Options {
    public class Settings : ISettings, INotifyPropertyChanged {
        #region Fields
        private string font = string.Empty;
        private double size = -1.5;
        private double opacity = 0.8;
        private bool italic = true;
        private bool highlightTaskKeywordOnly = false;
        private bool underlineImportantComments = false;
        private bool strikethroughDoubleComments = false;
        private Dictionary<String, String> tokenValues = GetTokenDefaultValues();
        #endregion

        #region Settings Properties

        [Setting]
        public string Font {
            get { return font; }
            set { SetField(ref font, value); }
        }

        [Setting]
        public double Size {
            get { return size; }
            set { SetField(ref size, value); }
        }

        [Setting]
        public bool Italic {
            get { return italic; }
            set { SetField(ref italic, value); }
        }

        [Setting]
        public double Opacity {
            get { return opacity; }
            set { SetField(ref opacity, value); }
        }

        [Setting]
        public bool HighlightTaskKeywordOnly {
            get { return highlightTaskKeywordOnly; }
            set { SetField(ref highlightTaskKeywordOnly, value); }
        }

        [Setting]
        public bool UnderlineImportantComments {
            get { return underlineImportantComments; }
            set { SetField(ref underlineImportantComments, value); }
        }

        [Setting]
        public bool StrikethroughDoubleComments {
            get { return strikethroughDoubleComments; }
            set { SetField(ref strikethroughDoubleComments, value); }
        }

        [Setting]
        public Dictionary<String, String> TokenValues {
            get { return tokenValues; }
            set { SetField(ref tokenValues, value); }
        }

        #endregion

        #region ISettings Members

        public string Key => "BetterComments";

        #endregion

        #region INotifyPropertyChanged Members

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null) {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private void SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null) {
            if (EqualityComparer<T>.Default.Equals(field, value))
                return;

            field = value;

            OnPropertyChanged(propertyName);
        }

        #endregion

        public static Dictionary<String, String> GetTokenDefaultValues() {
            Dictionary<String, String> dictionary = new Dictionary<String, String>();

            IEnumerable<String> keys = Enum.GetNames(typeof(CommentType)).Where(p => ((CommentType)Enum.Parse(typeof(CommentType), p)).GetAttribute<CommentIgnoreAttribute>() == null);
            foreach(String key in keys) {
                dictionary.Add(key, ((CommentType)Enum.Parse(typeof(CommentType), key)).GetAttribute<CommentDefaultAttribute>()?.Value);
            }

            return dictionary;

        }

    }

}
