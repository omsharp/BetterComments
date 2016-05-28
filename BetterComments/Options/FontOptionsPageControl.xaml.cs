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
            SizeComboBox.ItemsSource = new[] { 3.0, 2.5, 2.0, 1.5, 1.0, 0.0, -1.0, -1.5, -2.0, -2.5, -3.0 };
            OpacityComboBox.ItemsSource = new[] { 1.0, 0.9, 0.8, 0.7, 0.6, 0.5, 0.4, 0.3, 0.2, 0.1 };
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
