// The MIT License (MIT)

// Copyright (c) 2014 Alec Siu, Eric Stollnitz

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace OpenPhotosynth.WebClient
{
    public static class ExtensionMethods
    {
        /// <summary>
        /// Parses the fragment part of a URI, i.e. everything after the '#', into key-value pairs
        /// (if in query string format).
        /// </summary>
        /// <param name="uri">The URI to parse.</param>
        /// <returns>A dictionary of key-value pairs</returns>
        public static Dictionary<string, string> ParseFragment(this Uri uri)
        {
            return ParseString(uri.Fragment, '#');
        }

        /// <summary>
        /// Parses the query string part of a URI, i.e. everything after the '#', into key-value pairs
        /// (if in query string format).
        /// </summary>
        /// <param name="uri">The URI to parse.</param>
        /// <returns>A dictionary of key-value pairs</returns>
        public static Dictionary<string, string> ParseQueryString(this Uri uri)
        {
            return ParseString(uri.Query, '?');
        }

        private static Dictionary<string, string> ParseString(string s, char prefix)
        {
            var parameters = new Dictionary<string, string>();
            if (s != null && s.Length > 0 && s[0] == prefix)
            {
                foreach (string paramString in s.Substring(1).Split('&'))
                {
                    int index = paramString.IndexOf('=');
                    if (index > 0)
                    {
                        string name = Uri.UnescapeDataString(paramString.Substring(0, index));
                        string value = Uri.UnescapeDataString(paramString.Substring(index + 1));
                        parameters.Add(name, value);
                    }
                }
            }

            return parameters;
        }

        /// <summary>
        /// Formats a TimeSpan into a human-friendly English string. Integer-precision for durations
        /// greater than 12 hours.
        /// </summary>
        /// <param name="timeSpan">The time span to format.</param>
        /// <returns>A human-friendly string, e.g. "2 minutes 35 seconds"</returns>
        public static string FormatTimeSpan(this TimeSpan timeSpan)
        {
            if (timeSpan.TotalDays > 1)
            {
                return Pluralize((int)Math.Round(timeSpan.TotalDays), "day");
            }
            else if (timeSpan.TotalHours > 12)
            {
                return Pluralize((int)Math.Round(timeSpan.TotalHours), "hour");
            }
            else if (timeSpan.TotalHours > 1)
            {
                int integralHours = (int)Math.Floor(timeSpan.TotalHours);
                return Pluralize(
                    integralHours, "hour",
                    (int)Math.Round((timeSpan - TimeSpan.FromHours(integralHours)).TotalMinutes), "minute");
            }
            else if (timeSpan.TotalMinutes > 1)
            {
                int integralMinutes = (int)Math.Floor(timeSpan.TotalMinutes);
                return Pluralize(
                    integralMinutes, "minute",
                    (int)Math.Round((timeSpan - TimeSpan.FromMinutes(integralMinutes)).TotalSeconds), "second");
            }
            else
            {
                return Pluralize((int)Math.Round(timeSpan.TotalSeconds), "second");
            }
        }

        private static string Pluralize(int value, string unit)
        {
            return string.Format(CultureInfo.CurrentCulture, "{0} {1}{2}", value, unit, value == 1 ? String.Empty : "s");
        }

        private static string Pluralize(int value, string unit, int remainder, string remainderUnit)
        {
            StringBuilder sb = new StringBuilder(Pluralize(value, unit));
            if (remainder > 0)
            {
                sb.Append(" ").Append(Pluralize(remainder, remainderUnit));
            }

            return sb.ToString();
        }
    }
}
