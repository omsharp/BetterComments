using System;
using System.ComponentModel.Composition;
using System.Diagnostics;
using Microsoft.VisualStudio.Text.Classification;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Utilities;
using Microsoft.Win32;

namespace BetterComments.BetterComments
{
    [Export(typeof(IWpfTextViewCreationListener))]
    [ContentType("code")]
    [TextViewRole(PredefinedTextViewRoles.Document)]
    internal sealed class ViewCreationListener : IWpfTextViewCreationListener
    {
       // private static readonly bool isEnabled;

        static ViewCreationListener()
        {
           // isEnabled = IsEnabled();
        }
        
#pragma warning disable 0649
        [Import]
        private IClassificationFormatMapService formatMapService;
        [Import]
        private IClassificationTypeRegistryService typeRegistry;
#pragma warning restore 0649

         public void TextViewCreated(IWpfTextView textView)
        {
            //if (isEnabled) //todo: check the registry thing
                textView.Properties.GetOrCreateSingletonProperty(() =>
                new CommentsDecorator(textView,
                formatMapService.GetClassificationFormatMap(textView),
                typeRegistry));
        }

        private static bool IsEnabled()
        {
            try
            {
                using (var subKey = Registry.CurrentUser.OpenSubKey(@"Software\BetterComments", false))
                    return Convert.ToInt32(subKey?.GetValue("EnablItalics", 1)) != 0;
            }
            catch (Exception ex)
            {
                Trace.WriteLine($"BetterComments failed to read the registry: {ex.Message}");
            }

            return false;
        }
    }
}
