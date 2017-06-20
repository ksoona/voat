﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;

namespace Voat.Common
{
    public static class StringExtensions
    {

        public static string ToRelativePath(this string url)
        {
            if (!url.IsTrimSafeNullOrEmpty())
            {
                var uri = new Uri(url);
                return uri.AbsolutePath;
            }
            return url;
        }
        public static string ReverseSplit(this string content, string seperator = ".")
        {
            if (!String.IsNullOrEmpty(content))
            {
                return String.Join(seperator, content.Split(new string[] { seperator }, StringSplitOptions.RemoveEmptyEntries).Reverse());
            }
            return content;
        }
        //public static IEnumerable<string> ToRelativePathParts(this string[] relativePaths, string additionalPart)
        //{
        //    string[] part = null;
        //    if (!String.IsNullOrEmpty(additionalPart))
        //    {
        //        part = new string[] { additionalPart };
        //    }
        //    return ToRelativePathParts(relativePaths, part);
        //}

        public static IEnumerable<string> ToPathParts(this IEnumerable<string> relativePaths, IEnumerable<string> additionalParts = null)
        {
            List<string> parts = new List<string>();
            relativePaths.ToList().ForEach(x =>
            {
                parts.AddRange(x.ToPathParts());
            });
            if (additionalParts != null && additionalParts.Count() > 0)
            {
                parts.AddRange(additionalParts);
            }
            return parts.AsEnumerable();
        }

        public static IEnumerable<string> ToPathParts(this string relativePath)
        {
            relativePath = relativePath.TrimStart('~');
            var parts = relativePath.Split(new string[] { "/", "\\" }, StringSplitOptions.RemoveEmptyEntries).AsEnumerable();
            parts = parts.Select(x => x.TrimSafe()).Where(x => !String.IsNullOrEmpty(x)).ToList();
            return parts.AsEnumerable();
        }

        // credits to http://stackoverflow.com/questions/1613896/truncate-string-on-whole-words-in-net-c-sharp
        public static string TruncateAtWord(this string input, int length)
        {
            if (input == null || input.Length < length)
                return input;
            var iNextSpace = input.LastIndexOf(" ", length, StringComparison.Ordinal);
            return string.Format("{0}...", input.Substring(0, (iNextSpace > 0) ? iNextSpace : length).Trim());
        }

        // check if a string contains unicode characters
        public static bool ContainsUnicode(this string stringToTest, bool includeUnprintableChars = true)
        {
            const int maxAnsiCode = 255;

            //Adding constraint for unprintable characters
            int minAnsiCode = (includeUnprintableChars ? 32 : 0);
            return stringToTest.Any(c => (c > maxAnsiCode || c < minAnsiCode));
        }

        public static string StripWhiteSpace(this string stringToClean)
        {
            var scrubbed = stringToClean;
            if (!String.IsNullOrEmpty(scrubbed))
            {
                scrubbed = Regex.Replace(scrubbed, @"\s{2,}", " ").Trim();
            }
            return scrubbed;
        }

        // string unicode characters from a string
        public static string StripUnicode(this string stringToClean, bool includeUnprintableChars = true, bool includeWhitespace = true)
        {
            var scrubbed = stringToClean;
            if (!String.IsNullOrEmpty(scrubbed))
            {
                scrubbed = Regex.Replace(scrubbed, String.Format(@"[^\u00{0}-\u00FF]", (includeUnprintableChars ? "20" : "00")), string.Empty).Trim();

                //remove sequential whitespace
                if (includeWhitespace)
                {
                    scrubbed = StripWhiteSpace(scrubbed);
                }
            }
            return scrubbed;
        }
    }
}
