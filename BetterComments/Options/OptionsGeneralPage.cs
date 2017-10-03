using Microsoft.VisualStudio.Shell;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows;

namespace BetterComments.Options
{
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    [Guid("68d5d312-2753-4ce0-b900-2eb1ef8101c2")]
    public class OptionsGeneralPage : UIElementDialogPage
    {
        private OptionsGeneralPageControl pageControl;

        protected override UIElement Child
        {
            get { return pageControl ?? (pageControl = new OptionsGeneralPageControl()); }
        }

        protected override void OnApply(PageApplyEventArgs e)
        {
            //! To prevent saving twice or saving invalid tokens, saving logic is handled only in OptionsTokensPage.cs.
            base.OnApply(e);
        }
    }
}