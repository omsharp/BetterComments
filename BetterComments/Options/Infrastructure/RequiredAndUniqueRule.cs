using System.Globalization;
using System.Linq;
using System.Windows.Controls;
using System.Windows.Data;

namespace BetterComments.Options
{
   public class RequiredAndUniqueRule : ValidationRule
   {
      public RequiredAndUniqueRule()
      {
      }

      public override ValidationResult Validate(object value, CultureInfo cultureInfo)
      {
         var str = GetBoundValue(value) as string;

         if (!(str is string))
         {
            return new ValidationResult(false, "Value is not a string.");
         }

         if (str.IndexOfAny(new[] { '|', ',', '/' }) > -1)
         {
            return new ValidationResult(false, "Tokens can't contain any of the following characters | , /");
         }

         if (string.IsNullOrWhiteSpace(str))
         {
            return new ValidationResult(false, "Value is required.");
         }

         if (Settings.Instance.CommentTokens.Count(t => t.CurrentValue.Equals(str)) > 1)
         {
            return new ValidationResult(false, "Value must be unique.");
         }

         return ValidationResult.ValidResult;
      }

      private object GetBoundValue(object value)
      {
         if (value is BindingExpression be)
         {
            // ValidationStep = UpdatedValue or CommittedValue (Validate after setting)
            return be.DataItem
                     .GetType()
                     .GetProperty(be.ParentBinding.Path.Path)
                     .GetValue(be.DataItem, null);
         }
         else
         {
            // ValidationStep = RawProposedValue or ConvertedProposedValue
            return value;
         }
      }
   }
}