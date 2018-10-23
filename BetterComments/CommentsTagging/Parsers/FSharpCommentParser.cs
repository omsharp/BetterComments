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
         return temp.StartsWith("//") || temp.StartsWith("(*");
      }

      protected override Comment SpecificParse(SnapshotSpan span, CommentType commentType)
      {
         var spanText = span.GetText().ToLower();
         var commentSpans = new List<SnapshotSpan>();
         var startOffset = ParseHelper.ComputeSingleLineCommentStartIndex(spanText, "////", commentType);

         if (spanText.StartsWith("//") && startOffset > 0)
         {
            commentSpans.Add(new SnapshotSpan(span.Snapshot, span.Start + startOffset, span.Length - startOffset));
         }
         else if (spanText.Contains("(*") && spanText.Contains("*)"))
         {
            startOffset = ParseHelper.ComputeDelimitedCommentStartIndex(spanText, commentType);

            var spanLength = spanText.IndexOfFirstCharReverse(spanText.IndexOf("*)") - 1) - (startOffset - 1);

            commentSpans.Add(new SnapshotSpan(span.Snapshot, span.Start + startOffset, spanLength));
         }

         return new Comment(commentSpans, commentType);
      }

      protected override CommentType GetCommentType(SnapshotSpan span)
      {
         if (span.GetText().StartsWith("////") && Settings.StrikethroughDoubleComments)
            return CommentType.Crossed;

         return base.GetCommentType(span);
      }

      protected override string SpanTextWithoutCommentStarter(SnapshotSpan span)
      {
         return span.GetText().Substring(2);
      }
   }
}