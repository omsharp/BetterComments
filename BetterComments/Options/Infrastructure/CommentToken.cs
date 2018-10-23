using BetterComments.CommentsTagging;

namespace BetterComments.Options
{
   public class CommentToken : PropertyChangeNotifier
   {
      private CommentType type;
      private string defaultValue;
      private string currentValue;

      public CommentType Type
      {
         get { return type; }
         set { SetField(ref type, value); }
      }

      public string CurrentValue
      {
         get { return currentValue; }
         set { SetField(ref currentValue, value); }
      }

      public string DefaultValue
      {
         get { return defaultValue; }
      }

      public CommentToken(CommentType type, string defaultValue, string value)
      {
         this.type = type;
         this.defaultValue = defaultValue;
         this.currentValue = value;
      }

      public bool IsOfType(string type)
      {
         return type == null ? false : Type.ToString().Equals(type.Trim());
      }

      public override string ToString()
      {
         return $"{type},{currentValue.Trim()}";
      }
   }
}