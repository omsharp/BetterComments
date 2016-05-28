using System;
using System.Linq;

namespace BetterComments.CommentsTagging
{
    internal static class StringExtensions
    {
        public static string TrimFromTheBeginning(this string source, int numberOfCharacters)
        {
            return source.Substring(numberOfCharacters, source.Length - numberOfCharacters);
        }

        public static bool EqualsCaseIgnored(this string text, string value)
        {
            return text.Equals(value, StringComparison.OrdinalIgnoreCase);
        }

        public static bool ContainsCaseIgnored(this string text, string value)
        {
            return text.IndexOf(value, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public static bool StartsWith(this string text, string value, int startIndex, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            if (string.IsNullOrWhiteSpace(text) || startIndex > text.Length || string.IsNullOrWhiteSpace(value))
                return false;

            return text.IndexOf(value, startIndex, comparison) == startIndex;
        }

        public static string StartsWithOneOf(this string text, string[] strings, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            if (string.IsNullOrWhiteSpace(text) || strings.Length == 0)
                return null;


            return strings.FirstOrDefault(s => text.StartsWith(s, comparison));
        }

        public static bool StartsWithAnyOf(this string text, string[] strings, StringComparison comparison = StringComparison.OrdinalIgnoreCase)
        {
            if (string.IsNullOrWhiteSpace(text) || strings.Length == 0)
                return false;

            return !string.IsNullOrWhiteSpace(strings.FirstOrDefault(s => text.StartsWith(s, comparison)));
        }
    }
}