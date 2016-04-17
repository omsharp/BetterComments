using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;

namespace BetterComments.CommentsItalicizing
{
    internal sealed class CommentViewDecorator
    {
        private bool fixing;
        private readonly IClassificationFormatMap classificationFormatMap;
        private readonly IClassificationTypeRegistryService typeRegistryService;
         
        private static readonly List<string> commentTypes = new List<string>()
            {
                "comment",
                "xml doc comment",
                "vb xml doc comment",
                "xml comment",
                "html comment",
                "xaml comment"
            };

        public CommentViewDecorator(ITextView textView,
                                IClassificationFormatMap classificationFormatMap,
                                IClassificationTypeRegistryService typeRegistryService)
        {
            textView.GotAggregateFocus += TextView_GotAggregateFocus;

            this.classificationFormatMap = classificationFormatMap;
            this.typeRegistryService = typeRegistryService;

            FixComments();
        }

        private void TextView_GotAggregateFocus(object sender, EventArgs e)
        {
            var view = sender as ITextView;
            if (view != null)
                view.GotAggregateFocus -= TextView_GotAggregateFocus;

            // TODO: Deal with this issue gracefully in release mode.
            Debug.Assert(!fixing, "Can't format comments while the view is getting focus!");

            FixComments();
        }

        private void FixComments()
        {
            fixing = true;

            try
            {
                FixKnownClassificationTypes();
                FixUnknowClassificationTypes();
            }
            catch (Exception ex)
            {
                //TODO: Handle the exception gracefully in relaese mode.
                Debug.Assert(false, $"Exception while formatting! \n", ex.Message);
            }
            finally
            {
                fixing = false;
            }
        }

        private void FixKnownClassificationTypes()
        {
            var knowns = commentTypes.Select(type => typeRegistryService.GetClassificationType(type))
                                     .Where(type => type != null);

            foreach (var classificationType in knowns)
                Italicize(classificationType);
        }

        private void FixUnknowClassificationTypes()
        {
            var unknowns = from type in classificationFormatMap.CurrentPriorityOrder.Where(type => type != null)
                           let name = type.Classification.ToLowerInvariant()
                           where name.Contains("comment") && !commentTypes.Contains(name)
                           select type;

            foreach (var classificationType in unknowns)
                Italicize(classificationType);
        }

        private void Italicize(IClassificationType classificationType)
        {
            var properties = classificationFormatMap.GetTextProperties(classificationType);
            
            if (properties.Italic)
                return;

            classificationFormatMap.SetTextProperties(classificationType, properties.SetItalic(true));
        }
    }
}