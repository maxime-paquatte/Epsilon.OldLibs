using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Epsilon.Utils
{
    public static class StringHelper
    {
        public static string CleanUrl(string url, char replaceChar = '-')
        {
            return CleanUrl(url, replaceChar, 0);
        }

        public static string CleanUrl(string url, char replaceChar, int maxLength = 0)
        {
            url = StringHelper.RemoveDiacritics(url);
            url = Regex.Replace(url, @"[^a-zA-Z0-9\s]", String.Empty);
            url = url.Replace(' ', replaceChar).Replace(new string(replaceChar, 2), replaceChar.ToString());
            url = url.ToLower().Trim(replaceChar);
            return (maxLength > 0 && url.Length > maxLength) ? url.Substring(0, maxLength - 1) : url;
        }



        /// <summary>
        /// Removes diacritics from a string (converts them to their basic form after having filtered NonSpacingMark characters).
        /// From http://blogs.msdn.com/michkap/archive/2007/05/14/2629747.aspx
        /// </summary>
        /// <param name="sharedBuffer">A shared buffer that can be reused.</param>
        /// <param name="input">The string to process.If null, <see cref="String.Empty"/> is returned.</param>
        /// <returns>The string from which diacritics are removed.</returns>
        public static string RemoveDiacritics(ref StringBuilder sharedBuffer, string input)
        {
            if (input == null || input.Length == 0) return String.Empty;
            string stFormD = input.Normalize(NormalizationForm.FormD);
            int len = stFormD.Length;
            if (sharedBuffer == null) sharedBuffer = new StringBuilder(len);
            else
            {
                sharedBuffer.EnsureCapacity(len);
                sharedBuffer.Length = 0;
            }
            for (int i = 0; i < len; i++)
            {
                UnicodeCategory uc = CharUnicodeInfo.GetUnicodeCategory(stFormD, i);
                if (uc != UnicodeCategory.NonSpacingMark)
                {
                    sharedBuffer.Append(stFormD[i]);
                }
            }
            return sharedBuffer.ToString().Normalize(NormalizationForm.FormC);
        }

        public static string RemoveDiacritics(string input)
        {
            StringBuilder sb = null;
            return RemoveDiacritics(ref sb, input);
        }

        public static Regex TemplateRegex = new Regex("\\{\\{\\s*(?<Key>\\w*)\\s*\\}\\}", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        public static string ApplyTemplate(string template, object values)
        {
            var dic = ObjectHelper.AnonymousObjectToDictionary(values);
            return ApplyTemplate(template, dic);
        }

        public static string ApplyTemplate(string template, IDictionary<string, object> values)
        {
            if (template == null) throw new ArgumentNullException("template");
            if (values == null) throw new ArgumentNullException("values");

            return ApplyTemplate(template, s => values.TryGetValue(s, out var val) ? val?.ToString() : string.Empty);
        }

        public static string ApplyTemplate(string template, Func<string, string> fn)
        {
            if (template == null) throw new ArgumentNullException(nameof(template));
            if (fn == null) throw new ArgumentNullException(nameof(fn));

            return TemplateRegex.Replace(template, m => fn(m.Groups["Key"].Value));
        }

        private static readonly Regex EmailRegex = new Regex(@"([a-zA-Z0-9_\-\.]+)@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.)|(([a-zA-Z0-9\-]+\.)+))([a-zA-Z]{2,4}|[0-9]{1,3})", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant | RegexOptions.IgnorePatternWhitespace | RegexOptions.Compiled);

        public static bool IsValidEmail(string email)
        {
            return EmailRegex.IsMatch(email);
        }


        //https://stackoverflow.com/a/847251
        public static string MakeValidFileName(string name)
        {
            string invalidChars = System.Text.RegularExpressions.Regex.Escape(new string(System.IO.Path.GetInvalidFileNameChars()));
            string invalidRegStr = string.Format(@"([{0}]*\.+$)|([{0}]+)", invalidChars);

            return System.Text.RegularExpressions.Regex.Replace(name, invalidRegStr, "_");
        }
    }
}
