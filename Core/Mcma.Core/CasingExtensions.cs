using System.Text;

namespace Mcma.Core
{
    public static class CasingExtensions
    {
        #region Camel <-> Pascal

        /// <summary>
        /// Converts a camel case string to Pascal case
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string CamelCaseToPascalCase(this string text)
        {
            return char.ToUpper(text[0]) + text.Substring(1);
        }

        /// <summary>
        /// Converts a Pascal case string to camel case
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string PascalCaseToCamelCase(this string text)
        {
            return char.ToLower(text[0]) + text.Substring(1);
        }

        #endregion

        #region Camel <-> Kebab

        /// <summary>
        /// Converts a kebab case string to camel case
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string KebabCaseToCamelCase(this string text)
        {
            var sb = new StringBuilder();

            for (var i = 0; i < text.Length; i++)
            {
                if (text[i] == '-')
                {
                    i++;
                    sb.Append(char.ToUpper(text[i]));
                }
                else
                    sb.Append(text[i]);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Converts a camel case string to kebab case
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string CamelCaseToKebabCase(this string text)
        {
            var sb = new StringBuilder();

            foreach (var c in text)
            {
                if (char.IsUpper(c))
                {
                    if (sb.Length > 0)
                        sb.Append("-");
                    sb.Append(char.ToLower(c));
                }
                else
                    sb.Append(c);
            }

            return sb.ToString();
        }

        #endregion

        #region Pascal <-> Kebab

        /// <summary>
        /// Converts a Pascal case string to kebab case
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string PascalCaseToKebabCase(this string text)
        {
            return CamelCaseToKebabCase(PascalCaseToCamelCase(text));
        }

        /// <summary>
        /// Converts a kebab case string to Pascal case
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        public static string KebabCaseToPascalCase(this string text)
        {
            return CamelCaseToPascalCase(KebabCaseToCamelCase(text));
        }

        #endregion
    }
}