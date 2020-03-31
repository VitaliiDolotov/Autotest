using System.Globalization;
using System.Text.RegularExpressions;

namespace Autotest.Extensions
{
    public static class StringExtensions
    {
        public static bool ContainsText(this string fullText, string term)
        {
            CultureInfo culture = CultureInfo.InvariantCulture;
            return culture.CompareInfo.IndexOf(fullText, term, CompareOptions.IgnoreCase) >= 0;
        }

        static readonly Regex SpaceTrimmer = new Regex(@"\s\s+");

        public static string RemoveExtraSpaces(this string str)
        {
            try
            {
                return SpaceTrimmer.Replace(str, " ");
            }
            catch
            {
                return null;
            }
        }
    }
}
