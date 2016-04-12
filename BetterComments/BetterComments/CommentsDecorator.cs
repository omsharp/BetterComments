using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;

namespace BetterComments.BetterComments
{
    internal sealed class CommentsDecorator
    {
        private bool formattingComments;
        private readonly IWpfTextView textView;
        private readonly IClassificationFormatMap formatMap;
        private readonly IClassificationType textClassification;
        private readonly IClassificationTypeRegistryService typeRegistryService;

        private static readonly List<string> commentTypes = new List<string>() { "comment", "xml doc comment", "vb xml doc comment", "xml comment", "html comment", "xaml comment" };
        private static readonly List<string> docTagTypes = new List<string>() { "xml doc tag", "vb xml doc tag", "xml doc attribute" };

        public CommentsDecorator(IWpfTextView textView,
            IClassificationFormatMap formatMap,
            IClassificationTypeRegistryService typeRegistryService)
        {
            this.textView = textView;
            this.formatMap = formatMap;
            this.typeRegistryService = typeRegistryService;
            this.textClassification = this.typeRegistryService.GetClassificationType("text");

            FormatComments();

            // fires when the text editor open. Need it only for the first time.
            this.textView.GotAggregateFocus += TextView_GotAggregateFocus;

            // this.textView.Closed += TextView_Closed;
        }

        private void TextView_GotAggregateFocus(object sender, EventArgs e)
        {
            var view = sender as ITextView;
            if (view != null)
                view.GotAggregateFocus -= TextView_GotAggregateFocus;

            Debug.Assert(!formattingComments, "How can we be updating *while* the view is getting focus?");

            FormatComments();
        }

        private void FormatComments()
        {
            try
            {
                formattingComments = true;

                foreach (var type in commentTypes.Select(type => typeRegistryService.GetClassificationType(type)).Where(type => type != null))
                {
                    Italicize(type);
                    Colorize(type);
                }

                foreach (var type in formatMap.CurrentPriorityOrder.Where(type => type != null))
                {
                    var name = type.Classification.ToLowerInvariant();
                    if (!commentTypes.Contains(name) && name.Contains("comment"))
                    {
                        Italicize(type);
                        Colorize(type);
                    }
                }
            }
            finally
            {
                formattingComments = false;
            }
        }

        private void Colorize(IClassificationType type)
        {
            var properties = formatMap.GetTextProperties(type);

            properties = properties.SetForeground(Color.FromRgb(65, 100, 80));

            formatMap.SetTextProperties(type, properties);
        }

        private void Italicize(IClassificationType type)
        {
            var properties = formatMap.GetTextProperties(type);
            var typeface = properties.Typeface;

            if (typeface.Style == FontStyles.Italic)
                return;
            
            var newTypeface = new Typeface(new FontFamily("Comic Sans MS"), FontStyles.Italic, FontWeights.Light, typeface.Stretch);
            var newProps = properties.SetTypeface(newTypeface).SetFontRenderingEmSize(properties.FontRenderingEmSize + 1);

            formatMap.SetTextProperties(type, newProps);
        }
    }
}