using BetterComments.Options;
using Microsoft.VisualStudio.Text;
using System.Collections.Generic;
using System.Linq;

namespace BetterComments.CommentsTagging
{
    internal class JavaScriptCommentParser : CommentParser
    {
        public override bool IsValidComment(SnapshotSpan span)
        {
            var txt = span.GetText();

            return (txt.StartsWith("//") || txt.StartsWith("/*"));
        }

        public override Comment Parse(SnapshotSpan span)
        {
            // Just get enough length for GetCommentType() to work.
            var len = Settings.CommentTokens.Max(t => t.CurrentValue.Length) * 2;

            return base.Parse(new SnapshotSpan(span.Snapshot, span.Start, len));
        }

        protected override Comment SpecificParse(SnapshotSpan span, CommentType commentType)
        {
            var spanText = span.GetText().ToLower();

            if (spanText.StartsWith("//")) //! The comment span consists of a single line.
            {
                var fullSpan = ParseHelper.CompleteSingleLineCommentSpan(span, "//");

                spanText = fullSpan.GetText().ToLower();

                var startOffset = ParseHelper.ComputeSingleLineCommentStartIndex(spanText, "////", commentType);
                var spanLength = spanText.Length - startOffset;

                if (spanLength > 0)
                {
                    return new Comment(
                        new SnapshotSpan(fullSpan.Snapshot, fullSpan.Start + startOffset, spanLength),
                        commentType);
                }
            }
            else if (spanText.Contains("/*"))
            {
                var fullSpans = ParseHelper.CompleteDelimitedCommentSpan(span, "/*", "*/");

                if (fullSpans.Count == 1)
                {
                    spanText = fullSpans[0].GetText().ToLower();
                    var startOffset = ParseHelper.ComputeDelimitedCommentStartIndex(spanText, commentType);
                    var spanLength = spanText.IndexOfFirstCharReverse(spanText.IndexOf("*/") - 1) - (startOffset - 1);

                    if (spanLength > 0)
                    {
                        return new Comment(
                            new SnapshotSpan(fullSpans[0].Snapshot, fullSpans[0].Start + startOffset, spanLength),
                            commentType);
                    }
                }
            }

            return new Comment(new List<SnapshotSpan>(), commentType);
        }

        protected override CommentType GetCommentType(SnapshotSpan span)
        {
            var fullSpan = span.GetText().Contains("//")
                         ? ParseHelper.CompleteSingleLineCommentSpan(span, "//")
                         : ParseHelper.CompleteDelimitedCommentSpan(span, "/*", "*/").First();

            if (fullSpan.GetText().StartsWith("////") && Settings.StrikethroughDoubleComments)
                return CommentType.Crossed;

            return base.GetCommentType(fullSpan);
        }

        protected override string SpanTextWithoutCommentStarter(SnapshotSpan span)
        {
            return span.GetText().Substring(2);
        }
    }
}
