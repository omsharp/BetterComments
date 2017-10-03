using Microsoft.VisualStudio.Shell;
using System;
using System.ComponentModel;
using System.Globalization;
using System.Media;
using System.Windows;

namespace BetterComments.Options
{
    public abstract class OptionsPageBase : UIElementDialogPage
    {
        protected bool TokensValidated { get; private set; }

        protected override UIElement Child { get; }

        protected override void OnActivate(CancelEventArgs e)
        {
            TokensValidated = false;
            base.OnActivate(e);
        }

        protected override void OnApply(PageApplyEventArgs e)
        {
            TokensValidated = ValidateTokens();

            if (TokensValidated) 
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

        protected override void OnClosed(EventArgs e)
        {
            if (TokensValidated)
            {
                SettingsStore.SaveSettings(Settings.Instance);
            }
            else
            {
                SettingsStore.LoadSettings(Settings.Instance);
            }

            base.OnClosed(e);
        }

        protected bool ValidateTokens()
        {
            var rule = new RequiredAndUniqueRule();

            foreach (var tk in Settings.Instance.CommentTokens)
            {
                if (!rule.Validate(tk.CurrentValue, CultureInfo.InvariantCulture).IsValid)
                    return false;
            }

            return true;
        }

        protected void ShowInvalidTokenMessage()
        {
            SystemSounds.Exclamation.Play();
            MessageBox.Show("Invalid token!", "Better Comments", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}