using BetterComments.Options;
using Microsoft.VisualStudio.Text;
using System.Collections.Generic;

namespace BetterComments.CommentsTagging
{
    internal class CppCommentParser : CommentParser
    {
        public override bool IsValidComment(SnapshotSpan span)
        {
            var txt = span.GetText();

            return txt.StartsWith("//") || txt.StartsWith("/*");
        }

        protected override Comment SpecificParse(SnapshotSpan span, CommentType commentType)
        {
            var spanText = span.GetText().ToLower();

            var commentSpans = new List<SnapshotSpan>();

            var startOffset = ParseHelper.ComputeSingleLineCommentStartIndex(spanText, "////", commentType);

            if (spanText.StartsWith("//"))
            {
                commentSpans.Add(new SnapshotSpan(span.Snapshot, span.Start + startOffset, span.Length - startOffset));
            }
            else if (spanText.StartsWith("/*") && spanText.EndsWith("*/") && spanText.Length > 5)
            {
                startOffset = ParseHelper.ComputeDelimitedCommentStartIndex(spanText, commentType);
                var spanLength = spanText.IndexOfFirstCharReverse(spanText.IndexOf("*/") - 1) - (startOffset - 1);

                if (spanLength > 0)
                    commentSpans.Add(new SnapshotSpan(span.Snapshot, span.Start + startOffset, spanLength));
            }

            return new Comment(commentSpans, commentType);
        }

        protected override CommentType GetCommentType(SnapshotSpan span)
        {
            if (span.GetText().StartsWith("////") && Settings.StrikethroughDoubleComments)
                return CommentType.Crossed;

            return base.GetCommentType(span);
        }

        protected override string SpanTextWithoutCommentStarter(SnapshotSpan span)
        {
            return span.GetText().Substring(2);
        }
    }
}