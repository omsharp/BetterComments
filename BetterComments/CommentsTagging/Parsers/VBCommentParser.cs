using BetterComments.Options;
using Microsoft.VisualStudio.Text;

namespace BetterComments.CommentsTagging
{
   internal class VBCommentParser : CommentParser
   {
      public override bool IsValidComment(SnapshotSpan span)
      {
         return span.GetText().Trim().StartsWith("'", OrdinalIgnoreCase);
      }

      protected override Comment SpecificParse(SnapshotSpan span, CommentType commentType)
      {
         var spanText = span.GetText().ToLower();
         var startOffset = ParseHelper.SingleLineCommentStartIndex(spanText, "''", commentType);

         return new Comment(
             new SnapshotSpan(span.Snapshot, span.Start + startOffset, span.Length - startOffset),
             commentType);
      }

      protected override CommentType GetCommentType(SnapshotSpan span)
      {
         if (Settings.StrikethroughDoubleComments && span.GetText().StartsWith("''", OrdinalIgnoreCase))
            return CommentType.Crossed;

         return base.GetCommentType(span);
      }

      protected override string SpanTextWithoutCommentStarter(SnapshotSpan span)
      {
         return span.GetText().Substring(1);
      }
   }
}