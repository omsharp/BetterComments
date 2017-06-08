using BetterComments.Options;
using Microsoft.VisualStudio.Text;

namespace BetterComments.CommentsTagging
{
   internal abstract class CommentParser : ICommentParser
   {
      protected Settings Settings;

      protected CommentParser(Settings settings)
      {
         Settings = settings;
      }

      protected virtual CommentType GetCommentType(string commentText)
      {
         if (commentText.StartsWith("!"))
            return CommentType.Important;

         if (commentText.StartsWith("?"))
            return CommentType.Question;

         if (commentText.StartsWith("x"))
            return CommentType.Crossed;
         
         if (commentText.Trim().StartsWith("todo"))
            return CommentType.Task;

         return CommentType.Normal;
      }

      public abstract bool IsValidComment(SnapshotSpan span);

      public abstract Comment Parse(SnapshotSpan span);
   }
}
