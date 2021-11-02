using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;

namespace Epsilon.Utils
{
    public static class UriHelper
    {
        /// <summary>When val is null, the parameter will be removed.</summary>
        private static string AddUrlParameter(string u, string parameter, string val, bool merge)
        {
            if (string.IsNullOrEmpty(parameter))
                return u;
            int startAt = u.IndexOf('?');
            if (val == null)
            {
                merge = true;
            }
            else
            {
                if (startAt < 0)
                    return u + (object)'?' + parameter + (object)'=' + val;
                if (startAt == u.Length - 1)
                    return u + parameter + (object)'=' + val;
            }
            if (merge)
            {
                Match match = new Regex("(&|\\?)" + parameter + "((?<1>=[^&]*))?(&|$)", RegexOptions.IgnoreCase | RegexOptions.ExplicitCapture | RegexOptions.CultureInvariant).Match(u, startAt);
                if (match.Success)
                {
                    if (val == null)
                        return startAt == match.Index ? u.Remove(match.Index + 1, match.Length - 1) : (u.Length != match.Index + match.Length ? u.Remove(match.Index, match.Length - 1) : u.Remove(match.Index, match.Length));
                    System.Text.RegularExpressions.Group group = match.Groups[1];
                    int length = match.Index + parameter.Length + 1;
                    return u.Substring(0, length) + (object)'=' + val + u.Substring(length + group.Length);
                }
            }
            string str;
            if (val == null)
                str = u;
            else
                str = u + (object)'&' + parameter + (object)'=' + val;
            return str;
        }

        /// <summary>
        /// Ensures that the given parameter occurs in the query string with the given value.
        /// </summary>
        /// <param name="u">Url that may already contain the parameter: in such case, its value
        /// will be updated.</param>
        /// <param name="parameter">Name of the parameter.</param>
        /// <param name="val">Value of the parameter. It must be url safe since this method
        /// will not escape it.</param>
        /// <returns>The url with the appended or updated parameter.</returns>
        public static string AssumeUrlParameter(string u, string parameter, string val)
        {
            val ??= string.Empty;
            return AddUrlParameter(u, parameter, val, true);
        }

        /// <summary>Removes the given parameter from the url.</summary>
        /// <param name="u">Original url</param>
        /// <param name="parameter">Parameter name to remove.</param>
        /// <returns>An url without the parameter.</returns>
        public static string RemoveUrlParameter(string u, string parameter) => AddUrlParameter(u, parameter, (string)null, true);

        /// <summary>
        /// Appends the given parameter and value to the url. If the parameter name already exists
        /// in the url (and you do not want duplicated parameters), use <see cref="M:AssumeUrlParameter" />
        /// instead.
        /// </summary>
        /// <param name="u">Url</param>
        /// <param name="parameter">Name of the parameter.</param>
        /// <param name="val">Value of the parameter. It must be url safe since this method
        /// will not escape it.</param>
        /// <returns>An url with the parameter and value added.</returns>
        public static string AppendUrlParameter(string u, string parameter, string val)
        {
            if (val == null)
                val = string.Empty;
            return UriHelper.AddUrlParameter(u, parameter, val, false);
        }
    }
}
