using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Windows;

namespace BetterComments.Options
{
   [ClassInterface(ClassInterfaceType.AutoDual)]
   [ComVisible(true)]
   [Guid("3F4564D7-3A70-4322-81FF-45E94C606D7B")]
   public class OptionsTokensPage : OptionsPageBase
   {
      private OptionsTokensPageControl pageControl;

      protected override UIElement Child
      {
         get { return pageControl ?? (pageControl = new OptionsTokensPageControl()); }
      }

      protected override void OnDeactivate(CancelEventArgs e)
      {
         if (!TokensValidated && !ValidateTokens())
         {
            e.Cancel = true;
            ShowInvalidTokenMessage();
         }

         base.OnDeactivate(e);
      }
   }
}