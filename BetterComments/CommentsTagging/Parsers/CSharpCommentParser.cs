using BetterComments.Options;
using Microsoft.VisualStudio.Text;
using System.Collections.Generic;

namespace BetterComments.CommentsTagging
{
    internal class CSharpCommentParser : CommentParser
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

            var firstLineNumber = span.Snapshot.GetLineFromPosition(span.Start).LineNumber;
            var lastLineNumber = span.Snapshot.GetLineFromPosition(span.End).LineNumber;
            
            if (firstLineNumber == lastLineNumber) //! The comment span consists of a single line.
            {
                var startOffset = ParseHelper.ComputeSingleLineCommentStartIndex(spanText, "////", commentType);

                var spanLength = spanText.StartsWith("//")
                               ? span.Length - startOffset
                               : spanText.IndexOfFirstCharReverse(spanText.IndexOf("*/") - 1) - (startOffset - 1);

                if (spanLength > 0)
                    commentSpans.Add(new SnapshotSpan(span.Snapshot, span.Start + startOffset, spanLength));
            }
            else //! The comment spans multiple lines
            {
                var startOffset = ParseHelper.ComputeDelimitedCommentStartIndex(spanText, commentType);

                for (var curr = firstLineNumber; curr <= lastLineNumber; curr++)
                {
                    var token = Settings.TokenValues[commentType.ToString()];
                    var line = span.Snapshot.GetLineFromLineNumber(curr);
                    var lineText = line.GetText().ToLower();

                    if (curr == firstLineNumber && lineText.Length > token.Length + 2) //! First line.
                    {
                        var index = lineText.IndexOf("/*");

                        startOffset = commentType == CommentType.Task
                                    ? lineText.IndexOf(token)
                                    : lineText.IndexOfFirstChar(lineText.IndexOf(token) + token.Length);

                        commentSpans.Add(new SnapshotSpan(span.Snapshot, line.Start + startOffset, line.Length - startOffset));
                    }
                    else if (curr > firstLineNumber && curr < lastLineNumber) //! Line in the middle
                    {
                        startOffset = lineText.IndexOfFirstChar();
                        commentSpans.Add(new SnapshotSpan(span.Snapshot, line.Start + startOffset, line.Length - startOffset));
                    }
                    else if (lineText.Contains("*/") && !lineText.Trim().StartsWith("*/"))//! Last line . Handle it ONLY if it is more than just a comment ender.
                    {
                        startOffset = lineText.IndexOfFirstChar();
                        var spanLength = lineText.IndexOfFirstCharReverse(lineText.IndexOf("*/") - 1) - startOffset + 1;

                        commentSpans.Add(new SnapshotSpan(span.Snapshot, line.Start + startOffset, spanLength));
                    }
                }
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