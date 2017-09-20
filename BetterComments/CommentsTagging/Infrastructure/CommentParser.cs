using BetterComments.Options;
using Microsoft.VisualStudio.Text;

namespace BetterComments.CommentsTagging {
    internal abstract class CommentParser : ICommentParser {
        protected Settings Settings;

        protected CommentParser(Settings settings) {
            Settings = settings;
        }

        protected virtual CommentType GetCommentType(string commentText) {
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

        public abstract bool IsValidComment(SnapshotSpan span);

        public abstract Comment Parse(SnapshotSpan span);
    }
}
