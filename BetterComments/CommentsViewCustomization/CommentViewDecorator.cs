using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using BetterComments.CommentsClassification;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using BetterComments.Options;

namespace BetterComments.CommentsViewCustomization
{
   internal sealed class CommentViewDecorator
   {
      private bool isDecorating;
      private readonly IClassificationFormatMap map;
      private readonly IClassificationTypeRegistryService regService;

      private static readonly List<string> commentTypes = new List<string>()
            {
                "comment",
                "xml doc comment",
                "vb xml doc comment",
                "xml comment",
                "html comment",
                "xaml comment"
            };

      // TODO : Try making singleton with a static Create() method
      public CommentViewDecorator(ITextView view, IClassificationFormatMap map,
                                   IClassificationTypeRegistryService service)
      {
         view.GotAggregateFocus += TextView_GotAggregateFocus;
         SettingsManager.SettingsSaved += SettingsSaved;

         this.map = map;
         regService = service;

         Decorate();
      }

      private void SettingsSaved(object sender, EventArgs eventArgs)
      {
         if (!isDecorating)
            Decorate();
      }

      private void TextView_GotAggregateFocus(object sender, EventArgs e)
      {
         if (sender is ITextView view)
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
         var knowns = commentTypes.Select(type => regService.GetClassificationType(type))
                                  .Where(type => type != null);

         foreach (var classificationType in knowns)
            SetProperties(classificationType);
      }

      private void DecorateUnknowClassificationTypes()
      {
         var unknowns = from type in map.CurrentPriorityOrder.Where(type => type != null)
                        let name = type.Classification.ToLowerInvariant()
                        where name.Contains("comment") && !commentTypes.Contains(name)
                        select type;

         foreach (var classificationType in unknowns)
            SetProperties(classificationType);
      }

      private void SetProperties(IClassificationType classificationType)
      {
         //? Might need to benchmark this function for performance.

         var properties = map.GetTextProperties(classificationType);
         var settings = SettingsManager.CurrentSettings;
         var fontSize = GetEditorTextSize() + settings.Size;

         if (!string.IsNullOrWhiteSpace(settings.Font))
            properties = properties.SetTypeface(new Typeface(settings.Font));

         if (Math.Abs(fontSize - properties.FontRenderingEmSize) > 0)
            properties = properties.SetFontRenderingEmSize(fontSize);

         if (properties.Italic != settings.Italic)
            properties = properties.SetItalic(settings.Italic);

         if (settings.Opacity >= 0.1 && settings.Opacity <= 1)
            properties = properties.SetForegroundOpacity(settings.Opacity);

         if (classificationType.IsOfType(CommentNames.IMPORTANT_COMMENT))
            properties = properties.SetTextDecorations(GetTextDecoration(settings));

         map.SetTextProperties(classificationType, properties);
      }

      private double GetEditorTextSize()
      {
         return map.GetTextProperties(regService.GetClassificationType("text"))
                   .FontRenderingEmSize;
      }

      private TextDecorationCollection GetTextDecoration(Settings settings)
      {
         return settings.UnderlineImportantComments
                    ? new TextDecorationCollection { TextDecorations.Underline }
                    : new TextDecorationCollection();
      }
   }
}