using BetterComments.Options;
using Microsoft.VisualStudio.Text;

namespace BetterComments.CommentsTagging
{
   /// <summary>
   /// Handles HTML and XAML comments
   /// </summary>
   internal class MarkupCommentParser : CommentParser
   {
      public override bool IsValidComment(SnapshotSpan span)
      {
         var txt = span.GetText();
         return !txt.Contains("\r\n") && txt.Contains("<!--") && txt.Contains("-->");
      }

      protected override Comment SpecificParse(SnapshotSpan span, CommentType commentType)
      {
         var spanText = span.GetText().ToLower();

         var token = Settings.Instance.GetTokenValue(commentType);

         var startOffset = commentType == CommentType.Task
                         ? spanText.IndexOf(token, 3, OrdinalIgnoreCase)
                         : spanText.IndexOfFirstChar(spanText.IndexOf(token, 3, OrdinalIgnoreCase) + token.Length);

         var closerIndex = spanText.IndexOf("-->", OrdinalIgnoreCase);
         var spanLength = spanText.IndexOfFirstCharReverse(closerIndex - 1) - (startOffset - 1);

         return new Comment(
             new SnapshotSpan(span.Snapshot, span.Start + startOffset, spanLength),
             commentType);
      }

      protected override CommentType GetCommentType(SnapshotSpan span)
      {
         return base.GetCommentType(span);
      }

      protected override string SpanTextWithoutCommentStarter(SnapshotSpan span)
      {
         return span.GetText().Substring(4);
      }
   }
}