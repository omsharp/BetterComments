using BetterComments.Options;
using Microsoft.VisualStudio.Text;
using System.Collections.Generic;

namespace BetterComments.CommentsTagging
{
    internal abstract class CommentParser : ICommentParser
    {
        protected Settings Settings = Settings.Instance;
        
        #region ICommentParser Members

        public Comment Parse(SnapshotSpan span)
        {
            var commentType = GetCommentType(span);

            if (commentType == CommentType.Normal)
                return new Comment(new List<SnapshotSpan> { span }, CommentType.Normal);

            // Color only the "Todo" keyword.
            if (Settings.HighlightTaskKeywordOnly && commentType == CommentType.Task)
            {
                var spanText = span.GetText().ToLower();
                var token = Settings.TokenValues[CommentType.Task.ToString()];
                var start = spanText.IndexOf(token);

                return new Comment(
                    new SnapshotSpan(span.Snapshot, span.Start + start, token.Length),
                    CommentType.Task);
            }

            return SpecificParse(span, commentType);
        }

        public abstract bool IsValidComment(SnapshotSpan span);

        #endregion

        protected virtual CommentType GetCommentType(SnapshotSpan span)
        {
            var commentText = SpanTextWithoutCommentStarter(span).ToLower();

            if (commentText.StartsWith(Settings.TokenValues[CommentType.Important.ToString()]))
                return CommentType.Important;

            if (commentText.StartsWith(Settings.TokenValues[CommentType.Question.ToString()]))
                return CommentType.Question;

            if (commentText.StartsWith(Settings.TokenValues[CommentType.Crossed.ToString()]))
                return CommentType.Crossed;

            if (commentText.Trim().StartsWith(Settings.TokenValues[CommentType.Task.ToString()]))
                return CommentType.Task;

            return CommentType.Normal;
        }

        protected abstract Comment SpecificParse(SnapshotSpan span, CommentType commentType);

        protected abstract string SpanTextWithoutCommentStarter(SnapshotSpan span);
    }
}