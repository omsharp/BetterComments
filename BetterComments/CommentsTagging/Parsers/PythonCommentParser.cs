using BetterComments.Options;
using Microsoft.VisualStudio.Text;

namespace BetterComments.CommentsTagging
{
   internal class PythonCommentParser : CommentParser
   {
      public override bool IsValidComment(SnapshotSpan span)
      {
         return span.GetText().Trim().StartsWith("#");
      }

      protected override Comment SpecificParse(SnapshotSpan span, CommentType commentType)
      {
         var spanText = span.GetText().ToLower();
         var startOffset = ParseHelper.ComputeSingleLineCommentStartIndex(spanText, "##", commentType);

         return new Comment(
             new SnapshotSpan(span.Snapshot, span.Start + startOffset, span.Length - startOffset),
             commentType);
      }

      protected override CommentType GetCommentType(SnapshotSpan span)
      {
         if (span.GetText().StartsWith("##") && Settings.StrikethroughDoubleComments)
            return CommentType.Crossed;

         return base.GetCommentType(span);
      }

      protected override string SpanTextWithoutCommentStarter(SnapshotSpan span)
      {
         return span.GetText().Substring(1);
      }
   }
}