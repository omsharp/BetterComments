using System;
using System.Collections.Generic;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace BetterComments.Options
{
    public class TokenValuesToValueConverter : Freezable, IValueConverter
    {
        #region Tokens

        public static readonly DependencyProperty TokensProperty = DependencyProperty.Register(
            "Tokens",
            typeof(Dictionary<String, String>),
            typeof(TokenValuesToValueConverter),
            new PropertyMetadata(null)
        );

        public Dictionary<String, String> Tokens
        {
            get { return (Dictionary<String, String>)GetValue(TokensProperty); }
            set { SetValue(TokensProperty, value); }
        }

        #endregion Tokens

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (Tokens != null && value is String key)
            {
                if (!Tokens.ContainsKey(key))
                {
                    Tokens.Add(key, String.Empty);
                }
                return Tokens[key];
            }
            return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        protected override Freezable CreateInstanceCore()
        {
            return new TokenValuesToValueConverter();
        }
    }
}