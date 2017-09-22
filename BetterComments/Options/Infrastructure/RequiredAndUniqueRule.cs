using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace BetterComments.Options {
    public class RequiredAndUniqueRule : ValidationRule {

        private Dictionary<String, String> m_Tokens;

        public RequiredAndUniqueRule(Dictionary<String, String> tokens) {
            m_Tokens = tokens;
        }

        public override ValidationResult Validate(object value, CultureInfo cultureInfo) {
            if (value is String str) {
                if (String.IsNullOrEmpty(str)) {
                    return new ValidationResult(false, "Value is required");
                }
                return ValidationResult.ValidResult;
            }
            return new ValidationResult(false, "Value is not a valid string");
        }
    }
}
