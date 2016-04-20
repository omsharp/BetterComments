using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        Crossed,
        Task
    }

    internal class CommentTagger : ITagger<ClassificationTag>
    {
        private readonly IClassificationTypeRegistryService classificationRegistry;
        private readonly ITagAggregator<IClassificationTag> tagAggregator;

        private static readonly string[] nonMarkupSingleLineCommentStarters = { "//", "'", "#" };
        private static readonly string[] nonMarkupDelimitedCommentStarters = { "/*", "(*" };
        private static readonly Dictionary<string, string> delimitedCommentEnders
            = new Dictionary<string, string> { { "/*", "*/" }, { "(*", "*)" } };

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
                    var commentStarter = GetCommentStarter(comment);

                    if (nonMarkupSingleLineCommentStarters.Contains(commentStarter))
                    { //! A single-line C/C++, C#, VB.NET, Python, or F# comment.

                        if (comment.Length < commentStarter.Length + 3)
                            continue; // We need at least 3 characters long comment.

                        var trimmedComment = RemoveCommentStarter(comment);
                        var spanStartOffset = GetStartIndex(trimmedComment, commentStarter.Length);

                        yield return BuildTagSpan(BuildClassificationTag(GetCommentType(trimmedComment)),
                                                                         CreateSpan(span, spanStartOffset));
                    }
                    else if (nonMarkupDelimitedCommentStarters.Contains(commentStarter))
                    { //! A delimited C/C++, C#, or F# comment.
                        
                        var commentTrim = RemoveCommentStarter(comment);
                        var commentEnder = delimitedCommentEnders[commentStarter];

                        if (commentTrim.EndsWith(commentEnder))
                        {
                            var spanStart = span.Start + 2;
                            var spanEnd = spanStart + commentTrim.Length - 2;
                            var newSpan = new SnapshotSpan(spanStart, spanEnd);

                            Debug.WriteLine("Perfect");
                            Debug.WriteLine($"Start [");
                            Debug.WriteLine(newSpan.GetText());
                            Debug.WriteLine($"] End");
                            Debug.WriteLine(" ");

                            yield return BuildTagSpan(BuildClassificationTag(GetCommentType(commentTrim)), newSpan);
                        }
                        else 
                        { //! C/C++ or F# comment in taking multible lines.

                            Debug.WriteLine("Strange");
                            Debug.WriteLine($"Start [");
                            Debug.WriteLine(span.GetText());
                            Debug.WriteLine($"] End");
                            Debug.WriteLine(" ");

                            var endPosition = span.End.Position;
                            var newSpan = new SnapshotSpan(span.Start, endPosition);

                            if (commentTrim.Contains(commentEnder))
                            { //! Comment needs shrinking.
                                while (true)
                                {
                                    Debug.WriteLine("Span reconstructed (Too Long)");
                                    Debug.WriteLine($"Start [");
                                    Debug.WriteLine(newSpan.GetText());
                                    Debug.WriteLine($"] End");
                                    Debug.WriteLine(" ");

                                    if(newSpan.End >= newSpan.Snapshot.Lines.Last().End)
                                        break;

                                    if (!newSpan.GetText().EndsWith(commentEnder))
                                    {
                                        endPosition--;
                                        newSpan = new SnapshotSpan(span.Start, endPosition);
                                        continue;
                                    }

                                    yield return BuildTagSpan(BuildClassificationTag(GetCommentType(commentTrim)), newSpan);
                                    break;
                                }
                            }
                            else
                            { //! Comment is not complete

                                while (true)
                                {
                                    if (newSpan.End >= newSpan.Snapshot.Lines.Last().End)
                                        break;

                                    if (!newSpan.GetText().EndsWith(commentEnder))
                                    {
                                        endPosition++;
                                        newSpan = new SnapshotSpan(span.Start, endPosition);
                                        continue;
                                    }
                                    Debug.WriteLine("Span reconstructed (Too Short)");
                                    Debug.WriteLine($"Start [");
                                    Debug.WriteLine(newSpan.GetText());
                                    Debug.WriteLine($"] End");
                                    Debug.WriteLine(" ");

                                    yield return BuildTagSpan(BuildClassificationTag(GetCommentType(commentTrim)), newSpan);
                                    break;
                                }
                            }
                        }

                    }
                    else if (commentStarter.Equals("<!--"))
                    {  //! It's a markup comment.

                        //? multiline html comments come intact in a single span.
                        //? multiline xaml comments are broken.
                    }
                }
            }
        }

        private static SnapshotSpan CreateSpan(SnapshotSpan span, int startOffset)
        {
            return new SnapshotSpan(/*span.Snapshot,*/
                                    span.Start + startOffset,
                                    span.Length - startOffset);
        }

        private static string GetCommentStarter(string comment)
        {
            var result = comment.StartsWithOneOf(nonMarkupSingleLineCommentStarters);
            if (!string.IsNullOrWhiteSpace(result))
                return result;

            result = comment.StartsWithOneOf(nonMarkupDelimitedCommentStarters);
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
            if (comment.StartsWith("//") || comment.StartsWithAnyOf(nonMarkupDelimitedCommentStarters))
                return comment.Substring(2, comment.Length - 2);
            
            if (comment.StartsWith("'") || comment.StartsWith("#"))
                return comment.Substring(1, comment.Length - 1);
            
            return comment;
        }
        private static CommentType GetCommentType(string trimmedComment)
        {
            //! double comment = strikethrough
            if (trimmedComment.StartsWithAnyOf(nonMarkupSingleLineCommentStarters))
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
            //! "todo" is 4 characters long.
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
