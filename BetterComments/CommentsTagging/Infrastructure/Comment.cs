using Microsoft.VisualStudio.Text;
using System.Collections.Generic;

namespace BetterComments.CommentsTagging
{
    internal class Comment
    {
        public CommentType Type { get; private set; }
        public IEnumerable<SnapshotSpan> Spans { get; private set; }

        public Comment(IEnumerable<SnapshotSpan> spans, CommentType type)
        {
            Spans = spans;
            Type = type;
        }

        public Comment(SnapshotSpan span, CommentType type)
        {
            Spans = new List<SnapshotSpan>(new[] { span });
            Type = type;
        }
    }
}