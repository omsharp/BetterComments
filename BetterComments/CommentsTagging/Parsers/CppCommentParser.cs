using System.Collections.Generic;
using BetterComments.Options;
using Microsoft.VisualStudio.Text;

namespace BetterComments.CommentsTagging
{
   class CppCommentParser : CommentParser
   {
      public CppCommentParser(Settings settings) : base(settings)
      {
      }

      public override bool IsValidComment(SnapshotSpan span)
      {
         var txt = span.GetText();

         return txt.StartsWith("//") || txt.StartsWith("/*");
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
         else if (Settings.HighlightTaskKeywordOnly && commentType == CommentType.Task) //! Tag only the "Todo" keyword.
         {
            commentSpans.Add(new SnapshotSpan(span.Snapshot, span.Start + spanText.IndexOfFirstChar(2), 4));
         }
         else if (spanText.StartsWith("//"))
         {
            var startOffset = commentType == CommentType.Task ? spanText.IndexOf("todo") : spanText.IndexOfFirstChar(3);

            commentSpans.Add(new SnapshotSpan(span.Snapshot, span.Start + startOffset, span.Length - startOffset));
         }
         else if (spanText.StartsWith("/*") && spanText.EndsWith("*/"))
         {
            if (spanText.Length > 5)
            {
               var startOffset = commentType == CommentType.Task ? spanText.IndexOf("todo") : spanText.IndexOfFirstChar(3);

               var spanLength = spanText.IndexOfFirstCharReverse(spanText.IndexOf("*/") - 1) - (startOffset - 1);

               if (spanLength > 0)
                  commentSpans.Add(new SnapshotSpan(span.Snapshot, span.Start + startOffset, spanLength));
            }
         }
       
         return new Comment(commentSpans, commentType);
      }

      protected override CommentType GetCommentType(string commentText)
      {
         var commentWithoutStarter = commentText.Substring(2);

         if (commentWithoutStarter.StartsWith("//") && Settings.StrikethroughDoubleComments)
            return CommentType.Crossed;

         return base.GetCommentType(commentWithoutStarter);
      }
   }
}
