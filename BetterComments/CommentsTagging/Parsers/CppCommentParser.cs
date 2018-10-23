using BetterComments.Options;
using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;

namespace BetterComments.CommentsTagging
{
   internal class CppCommentParser : CommentParser
   {
      public override bool IsValidComment(SnapshotSpan span)
      {
         var txt = span.GetText();

         return txt.StartsWith("//", OrdinalIgnoreCase) || txt.StartsWith("/*", OrdinalIgnoreCase);
      }

      protected override Comment SpecificParse(SnapshotSpan span, CommentType commentType)
      {
         var spanText = span.GetText().ToLower();

         var commentSpans = new List<SnapshotSpan>();

         var startOffset = ParseHelper.SingleLineCommentStartIndex(spanText, "////", commentType);

         // single line comment
         if (spanText.StartsWith("//", OrdinalIgnoreCase))
         {
            commentSpans.Add(
               new SnapshotSpan(span.Snapshot, span.Start + startOffset, span.Length - startOffset));
         }
         // delimited comment in a single line
         else if (spanText.StartsWith("/*", OrdinalIgnoreCase)
               && spanText.EndsWith("*/", OrdinalIgnoreCase)
               && spanText.Length > 5)
         {
            startOffset = ParseHelper.DelimitedCommentStartIndex(spanText, commentType);

            var indexOfStarter = spanText.IndexOf("*/", OrdinalIgnoreCase);
            var spanLength = spanText.IndexOfFirstCharReverse(indexOfStarter - 1) - (startOffset - 1);

            if (spanLength > 0)
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