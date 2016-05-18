using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using BetterComments.Options;

namespace BetterComments.CommentsViewCustomization
{
    internal sealed class CommentViewDecorator
    {
        private bool isDecorating;
        private readonly double textFontSize;
        private readonly IClassificationFormatMap formatMap;
        private readonly IClassificationTypeRegistryService registryService;

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
            FontSettingsManager.SettingsSaved += SettingsSaved;

            this.formatMap = formatMap;
            this.registryService = registryService;
            textFontSize = formatMap.GetTextProperties(registryService.GetClassificationType("text")).FontRenderingEmSize;

            Decorate();
        }

        private void SettingsSaved(object sender, EventArgs eventArgs)
        {
            if (!isDecorating)
                Decorate();
        }

        private void TextView_GotAggregateFocus(object sender, EventArgs e)
        {
            var view = sender as ITextView;
            if (view != null)
                view.GotAggregateFocus -= TextView_GotAggregateFocus;

            if (!isDecorating)
                Decorate();
        }

        private void Decorate()
        {
            try
            {
                isDecorating = true;
                DecorateKnownClassificationTypes();
                DecorateUnknowClassificationTypes();
            }
            catch (Exception ex)
            {
                //TODO: Handle the exception gracefully.
                Debug.Assert(false, $"Exception while formatting! \n", ex.Message);
            }
            finally
            {
                isDecorating = false;
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
            var properties = formatMap.GetTextProperties(classificationType);

            if (!string.IsNullOrWhiteSpace(FontSettingsManager.CurrentSettings.Font))
                properties = properties.SetTypeface(new Typeface(FontSettingsManager.CurrentSettings.Font));

            properties = properties.SetFontRenderingEmSize(textFontSize + FontSettingsManager.CurrentSettings.Size)
                                   .SetItalic(FontSettingsManager.CurrentSettings.IsItalic)
                                   .SetBold(FontSettingsManager.CurrentSettings.IsBold);

            if (FontSettingsManager.CurrentSettings.Opacity >= 0.1)
                properties = properties.SetForegroundOpacity(FontSettingsManager.CurrentSettings.Opacity);

            formatMap.SetTextProperties(classificationType, properties);
        }
    }
}