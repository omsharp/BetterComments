using BetterComments.CommentsTagging;
using System.Collections.ObjectModel;
using System.Linq;

namespace BetterComments.Options
{
   public class Settings : PropertyChangeNotifier, ISettings
   {
      #region Fields

      private string font = string.Empty;
      private double size = -1.5;
      private double opacity = 0.8;
      private bool italic = true;
      private bool highlightTaskKeywordOnly = false;
      private bool underlineImportantComments = false;
      private bool strikethroughDoubleComments = false;

      private readonly ObservableCollection<CommentToken> commentTokens
          = new ObservableCollection<CommentToken>
          {
                new CommentToken(type: CommentType.Important, defaultValue: "!", value: "!"),
                new CommentToken(type: CommentType.Crossed, defaultValue: "x", value: "x"),
                new CommentToken(type: CommentType.Question, defaultValue: "?", value: "?"),
                new CommentToken(type: CommentType.Task, defaultValue: "todo", value: "todo")
          };

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
         ResetTokens = new RelayCommand(SetTokensToDefault);
         SettingsStore.LoadSettings(this);
      }

      #endregion Singleton

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
      public string CommentTokensAsString
      {
         get { return ConvertCommentTokensToString(); }
         set { UpdateCommentTokens(value); }
      }

      #endregion Settings Properties

      #region Non-Settings Properties & Commands

      public RelayCommand ResetTokens { get; }

      public ObservableCollection<CommentToken> CommentTokens
      {
         get { return commentTokens; }
      }

      #endregion Non-Settings Properties & Commands

      #region ISettings Members

      public string Key => "BetterComments";

      #endregion ISettings Members

      #region Public Methods

      public CommentToken GetToken(CommentType type)
      {
         return commentTokens.Single(t => t.Type == type);
      }

      public string GetTokenValue(CommentType type)
      {
         return GetToken(type).CurrentValue;
      }

      #endregion Public Methods

      #region Private Helpers

      private void SetTokensToDefault()
      {
         foreach (var token in commentTokens)
            token.CurrentValue = token.DefaultValue;
      }

      private void UpdateCommentTokens(string tokensString)
      {
         if (tokensString.IsNullOrWhiteSpace())
            return;

         foreach (var pair in tokensString.Split('|').Select(p => p.Split(',')))
         {
            var token = commentTokens.SingleOrDefault(t => t.IsOfType(pair[0]));

            if (token != null)
               token.CurrentValue = pair[1].Trim();
         }
      }

      private string ConvertCommentTokensToString()
      {
         var r = string.Join("|", commentTokens.Select(t => t.ToString()));
         return r;
      }

      #endregion Private Helpers
   }
}