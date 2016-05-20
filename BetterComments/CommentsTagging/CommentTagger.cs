using System;
using System.Collections.Generic;
using System.Linq;
using BetterComments.CommentsClassification;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;

namespace BetterComments.CommentsTagging
{
    internal enum CommentType
    {
        Normal,
        Important,
        Question,
        Crossed,
        Task
    }

    internal class CommentTagger : ITagger<ClassificationTag>, IDisposable
    {
        private readonly IClassificationTypeRegistryService classificationRegistry;
        private readonly ITagAggregator<IClassificationTag> tagAggregator;

        private static readonly string[] singleLineCommentStarters = { "//", "'", "#" };
        private static readonly string[] delimitedCommentStarters = { "/*", "(*" };
        private static readonly Dictionary<string, string> delimitedCommentEnders
            = new Dictionary<string, string> { { "/*", "*/" }, { "(*", "*)" } };

        public CommentTagger(IClassificationTypeRegistryService classificationRegistry,
                             ITagAggregator<IClassificationTag> tagAggregator)
        {
            this.classificationRegistry = classificationRegistry;
            this.tagAggregator = tagAggregator;
        }

#pragma warning disable 0067
        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;
#pragma warning restore 0067

        public IEnumerable<ITagSpan<ClassificationTag>> GetTags(NormalizedSnapshotSpanCollection spans)
        {
            var snapshot = spans[0].Snapshot;

            if (!snapshot.ContentType.IsOfType("code"))
                yield break; // Content is not code. Don't bother!

            // Work through all comment tags associated with the passed spans. Ignore xml doc comments.
            foreach (var tagSpanPair in from tag in tagAggregator.GetTags(spans)
                                        let classification = tag.Tag.ClassificationType.Classification
                                        where classification.ContainsCaseIgnored("comment") && !classification.ContainsCaseIgnored("doc")
                                        select tag)
            {
                // Get all the spans associated with the current tag, mapped to our snapshot
                foreach (var span in tagSpanPair.Span.GetSpans(snapshot))
                {
                    var commentText = span.GetText();
                    var commentStarter = GetCommentStarter(commentText);
                    var trimmedComment = RemoveCommentStarter(commentText);

                    if (singleLineCommentStarters.Contains(commentStarter))
                    { //! Single-line C/C++, C#, VB.NET, Python, Javascript, or F# comment.

                        if (commentText.Length < commentStarter.Length + 3)
                            continue; // We need at least 3 characters long comment.

                        var startIndex = GetStartIndex(trimmedComment, commentStarter.Length);
                        yield return BuildTagSpan(BuildClassificationTag(GetCommentType(trimmedComment)), BuildSnapshotSpan(span, startIndex, startIndex));
                    }
                    else if (delimitedCommentStarters.Contains(commentStarter))
                    { //! A Delimited comment
                        var commentEnder = delimitedCommentEnders[commentStarter];

                        if (commentText.EndsWith(commentEnder))
                        { //! Delimited C/C++, F#, and Javascript comment (single-line only).
                          //! Delimeted C# comment (Both single and multiline).
                            var startIndex = GetStartIndex(trimmedComment, commentStarter.Length);
                            yield return BuildTagSpan(BuildClassificationTag(GetCommentType(trimmedComment)), BuildSnapshotSpan(span, startIndex, startIndex + 2));
                        }
                    }
                    else if (commentStarter.Equals("<!--"))
                    { //! Hey, look! It's a markup comment!
                        if (commentText.EndsWith("-->"))
                        {
                            var commentType = GetCommentType(trimmedComment);
                            //! Ignore normal markup comments so their foreground color won't be overridden.
                            if (commentType == CommentType.Normal)
                                yield break;

                            var startIndex = GetStartIndex(trimmedComment, commentStarter.Length);

                            yield return BuildTagSpan(BuildClassificationTag(commentType), BuildSnapshotSpan(span, startIndex, startIndex + 3));
                        }
                    }
                }
            }
        }

        private static SnapshotSpan BuildSnapshotSpan(SnapshotSpan span, int startOffset, int lengthOffset)
        {
            return new SnapshotSpan(span.Snapshot, span.Start + startOffset, span.Length - lengthOffset);
        }

        private static string GetCommentStarter(string comment)
        {
            var result = comment.StartsWithOneOf(singleLineCommentStarters);

            if (!string.IsNullOrWhiteSpace(result))
                return result;

            result = comment.StartsWithOneOf(delimitedCommentStarters);

            if (!string.IsNullOrWhiteSpace(result))
                return result;

            if (comment.StartsWith("<!--"))
                return "<!--";

            return string.Empty;
        }

        private static int GetStartIndex(string trimmedComment, int commentStarterLength)
        {
            if (trimmedComment.StartsWith("//"))
                return 4;

            if (trimmedComment.StartsWith("'") || trimmedComment.StartsWith("#"))
                return 2;

            return IsTaskComment(trimmedComment) ? commentStarterLength : commentStarterLength + 2;
        }

        private static string RemoveCommentStarter(string comment)
        {
            // comments starting with //, /*, or (*
            if (comment.StartsWith("//") || comment.StartsWithAnyOf(delimitedCommentStarters))
                return comment.Substring(2, comment.Length - 2);

            if (comment.StartsWith("'") || comment.StartsWith("#"))
                return comment.Substring(1, comment.Length - 1);

            if (comment.StartsWith("<!--"))
                return comment.Substring(4, comment.Length - 4);

            return comment;
        }
        private static CommentType GetCommentType(string trimmedComment)
        {
            //! double comment = strikethrough
            if (trimmedComment.StartsWithAnyOf(singleLineCommentStarters))
                return CommentType.Crossed;

            if (IsTaskComment(trimmedComment))
                return CommentType.Task;

            //! there must be an empty space between the token place and the rest of the comment.
            return trimmedComment[0] != ' ' && trimmedComment[1] == ' '
                 ? ConvertToCommentType(trimmedComment[0].ToString())
                 : CommentType.Normal;
        }

        private static bool IsTaskComment(string trimmedComment)
        {
            return trimmedComment.Length > 4
                   && trimmedComment.IndexOf("todo", 0, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static CommentType ConvertToCommentType(string token)
        {
            switch (token)
            {
                case "!":
                    return CommentType.Important;
                case "?":
                    return CommentType.Question;
                case "x":
                case "X":
                    return CommentType.Crossed;
                case "todo":
                    return CommentType.Task;
                default:
                    return CommentType.Normal;
            }
        }

        private static TagSpan<ClassificationTag> BuildTagSpan(ClassificationTag classificationTag, SnapshotSpan span)
        {
            return new TagSpan<ClassificationTag>(span, classificationTag);
        }

        private ClassificationTag BuildClassificationTag(CommentType commentType)
        {
            switch (commentType)
            {
                case CommentType.Important:
                    return new ClassificationTag(classificationRegistry.GetClassificationType(CommentNames.IMPORTANT_COMMENT));

                case CommentType.Crossed:
                    return new ClassificationTag(classificationRegistry.GetClassificationType(CommentNames.CROSSED_COMMENT));

                case CommentType.Question:
                    return new ClassificationTag(classificationRegistry.GetClassificationType(CommentNames.QUESTION_COMMENT));

                case CommentType.Task:
                    return new ClassificationTag(classificationRegistry.GetClassificationType(CommentNames.TASK_COMMENT));

                case CommentType.Normal:
                    return new ClassificationTag(classificationRegistry.GetClassificationType("comment"));

                default:
                    throw new ArgumentException(@"Invalid comment type!", nameof(commentType), null);
            }
        }

        public void Dispose()
        {
            tagAggregator.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
