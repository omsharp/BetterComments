using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;

namespace BetterComments.Options
{
    public partial class OptionsPageControl
    {
        public FontOptions Options { get; } = new FontOptions();

        public OptionsPageControl()
        {
            InitializeComponent();
            FontsComboBox.ItemsSource = GetInstalledFonts();
            SizeComboBox.ItemsSource = Enumerable.Range(1, 24).Select(n => (double)n);
            DataContext = Options;
        }

        public void SetOptions(FontOptions options)
        {
            Options.Font = options.Font;
            Options.Size = options.Size;
            Options.IsItalic = options.IsItalic;
            Options.IsBold = options.IsBold;
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
