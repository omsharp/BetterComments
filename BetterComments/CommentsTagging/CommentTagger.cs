using System;
using System.Collections.Generic;
using System.Linq;
using BetterComments.CommentsClassification;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace BetterComments.CommentsTagging
{
    internal enum CommentType
    {
        Normal,
        Important,
        Question,
        Strikeout,
        Task
    }

    internal class CommentTagger : ITagger<ClassificationTag>
    {
        private readonly IClassificationTypeRegistryService classificationRegistry;
        private readonly ITagAggregator<IClassificationTag> tagAggregator;

        private static readonly string[] nonMarkupSingleLineCommentStarters = { "//", "'", "#" };
        private static readonly string[] nonMarkupMultiLineCommentStarters = { "/*", "(*" };

        public CommentTagger(IClassificationTypeRegistryService classificationRegistry,
                             ITagAggregator<IClassificationTag> tagAggregator)
        {
            this.classificationRegistry = classificationRegistry;
            this.tagAggregator = tagAggregator;
        }

        public event EventHandler<SnapshotSpanEventArgs> TagsChanged { add { } remove { } }

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
                    var comment = span.GetText();
                    var commentStarter = comment.StartsWithOneOf(nonMarkupSingleLineCommentStarters);

                    if (!string.IsNullOrEmpty(commentStarter)) // Comment starts with "//", "'", or "#"                                                     
                    {
                        if (comment.Length < commentStarter.Length + 3)
                            continue; // We need at least 3 characters long comment.
                        
                        var commentTrim = comment.TrimStart('/', '\'', '#');
                        var startOffset = IsTaskComment(comment) ? commentStarter.Length : commentStarter.Length + 2;

                        yield return BuildTagSpan(BuildClassificationTag(GetCommentType(commentTrim)),
                                                                         BuildCommentSpan(span, startOffset));
                    }
                    else // Comment deosn't start with "//", "'", or "#". 
                    {    // It could be a markup comment, starting with "<!--" 
                         // Or a delimited comment, starting with "/*"  or "(*" 

                        if (!IsMarkup(snapshot.ContentType))
                        {//! Content is not markup. It must be a delimited comment
                            
                        }
                        else //! It's a markup comment.
                        {
                            //? multiline html comments come intact in a single span.
                            //? multiline xaml comments are broken.
                        }
                    }
                }
            }
        }

        private static SnapshotSpan BuildCommentSpan(SnapshotSpan span, int startOffset)
        {
            return new SnapshotSpan(span.Snapshot,
                                    span.Start.Position + startOffset,
                                    span.Length - startOffset);
        }
        
        private static CommentType GetCommentType(string comment)
        {
            if (IsTaskComment(comment))
                return CommentType.Task;
            
            //! if there is no space between the token place and the rest of the comment tag it as normal.
            return comment[0] != ' ' && comment[1] != ' '
                 ? CommentType.Normal
                 : ConvertToCommentType(comment[0].ToString());
        }
        
        private static bool IsTaskComment(string comment)
        {
            //! "todo" is 4 characters long.
            return comment.Length > 4
                   && comment.IndexOf("todo", 0, StringComparison.OrdinalIgnoreCase) >= 0;
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
                    return CommentType.Strikeout;
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

                case CommentType.Strikeout:
                    return new ClassificationTag(classificationRegistry.GetClassificationType(CommentNames.STRIKEOUT_COMMENT));

                case CommentType.Question:
                    return new ClassificationTag(classificationRegistry.GetClassificationType(CommentNames.QUESTION_COMMENT));

                case CommentType.Task:
                    return new ClassificationTag(classificationRegistry.GetClassificationType(CommentNames.TASK_COMMENT));

                case CommentType.Normal:
                    return new ClassificationTag(classificationRegistry.GetClassificationType("comment"));

                default:
                    throw new ArgumentException("Invalid comment type!", nameof(commentType), null);
            }
        }

        private static bool IsMarkup(IContentType contentType)
        {
            return contentType.IsOfType("html") || contentType.IsOfType("htmlx")
                   || contentType.IsOfType("XAML") || contentType.IsOfType("XML");
        }
    }
}
