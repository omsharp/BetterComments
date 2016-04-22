using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows.Media;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using BetterComments.Options;

namespace BetterComments.CommentsViewCustomization
{
    internal sealed class CommentViewDecorator
    {
        private bool fixing;
        private readonly IClassificationFormatMap formatMap;
        private readonly IClassificationTypeRegistryService registryService;
        private readonly FontOptions fontOptions;

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
                                    IClassificationFormatMap formatMap,
                                    IClassificationTypeRegistryService registryService)
        {
            textView.GotAggregateFocus += TextView_GotAggregateFocus;

            fontOptions = new FontOptions();
            this.formatMap = formatMap;
            this.registryService = registryService;
            Decorate();
        }

        private void TextView_GotAggregateFocus(object sender, EventArgs e)
        {
            var view = sender as ITextView;
            if (view != null)
                view.GotAggregateFocus -= TextView_GotAggregateFocus;

            // TODO: Deal with this issue gracefully in release mode.
            Debug.Assert(!fixing, "Can't format comments while the view is getting focus!");

            Decorate();
        }

        private void Decorate()
        {
            fixing = true;

            try
            {
                DecorateKnownClassificationTypes();
                DecorateUnknowClassificationTypes();
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

        private void DecorateKnownClassificationTypes()
        {
            var knowns = commentTypes.Select(type => registryService.GetClassificationType(type))
                                     .Where(type => type != null);

            foreach (var classificationType in knowns)
                SetProperties(classificationType);
        }

        private void DecorateUnknowClassificationTypes()
        {
            var unknowns = from type in formatMap.CurrentPriorityOrder.Where(type => type != null)
                           let name = type.Classification.ToLowerInvariant()
                           where name.Contains("comment") && !commentTypes.Contains(name)
                           select type;

            foreach (var classificationType in unknowns)
                SetProperties(classificationType);
        }

        private void SetProperties(IClassificationType classificationType)
        {
            fontOptions.LoadSettings();

            var properties = formatMap.GetTextProperties(classificationType)
                                      .SetItalic(fontOptions.IsItalic)
                                      .SetBold(fontOptions.IsBold)
                                      .SetTypeface(new Typeface(fontOptions.Font))
                                      .SetFontHintingEmSize(fontOptions.Size);

            formatMap.SetTextProperties(classificationType, properties);
        }
    }
}