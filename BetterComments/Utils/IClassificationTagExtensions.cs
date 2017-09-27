using Microsoft.VisualStudio.Text.Tagging;

namespace BetterComments
{
    internal static class IClassificationTagExtensions
    {
        public static bool IsComment(this IClassificationTag tag)
        {
            return tag.ClassificationType.Classification.ContainsCaseIgnored("comment");
        }

        public static bool IsXmlDoc(this IClassificationTag tag)
        {
            return tag.ClassificationType.Classification.ContainsCaseIgnored("doc");
        }
    }
}