using System.Collections.Generic;
using BetterComments.Options;
using Microsoft.VisualStudio.Text;

namespace BetterComments.CommentsTagging
{

   internal class VBCommentParser : CommentParser
   {
      public VBCommentParser(Settings settings) 
         : base(settings)
      {
      }

      public override bool IsValidComment(SnapshotSpan span)
      {
         return span.GetText().Trim().StartsWith("'");
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
            commentSpans.Add(new SnapshotSpan(span.Snapshot, span.Start + spanText.IndexOfFirstChar(1), 4));
         }
         else //! CommentType is not Normal and HighlightTaskKeywordOnly is off  == Process the whole span.
         {
            var startOffset = commentType == CommentType.Task ? spanText.IndexOf("todo") : spanText.IndexOfFirstChar(2); ;

            commentSpans.Add(new SnapshotSpan(span.Snapshot, span.Start + startOffset, span.Length - startOffset));
         }

         return new Comment(commentSpans, commentType);
      }

      protected override CommentType GetCommentType(string commentText)
      {
         var commentWithoutStarter = commentText.Substring(1);

         if (commentWithoutStarter.StartsWith("'") && Settings.StrikethroughDoubleComments)
            return CommentType.Crossed;

         return base.GetCommentType(commentWithoutStarter);
      }
   }
}
