using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Media;
using System.Runtime.InteropServices;
using System.Windows;

namespace BetterComments.Options
{
    [ClassInterface(ClassInterfaceType.AutoDual)]
    [ComVisible(true)]
    [Guid("3F4564D7-3A70-4322-81FF-45E94C606D7B")]
    public class OptionsTokensPage : UIElementDialogPage
    {
        private bool tokensValidated = false;
        private OptionsTokensPageControl pageControl;

        protected override UIElement Child
        {
            get { return pageControl ?? (pageControl = new OptionsTokensPageControl()); }
        }

        protected override void OnApply(PageApplyEventArgs e)
        {
            tokensValidated = ValidateTokens();

            if (tokensValidated) //ToDo: validation check should be here
            {
                e.ApplyBehavior = ApplyKind.Apply;
            }
            else
            {
                e.ApplyBehavior = ApplyKind.CancelNoNavigate;
                ShowInvalidTokenMessage();
            }

            base.OnApply(e);
        }

        protected override void OnDeactivate(CancelEventArgs e)
        {
            if (!tokensValidated && !ValidateTokens())
            {
                e.Cancel = true;
                ShowInvalidTokenMessage();
            }

            base.OnDeactivate(e);
        }

        protected override void OnClosed(EventArgs e)
        {
            if (tokensValidated)
            {
                SettingsStore.SaveSettings(Settings.Instance);
            }
            else
            {
                SettingsStore.LoadSettings(Settings.Instance);
            }

            tokensValidated = false;
            base.OnClosed(e);
        }

        private bool ValidateTokens()
        {
            var rule = new RequiredAndUniqueRule();

            foreach (var tk in Settings.Instance.CommentTokens)
            {
                if (!rule.Validate(tk.CurrentValue, CultureInfo.InvariantCulture).IsValid)
                    return false;
            }

            return true;
        }

        private void ShowInvalidTokenMessage()
        {
            SystemSounds.Exclamation.Play();
            MessageBox.Show("Invalid token!", "Better Comments", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}