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
      private readonly IClassificationFormatMap formatMap;
      private readonly IClassificationTypeRegistryService regService;

      private readonly Settings settings = Settings.Instance;

      private static readonly List<string> commentTypes = new List<string>()
            {
                "comment",
                "xml doc comment",
                "vb xml doc comment",
                "xml comment",
                "html comment",
                "xaml comment",
            };

      public static CommentViewDecorator Create(ITextView view, IClassificationFormatMap map,
                                                IClassificationTypeRegistryService service)
      {
         return view.Properties.GetOrCreateSingletonProperty(() => new CommentViewDecorator(view, map, service));
      }

      private CommentViewDecorator(ITextView view, IClassificationFormatMap map,
                                   IClassificationTypeRegistryService service)
      {
         view.GotAggregateFocus += TextView_GotAggregateFocus;

         SettingsStore.SettingsChanged += OnSettingsChanged;

         formatMap = map;
         regService = service;

         Decorate();
      }

      private void OnSettingsChanged()
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
            Debug.Assert(false, "Exception while formatting! \n", ex.Message);
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
         var unknowns = from type in formatMap.CurrentPriorityOrder.Where(type => type != null)
                        let name = type.Classification.ToLowerInvariant()
                        where name.Contains("comment") && !commentTypes.Contains(name)
                        select type;

         foreach (var classificationType in unknowns)
            SetProperties(classificationType);
      }

      private void SetProperties(IClassificationType classificationType)
      {
         //? Might need to benchmark this function for performance.

         var properties = formatMap.GetTextProperties(classificationType);
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

         formatMap.SetTextProperties(classificationType, properties);
      }

      private double GetEditorTextSize()
      {
         return formatMap.GetTextProperties(regService.GetClassificationType("text"))
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