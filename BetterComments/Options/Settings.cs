using System.ComponentModel;
using System.Runtime.CompilerServices;
using BetterComments.Annotations;

namespace BetterComments.Options
{
   public class Settings : INotifyPropertyChanged
   {
      private string font = string.Empty;
      private double size = 0;
      private double opacity = 1;
      private bool italic = false;
      private bool highlightTaskKeywordOnly = false;
      private bool underlineImportantComments = false;

      public event PropertyChangedEventHandler PropertyChanged;
      
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

      public bool Italic
      {
         get { return italic; }
         set
         {
            if (value == italic)
               return;
            italic = value;
            OnPropertyChanged();
         }
      }

      public double Opacity
      {
         get { return opacity; }
         set
         {
            if (value.Equals(opacity)) return;
            opacity = value;
            OnPropertyChanged();
         }
      }

      public bool HighlightTaskKeywordOnly
      {
         get { return highlightTaskKeywordOnly; }
         set
         {
            if (value == highlightTaskKeywordOnly) return;
            highlightTaskKeywordOnly = value;
            OnPropertyChanged();
         }
      }

      public bool UnderlineImportantComments
      {
         get { return underlineImportantComments; }
         set
         {
            if (value == underlineImportantComments) return;
            underlineImportantComments = value;
            OnPropertyChanged();
         }
      }

      public void Copy(Settings source)
      {
         Font = source.Font;
         Size = source.Size;
         Italic = source.Italic;
         Opacity = source.Opacity;
         HighlightTaskKeywordOnly = source.HighlightTaskKeywordOnly;
         UnderlineImportantComments = source.UnderlineImportantComments;
      }

      [NotifyPropertyChangedInvocator]
      protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
      {
         PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
      }
   }

}
