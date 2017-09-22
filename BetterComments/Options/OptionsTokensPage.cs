using System.Runtime.InteropServices;
using System.Windows;
using Microsoft.VisualStudio.Shell;
using System.ComponentModel;
using System;
using System.Linq;
using System.Collections.Generic;
using BetterComments.CommentsTagging;

namespace BetterComments.Options {

    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    [Guid("3F4564D7-3A70-4322-81FF-45E94C606D7B")]
    public class OptionsTokensPage : UIElementDialogPage {

        private OptionsTokensPageControl pageControl;

        protected override UIElement Child {
            get { return pageControl ?? (pageControl = new OptionsTokensPageControl()); }
        }

        protected override void OnActivate(CancelEventArgs e) {
            //SettingsStore.LoadSettings(pageControl.Settings);
            pageControl.RefreshItemControls();

            base.OnActivate(e);
        }

        protected override void OnApply(PageApplyEventArgs e) {
            CheckTokensAndReplaceInvalidByDefault();

            if (e.ApplyBehavior == ApplyKind.Apply)
                SettingsStore.SaveSettings(pageControl.Settings);

            base.OnApply(e);
        }

        private void CheckTokensAndReplaceInvalidByDefault() {
            foreach (KeyValuePair<String, String> item in pageControl.Settings.TokenValues.ToList()) {
                if (String.IsNullOrEmpty(item.Value)) {
                    CommentType enumKey = ((CommentType)Enum.Parse(typeof(CommentType), item.Key));
                    String defaultValue = enumKey.GetAttribute<CommentDefaultAttribute>()?.Value;
                    pageControl.Settings.TokenValues[item.Key] = defaultValue;
                }
            }
        }
    }
}