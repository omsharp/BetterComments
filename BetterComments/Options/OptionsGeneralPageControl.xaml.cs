using Microsoft.VisualStudio.PlatformUI;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;

namespace BetterComments.Options
{
   public partial class OptionsGeneralPageControl
   {
      public Settings Settings { get; } = new Settings();

      public OptionsGeneralPageControl()
      {
         DataContext = Settings;

         InitializeComponent();
         FontsComboBox.ItemsSource = GetInstalledFonts();
      }

      private static IEnumerable<string> GetInstalledFonts()
      {
         IEnumerable<string> result;

         using (var fonts = new InstalledFontCollection())
            result = fonts.Families.Select(f => f.Name);

         return result;
      }
   }
}
