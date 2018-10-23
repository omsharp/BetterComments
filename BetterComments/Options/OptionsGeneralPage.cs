using System.Runtime.InteropServices;
using System.Windows;

namespace BetterComments.Options
{
   [ClassInterface(ClassInterfaceType.AutoDual)]
   [ComVisible(true)]
   [Guid("68d5d312-2753-4ce0-b900-2eb1ef8101c2")]
   public class OptionsGeneralPage : OptionsPageBase
   {
      private OptionsGeneralPageControl pageControl;

      protected override UIElement Child
      {
         get { return pageControl ?? (pageControl = new OptionsGeneralPageControl()); }
      }
   }
}