using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;

namespace BetterComments.ItalicComments
{
    internal sealed class CommentsDecorator
    {
        private bool italicizing;
        private readonly IClassificationFormatMap formatMap;
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
        
        public CommentsDecorator(ITextView textView,
                                 IClassificationFormatMap formatMap,
                                 IClassificationTypeRegistryService typeRegistryService)
        {
            textView.GotAggregateFocus += TextView_GotAggregateFocus;

            this.formatMap = formatMap;
            this.typeRegistryService = typeRegistryService;

            FormatComments();
        }

        private void TextView_GotAggregateFocus(object sender, EventArgs e)
        {
            var view = sender as ITextView;
            if (view != null)
                view.GotAggregateFocus -= TextView_GotAggregateFocus;

            Debug.Assert(!italicizing, "Can't format comments while the view is getting focus!");

            FormatComments();
        }

        private void FormatComments()
        {
            try
            {
                italicizing = true;

                FormatKnownCommentTypes();
                FormatUnknowCommentsTypes();
            }
            finally
            {
                italicizing = false;
            }
        }

        private void FormatKnownCommentTypes()
        {
            var knowns = commentTypes.Select(type => typeRegistryService.GetClassificationType(type))
                                     .Where(type => type != null);

            foreach (var classificationType in knowns )
            {
                Italicize(classificationType);
            }
        }

        private void FormatUnknowCommentsTypes()
        {
            var unknowns = from type in formatMap.CurrentPriorityOrder.Where(type => type != null)
                           let name = type.Classification.ToLowerInvariant()
                           where !commentTypes.Contains(name) && name.ToLowerInvariant().Contains("comment")
                           select type;

            foreach (var classificationType in unknowns)
            {
                Italicize(classificationType);
            }
        }
        
        private void Italicize(IClassificationType type)
        {
            var properties = formatMap.GetTextProperties(type);
            var typeface = properties.Typeface;

            if (typeface.Style == FontStyles.Italic)
                return;
            
            var newTypeface = new Typeface(typeface.FontFamily, FontStyles.Italic, FontWeights.UltraLight, typeface.Stretch);
            var newProps = properties.SetTypeface(newTypeface).SetFontRenderingEmSize(properties.FontRenderingEmSize - 0.3);

            formatMap.SetTextProperties(type, newProps);
        }
    }
}