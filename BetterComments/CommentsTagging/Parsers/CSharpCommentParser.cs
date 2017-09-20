using System.Collections.Generic;
using Microsoft.VisualStudio.Text;
using BetterComments.Options;
using System.Diagnostics;

namespace BetterComments.CommentsTagging
{
   internal class CSharpCommentParser : CommentParser
   {
      public CSharpCommentParser(Settings settings)
         : base(settings)
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
         else if (Settings.HighlightTaskKeywordOnly && commentType == CommentType.Task) // Color only the "Todo" keyword.
         {
            commentSpans.Add(new SnapshotSpan(span.Snapshot, span.Start + spanText.IndexOfFirstChar(2), 4));
         }
         else //! CommentType is not Normal and HighlightTaskKeywordOnly is off  == Process the whole span.
         {
            var firstLine = span.Snapshot.GetLineFromPosition(span.Start).LineNumber;
            var lastLine = span.Snapshot.GetLineFromPosition(span.End).LineNumber;

            int startOffset;
            int spanLength;

            if (firstLine == lastLine) //! The comment span consists of a single line.
            {
               string keyword = Settings.TokenValues[commentType.ToString()];
               startOffset = spanText.IndexOf(keyword);
               //startOffset = commentType == CommentType.Task ? spanText.IndexOf("todo") : spanText.IndexOfFirstChar(3);

               spanLength = spanText.StartsWith("//")
                             ? span.Length - startOffset
                             : spanText.IndexOfFirstCharReverse(spanText.IndexOf("*/") - 1) - (startOffset - 1);

               if (spanLength > 0)
                  commentSpans.Add(new SnapshotSpan(span.Snapshot, span.Start + startOffset, spanLength));
            }
            else //! The comment spans multiple lines
            {
               for (var i = firstLine; i <= lastLine; i++)
               {
                  var line = span.Snapshot.GetLineFromLineNumber(i);
                  var lineText = line.GetText().ToLower();

                  if (i == firstLine) //! First line.
                  {
                     //! Handle first line, ONLY if it is more than just a comment starter
                     var indx = lineText.IndexOf("/*");
                     if(indx >= 0 && lineText.Substring(indx).Trim().Length > 3)
                     {
                        string keyword = Settings.TokenValues[commentType.ToString()];
                        startOffset = lineText.IndexOf(keyword);

                        //startOffset = commentType == CommentType.Task
                        //                              ? lineText.IndexOf("todo")
                        //                              : lineText.IndexOfFirstChar(lineText.IndexOf("/*") + 3);

                        commentSpans.Add(new SnapshotSpan(span.Snapshot, line.Start + startOffset, line.Length - startOffset));
                     }
                  }
                  else if (i > firstLine && i < lastLine) //! Line in the middle
                  {
                     startOffset = lineText.IndexOfFirstChar();
                     commentSpans.Add(new SnapshotSpan(span.Snapshot, line.Start + startOffset, line.Length - startOffset));
                  }
                  else if (!lineText.Trim().StartsWith("*/"))//! Last line . Handle it ONLY if it is more than just a comment ender.
                  {
                     startOffset = lineText.IndexOfFirstChar();
                     spanLength = lineText.IndexOfFirstCharReverse(lineText.IndexOf("*/") - 1) - startOffset + 1;

                     commentSpans.Add(new SnapshotSpan(span.Snapshot, line.Start + startOffset, spanLength));
                  }
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
