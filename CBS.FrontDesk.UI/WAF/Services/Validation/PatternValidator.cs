using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace CBS.FrontDesk.UI.WAF.Services.Validation
{
    using System.Text.RegularExpressions;

    public static class PatternValidator
    {
        /// <summary>
        /// Checks if the input string matches any pattern in the provided list.
        /// </summary>
        /// <param name="input">The input string to inspect.</param>
        /// <param name="patterns">A list of suspicious patterns (regex).</param>
        /// <param name="matchedPattern">The specific pattern that was matched (if any).</param>
        /// <returns>True if a match is found; false otherwise.</returns>
        public static bool ContainsSuspiciousPattern(string input, List<string> patterns, out string matchedPattern)
        {
            matchedPattern = null;

            if (string.IsNullOrWhiteSpace(input) || patterns == null || patterns.Count == 0)
                return false;

            foreach (var pattern in patterns)
            {
                if (input.IndexOf(pattern, StringComparison.OrdinalIgnoreCase) >= 0)
                {
                    matchedPattern = pattern;
                    return true;
                }
            }

            return false;
        }

    }

}