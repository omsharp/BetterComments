using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;

namespace BetterComments.Options
{
   public partial class OptionsPageControl
   {
      public Settings Settings { get; } = new Settings();

      public OptionsPageControl()
      {
         InitializeComponent();
         FontsComboBox.ItemsSource = GetInstalledFonts();
      }

      public void SetDataContext(Settings settings)
      {
         Settings.Copy(settings);
         DataContext = Settings;
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
