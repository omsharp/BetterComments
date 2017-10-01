using BetterComments.CommentsTagging;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;

namespace BetterComments.Options
{
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    [Guid("3F4564D7-3A70-4322-81FF-45E94C606D7B")]
    public class OptionsTokensPage : UIElementDialogPage
    {
        private OptionsTokensPageControl pageControl;

        protected override UIElement Child
        {
            get { return pageControl ?? (pageControl = new OptionsTokensPageControl()); }
        }

        protected override void OnActivate(CancelEventArgs e)
        {
            pageControl.RefreshItemControls();

            base.OnActivate(e);
        }

        protected override void OnApply(PageApplyEventArgs e)
        {
            CheckTokensAndReplaceInvalidByDefault();

            if (e.ApplyBehavior == ApplyKind.Apply)
                SettingsStore.SaveSettings(pageControl.Settings);

            base.OnApply(e);
        }

        private void CheckTokensAndReplaceInvalidByDefault()
        {
            foreach (var item in pageControl.Settings.TokenValues.ToList())
            {
                if (String.IsNullOrEmpty(item.Value))
                {
                    var enumKey = ((CommentType)Enum.Parse(typeof(CommentType), item.Key));
                    var defaultValue = enumKey.GetAttribute<CommentDefaultAttribute>()?.Value;
                    pageControl.Settings.TokenValues[item.Key] = defaultValue;
                }
            }
        }
    }
}