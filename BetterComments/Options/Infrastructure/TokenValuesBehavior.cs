using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace BetterComments.Options {

    public static class TokenValuesBehavior {

        #region Key

        public static readonly DependencyProperty KeyProperty = DependencyProperty.RegisterAttached(
            "Key",
            typeof(String),
            typeof(TokenValuesBehavior),
            new PropertyMetadata(null, KeyChanged)
        );

        public static String GetKey(DependencyObject target) 
        {
            return (String)target.GetValue(KeyProperty);
        }

        public static void SetKey(DependencyObject target, String value) 
        {
            target.SetValue(KeyProperty, value);
        }

        private static void KeyChanged(DependencyObject target, DependencyPropertyChangedEventArgs e) 
        {
            if (target is TextBox control) 
            {
                var behavior = GetOrCreateBehavior(control);
                behavior.Key = e.NewValue as String;
            }
        }


        #endregion

        #region Converter

        public static readonly DependencyProperty ConverterProperty = DependencyProperty.RegisterAttached(
            "Converter",
            typeof(TokenValuesToValueConverter),
            typeof(TokenValuesBehavior),
            new PropertyMetadata(null, ConverterChanged)
        );

        public static TokenValuesToValueConverter GetConverter(DependencyObject target) 
        {
            return (TokenValuesToValueConverter)target.GetValue(ConverterProperty);
        }

        public static void SetConverter(DependencyObject target, TokenValuesToValueConverter value) 
        {
            target.SetValue(ConverterProperty, value);
        }


        private static void ConverterChanged(DependencyObject target, DependencyPropertyChangedEventArgs e) 
        {
            if (target is TextBox control) 
            {
                var behavior = GetOrCreateBehavior(control);
                behavior.Converter = e.NewValue as TokenValuesToValueConverter;
            }
        }


        #endregion

        #region Behavior

        private static readonly DependencyProperty BehaviorProperty = DependencyProperty.RegisterAttached(
            "Behavior",
            typeof(TokenValuesBehaviorImpl),
            typeof(TokenValuesBehavior),
            new PropertyMetadata(null)
        );

        private static TokenValuesBehaviorImpl GetOrCreateBehavior(TextBox control) 
        {
            var behavior = control.GetValue(BehaviorProperty) as TokenValuesBehaviorImpl;
            if (behavior == null) 
            {
                behavior = new TokenValuesBehaviorImpl(control);
                control.SetValue(BehaviorProperty, behavior);
            }
            return behavior;
        }

        #endregion

    }

    public class TokenValuesBehaviorImpl {

        private readonly WeakReference targetObject;
        protected TextBox TargetObject {
            get { return targetObject.Target as TextBox; }
        }

        #region Key
        private String key;
        public String Key {
            get { return key; }
            set {
                if (value != key) 
                {
                    key = value;
                    CreateConverter();
                }
            }
        }
        #endregion

        #region Converter
        private TokenValuesToValueConverter converter;
        public TokenValuesToValueConverter Converter {
            get { return converter; }
            set {
                if (value != converter) 
                {
                    converter = value;
                    CreateConverter();
                }
            }
        }
        #endregion

        public TokenValuesBehaviorImpl(TextBox control) 
        {
            if (control == null) { throw new ArgumentNullException("control"); }
            targetObject = new WeakReference(control);
            control.TextChanged += Control_TextChanged;
        }

        private void Control_TextChanged(object sender, TextChangedEventArgs e) 
        {
            if (IsValid && sender is TextBox control) 
            {
                if (!Converter.Tokens.ContainsKey(Key)) 
                {
                    Converter.Tokens.Add(Key, String.Empty);
                }
                ValidateControl(control);
                Converter.Tokens[Key] = control.Text;
                control.GetBindingExpression(TextBox.TextProperty).UpdateTarget();
            }
        }

        private Boolean ValidateControl(TextBox control) 
        {
            var binding = BindingOperations.GetBinding(control, TextBox.TextProperty);
            foreach (var rule in binding.ValidationRules) 
            {
                if (!rule.Validate(control.GetValue(TextBox.TextProperty), CultureInfo.CurrentCulture).IsValid) 
                {
                    return false;
                }
            }
            return true;

        }

        private void CreateConverter() 
        {
            if (IsValid) 
            {
                var binding = new Binding {
                    Mode = BindingMode.OneWay,
                    Converter = Converter,
                };
                binding.ValidationRules.Add(new RequiredAndUniqueRule(Converter.Tokens) {
                    ValidatesOnTargetUpdated = true
                });
                BindingOperations.SetBinding(TargetObject, TextBox.TextProperty, binding);
            }
        }

        private Boolean IsValid 
        {
            get { return Converter != null && !String.IsNullOrEmpty(Key); }
        }

    }

}
