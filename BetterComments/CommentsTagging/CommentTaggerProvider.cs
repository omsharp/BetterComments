using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;

namespace BetterComments.CommentsTagging
{
    [Export(typeof(IViewTaggerProvider))]
    [ContentType("code")]
    [TagType(typeof(ClassificationTag))]
    internal class CommentTaggerProvider : IViewTaggerProvider
    {

#pragma warning disable 0649
        [Import]
        internal IClassificationTypeRegistryService ClassificationRegistry;
        [Import]
        internal IBufferTagAggregatorFactoryService Aggregator;
#pragma warning restore 0649

        public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer buffer) where T : ITag
        {
            var tagAggregator = Aggregator.CreateTagAggregator<IClassificationTag>(buffer);
            return new CommentTagger(ClassificationRegistry, tagAggregator) as ITagger<T>;
        }
    }
}
