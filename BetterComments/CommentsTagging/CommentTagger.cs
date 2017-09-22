using System;
using System.Collections.Generic;
using System.Linq;
using BetterComments.CommentsClassification;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Tagging;
using BetterComments.Options;
using Microsoft.VisualStudio.Utilities;
using System.Diagnostics;

namespace BetterComments.CommentsTagging {
    public enum CommentType {
        [CommentIgnore]
        Normal,
        [CommentDefault("!")]
        Important,
        [CommentDefault("?")]
        Question,
        [CommentDefault("x")]
        Crossed,
        [CommentDefault("todo")]
        Task
    }

    internal class CommentTagger : ITagger<ClassificationTag>, IDisposable {
        private readonly Settings settings = Settings.Instance;

        private readonly IClassificationTypeRegistryService classRegistry;
        private readonly ITagAggregator<IClassificationTag> tagAggregator;

        public CommentTagger(IClassificationTypeRegistryService reg,
                             ITagAggregator<IClassificationTag> agg) {
            classRegistry = reg;
            tagAggregator = agg;

            //SettingsStore.LoadSettings(settings);
            //SettingsStore.SettingsChanged += OnSettingsChanged;
        }

#pragma warning disable 0067
        public event EventHandler<SnapshotSpanEventArgs> TagsChanged;
#pragma warning restore 0067

        public IEnumerable<ITagSpan<ClassificationTag>> GetTags(NormalizedSnapshotSpanCollection spans) {
            var snapshot = spans[0].Snapshot;
            var results = new List<TagSpan<ClassificationTag>>();
            var parser = CreateCommentParser(snapshot.ContentType);

            if (parser == null) // Content is not supported
                return results;

            // Work through all comment tags associated with the passed spans. Ignore XML docs.
            foreach (var tagSpan in tagAggregator.GetTags(spans).Where(m => m.Tag.IsComment() && !m.Tag.IsXmlDoc())) {
                // Get all the spans associated with the current tag, mapped to our snapshot
                foreach (var span in tagSpan.Span.GetSpans(snapshot).Where(s => parser.IsValidComment(s))) {
                    try {
                        results.AddRange(CreateTagSpans(parser.Parse(span)));
                    } catch (Exception ex) {
                        //! Yeah. I'm ignoring the exception!
                        Debug.Fail($"Tagging Exception /n/n {ex.ToString()}");
                    }
                }
            }

            return results;
        }

        public IEnumerable<TagSpan<ClassificationTag>> CreateTagSpans(Comment comment) {
            return comment.Spans.Select(s => new TagSpan<ClassificationTag>(s, CreateTag(comment.Type)));
        }

        private ClassificationTag CreateTag(CommentType type) {
            switch (type) {
                case CommentType.Important:
                    return new ClassificationTag(classRegistry.GetClassificationType(CommentNames.IMPORTANT_COMMENT));

                case CommentType.Crossed:
                    return new ClassificationTag(classRegistry.GetClassificationType(CommentNames.CROSSED_COMMENT));

                case CommentType.Question:
                    return new ClassificationTag(classRegistry.GetClassificationType(CommentNames.QUESTION_COMMENT));

                case CommentType.Task:
                    return new ClassificationTag(classRegistry.GetClassificationType(CommentNames.TASK_COMMENT));

                default:
                    return new ClassificationTag(classRegistry.GetClassificationType("comment"));
            }
        }

        private ICommentParser CreateCommentParser(IContentType contentType) {
            if (contentType.IsOfType("CSharp"))
                return new CSharpCommentParser(settings);

            if (contentType.IsOfType("Basic"))
                return new VBCommentParser(settings);

            if (contentType.IsOfType("Python"))
                return new PythonCommentParser(settings);

            if (contentType.IsOfType("F#"))
                return new FSharpCommentParser(settings);

            if (contentType.IsOfType("C/C++"))
                return new CppCommentParser(settings);

            if (contentType.IsOfType("JScript") || contentType.IsOfType("TypeScript"))
                return new JavaScriptCommentParser(settings);

            if (contentType.IsOfType("RazorCSharp"))
                return new HTMLCommentParser(settings);

            var temp = contentType.TypeName.ToLower();

            if (temp.Contains("html"))
                return new HTMLCommentParser(settings);

            if (temp.Contains("xaml"))
                return new XAMLCommentParser(settings);

            return null;
        }

        //private void OnSettingsChanged() {
        //    SettingsStore.LoadSettings(settings);
        //}

        #region IDisposable

        public void Dispose() {
            //SettingsStore.SettingsChanged -= OnSettingsChanged;
            tagAggregator.Dispose();
            GC.SuppressFinalize(this);
        }

        #endregion
    }

}
