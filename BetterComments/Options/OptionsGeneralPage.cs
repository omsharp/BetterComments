using System.Runtime.InteropServices;
using System.Windows;
using Microsoft.VisualStudio.Shell;
using System.ComponentModel;

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
      
      protected override void OnActivate(CancelEventArgs e)
      {
         SettingsStore.LoadSettings(pageControl.Settings);

         base.OnActivate(e);
      }

      protected override void OnApply(PageApplyEventArgs e)
      {
         if (e.ApplyBehavior == ApplyKind.Apply)
            SettingsStore.SaveSettings(pageControl.Settings);

         base.OnApply(e);
      }
   }
}