using Microsoft.VisualStudio.PlatformUI;
using System.Collections.Generic;
using System.Drawing.Text;
using System.Linq;
using System;
using BetterComments.CommentsTagging;
using System.Windows.Controls;

namespace BetterComments.Options {
    public partial class OptionsTokensPageControl {
        public Settings Settings { get; } = Settings.Instance;

        public OptionsTokensPageControl() {
            DataContext = Settings;

            InitializeComponent();
            ListTokens.ItemsSource = GetCommentTypeTokens();
        }

        private static IEnumerable<String> GetCommentTypeTokens() {
            return Enum.GetNames(typeof(CommentType)).Where(p => ((CommentType)Enum.Parse(typeof(CommentType), p)).GetAttribute<CommentIgnoreAttribute>() == null);
        }

        private void Button_Click(object sender, System.Windows.RoutedEventArgs e) {
            Settings.TokenValues = Settings.GetTokenDefaultValues();
            RefreshItemControls();
        }

        public void RefreshItemControls() {
            ListTokens.ItemsSource = null;
            ListTokens.ItemsSource = GetCommentTypeTokens();
        }
    }
}
