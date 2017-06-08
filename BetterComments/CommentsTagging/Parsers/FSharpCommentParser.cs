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
         else if (Settings.HighlightTaskKeywordOnly && commentType == CommentType.Task) //! Tag only the "Todo" keyword.
         {
            commentSpans.Add(new SnapshotSpan(span.Snapshot, span.Start + spanText.IndexOfFirstChar(2), 4));
         }
         else if (spanText.StartsWith("//"))
         {
            var startOffset = commentType == CommentType.Task ? spanText.IndexOf("todo") : spanText.IndexOfFirstChar(3);

            if (startOffset > 0)
               commentSpans.Add(new SnapshotSpan(span.Snapshot, span.Start + startOffset, span.Length - startOffset));
         }
         else if (spanText.Contains("(*") && spanText.Contains("*)"))
         {
            var startOffset = commentType == CommentType.Task ? spanText.IndexOf("todo") : spanText.IndexOfFirstChar(3);
            var spanLength = spanText.IndexOfFirstCharReverse(spanText.IndexOf("*)") - 1) - (startOffset - 1);

            commentSpans.Add(new SnapshotSpan(span.Snapshot, span.Start + startOffset, spanLength));
         }
         else //! Comment spans multiple lines
         {
            var currentLine = span.Snapshot.GetLineFromPosition(span.Start);
            var lineText = currentLine.GetText();

            var startOffset = 0;
            var spanLength = 0;

            //! Handle first line, ONLY if it is more than just a comment starter
            var trimmedLineText = lineText.Trim();
            if ((trimmedLineText.Length - trimmedLineText.IndexOf("(*")) > 3)
            {
               startOffset = commentType == CommentType.Task
                                             ? lineText.IndexOf("todo")
                                             : lineText.IndexOfFirstChar(lineText.IndexOf("(*") + 3);

               commentSpans.Add(new SnapshotSpan(span.Snapshot, currentLine.Start + startOffset, currentLine.Length - startOffset));
            }

            //! Handle the rest of the lines
            var done = false;
            while (!done)
            {
               currentLine = span.Snapshot.GetLineFromLineNumber(currentLine.LineNumber + 1);
               lineText = currentLine.GetText();

               if (!lineText.Contains("*)")) //! Line in the middle
               {
                  startOffset = lineText.IndexOfFirstChar();
                  spanLength = currentLine.Length - startOffset;
                  commentSpans.Add(new SnapshotSpan(span.Snapshot, currentLine.Start + startOffset, spanLength));
               }
               else //! The last line
               {
                  //! Handle it ONLY if it is more than just a comment ender.
                  if (!lineText.Trim().StartsWith("*)"))
                  {
                     startOffset = lineText.IndexOfFirstChar();
                     spanLength = lineText.IndexOfFirstCharReverse(lineText.IndexOf("*)") - 1) - startOffset + 1;
                     commentSpans.Add(new SnapshotSpan(span.Snapshot, currentLine.Start + startOffset, spanLength));
                  }

                  done = true;
               }
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
