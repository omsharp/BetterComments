using System;
using System.Collections.Generic;
using BetterComments.Options;
using Microsoft.VisualStudio.Text;

namespace BetterComments.CommentsTagging
{
   internal class JavaScriptCommentParser : CommentParser
   {
      public JavaScriptCommentParser(Settings settings)
         : base(settings)
      {
      }

      public override bool IsValidComment(SnapshotSpan span)
      {
         var txt = span.GetText();

         return (txt.StartsWith("//") || txt.StartsWith("/*"));
      }

      public override Comment Parse(SnapshotSpan span)
      {
         var spanText = GetFullCommentSpan(span).ToLower();

         var commentType = GetCommentType(spanText);

         var commentSpans = new List<SnapshotSpan>();

         if (commentType == CommentType.Normal)
         {
            commentSpans.Add(span);
         }
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
            int startOffset;
            int spanLength;

            if (spanText.StartsWith("//") || (spanText.StartsWith("/*") && spanText.EndsWith("*/"))) //! The comment span consists of a single line.
            {
               string keyword = Settings.TokenValues[commentType.ToString()];
               startOffset = spanText.IndexOf(keyword);
              
               spanLength = spanText.StartsWith("//")
                             ? spanText.Length - startOffset
                             : spanText.IndexOfFirstCharReverse(spanText.IndexOf("*/") - 1) - (startOffset - 1);

               if (spanLength > 0)
                  commentSpans.Add(new SnapshotSpan(span.Snapshot, span.Start + startOffset, spanLength));
            }
            else //! The comment spans multiple lines
            {
               var currentLine = span.Snapshot.GetLineFromPosition(span.Start);
               var lineText = currentLine.GetText(); ;
               
               //! Handle first line, ONLY if it is more than just a comment starter
               var trimmedLineText = lineText.Trim();
               if ((trimmedLineText.Length - trimmedLineText.IndexOf("/*")) > 3)
               {

                  string keyword = Settings.TokenValues[commentType.ToString()];
                  startOffset = lineText.IndexOf(keyword);
                  
                  commentSpans.Add(new SnapshotSpan(span.Snapshot, currentLine.Start + startOffset, currentLine.Length - startOffset));
               }

               //! Handle the rest of the lines
               var done = false;
               while (!done && currentLine.LineNumber < span.Snapshot.LineCount)
               {
                  currentLine = span.Snapshot.GetLineFromLineNumber(currentLine.LineNumber + 1);
                  lineText = currentLine.GetText();
                  
                  if (!lineText.Contains("*/")) //! Line in the middle
                  {
                     startOffset = lineText.IndexOfFirstChar();
                     spanLength = currentLine.Length - startOffset;
                     commentSpans.Add(new SnapshotSpan(span.Snapshot, currentLine.Start + startOffset, spanLength));
                  }
                  else //! The last line
                  {
                     //! Handle it ONLY if it is more than just a comment ender.
                     if (!lineText.Trim().StartsWith("*/"))
                     {
                        startOffset = lineText.IndexOfFirstChar();
                        spanLength = lineText.IndexOfFirstCharReverse(lineText.IndexOf("*/") - 1) - startOffset + 1;
                        commentSpans.Add(new SnapshotSpan(span.Snapshot, currentLine.Start + startOffset, spanLength));
                     }

                     done = true;
                  }
               }
            }
         }

         return new Comment(commentSpans, commentType);
      }

      private string GetFullCommentSpan(SnapshotSpan span)
      {
         var result = span.Snapshot.GetLineFromPosition(span.Start).GetText();

         if (result.StartsWith("//"))
         {
            result = result.Substring(result.IndexOf("//"));
         }
         else if (result.StartsWith("/*"))
         {
            if (result.Contains("*/"))
            {
               var start = result.IndexOf("/*");
               var length = (result.IndexOf("*/") + 2) - start;

               result = result.Substring(start, length);
            }
            else
            {
               result = result.Substring(result.IndexOf("/*"));
            }
         }

         return result;
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
