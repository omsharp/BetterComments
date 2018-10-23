using Microsoft.VisualStudio.Text;

namespace BetterComments.CommentsTagging
{
   internal interface ICommentParser
   {
      Comment Parse(SnapshotSpan span);

      bool IsValidComment(SnapshotSpan span);
   }
}