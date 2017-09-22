using System.Collections.Generic;
using BetterComments.Options;
using Microsoft.VisualStudio.Text;

namespace BetterComments.CommentsTagging
{
   internal class FSharpCommentParser : CommentParser
   {
      public FSharpCommentParser(Settings settings)
         : base(settings)
      {
      }

      public override bool IsValidComment(SnapshotSpan span)
      {
         var temp = span.GetText();
         return temp.StartsWith("//") || temp.StartsWith("(*");
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
            commentSpans.Add(new SnapshotSpan(span.Snapshot, span.Start + spanText.IndexOfFirstChar(2), 4));
         }
         else if (spanText.StartsWith("//"))
         {
            string keyword = Settings.TokenValues[commentType.ToString()];
            var startOffset = spanText.IndexOf(keyword);
            
            if (startOffset > 0)
               commentSpans.Add(new SnapshotSpan(span.Snapshot, span.Start + startOffset, span.Length - startOffset));
         }
         else if (spanText.Contains("(*") && spanText.Contains("*)"))
         {
            string keyword = Settings.TokenValues[commentType.ToString()];
            var startOffset = spanText.IndexOf(keyword);
            
            var spanLength = spanText.IndexOfFirstCharReverse(spanText.IndexOf("*)") - 1) - (startOffset - 1);

            commentSpans.Add(new SnapshotSpan(span.Snapshot, span.Start + startOffset, spanLength));
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
