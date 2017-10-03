using BetterComments.Options;
using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;

namespace BetterComments.CommentsTagging
{
    public static class ParseHelper
    {
        public static int ComputeSingleLineCommentStartIndex(string comment, string doubleCommentStarter, CommentType type)
        {
            var token = Settings.Instance.GetTokenValue(type);

            if (comment.StartsWith(doubleCommentStarter) && Settings.Instance.StrikethroughDoubleComments)
            {
                return comment.IndexOfFirstChar(doubleCommentStarter.Length);
            }

            return comment.IndexOfFirstChar(comment.IndexOf(token) + token.Length);
        }

        public static int ComputeDelimitedCommentStartIndex(string comment, CommentType type)
        {
            var token = Settings.Instance.GetTokenValue(type);

            return comment.IndexOfFirstChar(comment.IndexOf(token) + token.Length);
        }
        
        public static SnapshotSpan CompleteSingleLineCommentSpan(SnapshotSpan source, string startString)
        {
            var spanText = source.GetText();

            if (!spanText.Contains(startString))
                throw new ArgumentException($"The SnapshotSpan does not contain \"{startString}\"", nameof(startString));

            var line = source.Snapshot.GetLineFromPosition(source.Start);

            var offset = line.GetText().IndexOf(startString);

            return new SnapshotSpan(source.Snapshot, line.Start + offset, line.Length - offset);
        }

        public static List<SnapshotSpan> CompleteDelimitedCommentSpan(SnapshotSpan source, string start, string end)
        {
            var spanText = source.GetText();

            if (!spanText.Contains(start))
                throw new ArgumentException($"The SnapshotSpan does not contain \"{start}\"", nameof(start));

            var commentSpans = new List<SnapshotSpan>();

            var currentLine = source.Snapshot.GetLineFromPosition(source.Start);
            var curLineText = currentLine.GetText(); ;

            //! One line
            if (curLineText.Contains(start) && curLineText.Contains(end))
            {
                var startIndex = curLineText.IndexOf(start);
                var len = curLineText.IndexOf(end) - curLineText.IndexOf(start) + end.Length;

                commentSpans.Add(new SnapshotSpan(source.Snapshot, currentLine.Start + startIndex, len));
            }

            //TODO: Handle multi-line comments.

            return commentSpans;
        }
    }
}