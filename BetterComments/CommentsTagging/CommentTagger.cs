using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Forms;
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

        //TODO: Implement IDisposable and dispose of tagAggregator

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
                                        where classification.ContainsCaseIgnored("comment") && !classification.ContainsCaseIgnored("xml doc")
                                        select tag)
            {

                // Get all the spans associated with the current tag, mapped to our snapshot
                foreach (var span in tagSpanPair.Span.GetSpans(snapshot))
                {
                    var commentText = span.GetText();
                    var commentStarter = GetCommentStarter(commentText);
                    var trimmedComment = RemoveCommentStarter(commentText);

                    if (nonMarkupSingleLineCommentStarters.Contains(commentStarter))
                    { //! A single-line C/C++, C#, VB.NET, Python, Javascript, or F# comment.

                        if (commentText.Length < commentStarter.Length + 3)
                            continue; // We need at least 3 characters long comment.

                        var spanStartOffset = GetStartIndex(trimmedComment, commentStarter.Length);
                        
                        yield return BuildTagSpan(BuildClassificationTag(GetCommentType(trimmedComment)), BuildSnapshotSpan(span, spanStartOffset));
                    }
                    else if (nonMarkupDelimitedCommentStarters.Contains(commentStarter))
                    {   //! A Delimited comment
                        var commentEnder = delimitedCommentEnders[commentStarter];
                        if (commentText.EndsWith(commentEnder))
                        { //! A Delimited C/C++, F#, and Javascript comment in a single line.
                          //! Or a C# delimeted comment. Both single and multi line C# comments are handled here.
                            yield return BuildTagSpan(BuildClassificationTag(GetCommentType(trimmedComment)), BuildSnapshotSpan(span, 2));
                        }
                        #region "Half baked... all commented"
                        //else
                        //{ //! Delimited C/C++, F#, and Javascript comment spanning multiple lines.
                        ////var spanEnd = span.End;
                        //var length = span.Length;
                        //var current = span.GetText();
                        //var newSpan = new SnapshotSpan(span.Snapshot, new Span(span.Start, length));

                        //while (true)
                        //{
                        //    if (newSpan.End >= snapshot.Lines.Last().End)
                        //        break;

                        //    if (!current.EndsWith(commentEnder))
                        //    {
                        //        length = current.Contains(commentEnder) ? length - 1 : length + 1;
                        //        newSpan = new SnapshotSpan(span.Snapshot, newSpan.Start, length);
                        //        current = newSpan.GetText();
                        //        continue;
                        //    }

                        //    newSpan = new SnapshotSpan(span.Snapshot, new Span(newSpan.Start, length));
                        //    break;
                        //}

                        ////? current = current.Substring(2, current.Length - 2);
                        //Debug.WriteLine("\nGetTag start ------------- {");
                        //Debug.WriteLine($"{GetCommentType(current)} : {{ \n {newSpan.GetText()} \n }} \n");

                        //var classification = new ClassificationTag(
                        //    classificationRegistry.GetClassificationType(CommentNames.TASK_COMMENT));

                        ////! break the fixed span into multiple lines and convert each line into its own span.
                        //var spansList = newSpan.Snapshot.Lines.Where(l => l.Start >= newSpan.Start && l.End <= newSpan.End)
                        //                       .Select(line => new SnapshotSpan(span.Snapshot, line.Start, line.Length))
                        //                       .ToList();

                        //var tagsList = spansList.Select(s => BuildTagSpan(classification, s));

                        //foreach (var tagSpan in tagsList)
                        //{
                        //    yield return tagSpan;
                        //}

                        ////foreach (var tagSpan in spansList.Select(s => BuildTagSpan(classification, s)))
                        ////{
                        ////    Debug.WriteLine($" Span : {tagSpan.Span}");
                        ////    Debug.WriteLine($"Class : {tagSpan.Tag.ClassificationType.Classification}");
                        ////    Debug.WriteLine("---");

                        ////    yield return tagSpan;
                        ////}

                        //Debug.WriteLine("}------------ GetTag end\n");
                        //}
                        #endregion
                    }
                    else if (commentStarter.Equals("<!--"))
                    {  //! Hey, look! It's a markup comment!
                        if (commentText.EndsWith("-->"))
                        {
                            yield return BuildTagSpan(BuildClassificationTag(GetCommentType(trimmedComment)), BuildSnapshotSpan(span, 4));
                        }
                    }
                }
            }
        }

        private static SnapshotSpan BuildSnapshotSpan(SnapshotSpan span, int offset)
        {
            return new SnapshotSpan(span.Snapshot, span.Start + offset, span.Length - offset);
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

            if (comment.StartsWith("<!--"))
                return comment.Substring(4, comment.Length - 4);

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
                    throw new ArgumentException(@"Invalid comment type!", nameof(commentType), null);
            }
        }
    }
}
