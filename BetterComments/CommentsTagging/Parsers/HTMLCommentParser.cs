using System.Collections.Generic;
using System.Diagnostics;
using BetterComments.Options;
using Microsoft.VisualStudio.Text;

namespace BetterComments.CommentsTagging
{
   internal class HTMLCommentParser : CommentParser
   {
      public HTMLCommentParser(Settings settings)
         : base(settings)
      {
      }

      public override bool IsValidComment(SnapshotSpan span)
      {
         return span.GetText().StartsWith("<!--");
      }

      public override Comment Parse(SnapshotSpan span)
      {
         var spanText = span.GetText().ToLower();
         var commentType = GetCommentType(spanText);
         var commentSpans = new List<SnapshotSpan>();

         if (commentType == CommentType.Normal)
         {
            commentSpans.Add(span);
         }
         else if (Settings.HighlightTaskKeywordOnly && commentType == CommentType.Task) // Color only the "Todo" keyword.
         {
            commentSpans.Add(new SnapshotSpan(span.Snapshot, span.Start + spanText.IndexOfFirstChar(4), 4));
         }
         else if (!spanText.Contains("\r\n")) // Single line
         {
            var startOffset = commentType == CommentType.Task ? spanText.IndexOf("todo") : spanText.IndexOfFirstChar(5);
            var spanLength = spanText.IndexOfFirstCharReverse(spanText.IndexOf("-->") - 1) - (startOffset - 1);

            if (spanLength > 0)
               commentSpans.Add(new SnapshotSpan(span.Snapshot, span.Start + startOffset, spanLength));
         }


         return new Comment(commentSpans, commentType);
      }

      protected override CommentType GetCommentType(string commentText)
      {
         return base.GetCommentType(commentText.Substring(4));
      }
   }
}
