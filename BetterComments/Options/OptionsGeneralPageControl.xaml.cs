using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;

namespace BetterComments.Options
{
   public partial class OptionsGeneralPageControl
   {
      public OptionsGeneralPageControl()
      {
         DataContext = Settings.Instance;

         InitializeComponent();
         FontsComboBox.ItemsSource = GetInstalledFonts();
      }

      private static IEnumerable<string> GetInstalledFonts()
      {
         IEnumerable<string> result;

         using (var fonts = new InstalledFontCollection())
         {
            result = fonts.Families.Select(f => f.Name);
         }

         return result;
      }

      private void Hyperlink_Click(object sender, System.Windows.RoutedEventArgs e)
      {
         System.Diagnostics.Process.Start("https://docs.google.com/forms/d/e/1FAIpQLScRNeHI2q4yiaAzfXtGOidp-Tu8E6TEaKNPWnE4Cos_osHX9w/viewform?usp=sf_link");
      }
   }
}