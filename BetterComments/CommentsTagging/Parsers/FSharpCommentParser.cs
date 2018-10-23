using BetterComments.Options;
using Microsoft.VisualStudio.Text;
using System.Collections.Generic;

namespace BetterComments.CommentsTagging
{
   internal class FSharpCommentParser : CommentParser
   {
      public override bool IsValidComment(SnapshotSpan span)
      {
         var temp = span.GetText();
         return temp.StartsWith("//", OrdinalIgnoreCase) || temp.StartsWith("(*", OrdinalIgnoreCase);
      }

      protected override Comment SpecificParse(SnapshotSpan span, CommentType commentType)
      {
         var spanText = span.GetText().ToLower();
         var commentSpans = new List<SnapshotSpan>();
         var startOffset = ParseHelper.SingleLineCommentStartIndex(spanText, "////", commentType);

         if (spanText.StartsWith("//", OrdinalIgnoreCase) && startOffset > 0)
         {
            commentSpans.Add(new SnapshotSpan(span.Snapshot, span.Start + startOffset, span.Length - startOffset));
         }
         else if (spanText.Contains("(*") && spanText.Contains("*)"))
         {
            startOffset = ParseHelper.DelimitedCommentStartIndex(spanText, commentType);

            var closerIndex = spanText.IndexOf("*)", OrdinalIgnoreCase);
            var spanLength = spanText.IndexOfFirstCharReverse(closerIndex - 1) - (startOffset - 1);

            commentSpans.Add(new SnapshotSpan(span.Snapshot, span.Start + startOffset, spanLength));
         }

         return new Comment(commentSpans, commentType);
      }

      protected override CommentType GetCommentType(SnapshotSpan span)
      {
         if (Settings.StrikethroughDoubleComments && span.GetText().StartsWith("////", OrdinalIgnoreCase))
            return CommentType.Crossed;

         return base.GetCommentType(span);
      }

      protected override string SpanTextWithoutCommentStarter(SnapshotSpan span)
      {
         return span.GetText().Substring(2);
      }
   }
}