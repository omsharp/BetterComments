using System.ComponentModel.Composition;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;

namespace BetterComments.ItalicComments
{
    [Export(typeof(IWpfTextViewCreationListener))]
    [ContentType("code")]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    internal sealed class ViewCreationListener : IWpfTextViewCreationListener
    {

#pragma warning disable 0649
        [Import]
        private IClassificationFormatMapService formatMap;
        [Import]
        private IClassificationTypeRegistryService typeRegistry;
#pragma warning restore 0649

        public void TextViewCreated(IWpfTextView textView)
        {
                textView.Properties
                        .GetOrCreateSingletonProperty(() => new CommentsDecorator(textView,
                                                                formatMap.GetClassificationFormatMap(textView),
                                                                typeRegistry));
        }
    }
}
