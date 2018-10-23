using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;

namespace BetterComments.CommentsViewCustomization
{
   [Export(typeof(IWpfTextViewCreationListener))]
   [ContentType("code")]
   [TextViewRole(PredefinedTextViewRoles.Document)]
   internal sealed class CommentViewCreationListener : IWpfTextViewCreationListener
   {
#pragma warning disable 0649

      [Import]
      private IClassificationFormatMapService formatMapService;

      [Import]
      private IClassificationTypeRegistryService typeRegistryService;

#pragma warning restore 0649

      public void TextViewCreated(IWpfTextView textView)
      {
         textView.Properties.GetOrCreateSingletonProperty(() => CreateDecorator(textView));
      }

      public CommentViewDecorator CreateDecorator(IWpfTextView textView)
      {
         return CommentViewDecorator.Create(textView,
                                            formatMapService.GetClassificationFormatMap(textView),
                                            typeRegistryService);
      }
   }
}