using System;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;

namespace BetterComments.Options
{
    public partial class OptionsPageControl
    {
        public OptionsPageControl()
        {
            InitializeComponent();
            FontsComboBox.ItemsSource = GetInstalledFonts();
            DataContext = FontSettingsManager.CurrentSettings;
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
