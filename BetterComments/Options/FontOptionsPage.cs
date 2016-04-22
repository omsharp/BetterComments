using System;
using System.ComponentModel;
using System.ComponentModel.Composition;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;
using Microsoft.VisualStudio.Settings;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Settings;

namespace BetterComments.Options
{
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    [Guid("68d5d312-2753-4ce0-b900-2eb1ef8101c2")]
    public class FontOptionsPage : UIElementDialogPage
    {
        private OptionsPageControl optionsPageControl;

        protected override UIElement Child =>
            optionsPageControl ?? (optionsPageControl = new OptionsPageControl());

        protected override void OnActivate(CancelEventArgs e)
        {
            base.OnActivate(e);
            optionsPageControl.Options.LoadSettings();
        }

        protected override void OnApply(PageApplyEventArgs e)
        {
            if (e.ApplyBehavior == ApplyKind.Apply)
                optionsPageControl.Options.SaveSettings();

            base.OnApply(e);
        }
    }
}