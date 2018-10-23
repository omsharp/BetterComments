using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Tagging;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;
using System.Diagnostics;

namespace BetterComments.CommentsTagging
{
   [Export(typeof(IViewTaggerProvider))]
   [ContentType("code")]
   [TagType(typeof(ClassificationTag))]
   internal class CommentTaggerProvider : IViewTaggerProvider
   {
#pragma warning disable 0649

      [Import]
      internal IClassificationTypeRegistryService reg;

      [Import]
      internal IBufferTagAggregatorFactoryService agg;

#pragma warning restore 0649

      public ITagger<T> CreateTagger<T>(ITextView textView, ITextBuffer buffer) where T : ITag
      {
         var tagAggregator = agg.CreateTagAggregator<IClassificationTag>(buffer);

         Debug.WriteLine("ITagger<T> Created");

         return new CommentTagger(reg, tagAggregator) as ITagger<T>;

         //return textView.Properties.GetOrCreateSingletonProperty(() => new CommentTagger(reg, tagAggregator)) as ITagger<T>;
      }
   }
}