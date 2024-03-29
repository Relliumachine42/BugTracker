﻿using System.Globalization;
using System.Text.RegularExpressions;
using System.Text;

namespace BugTracker.Helpers
{
    public static class StringHelper
    {
        public static string BTSlug(string? title)
        {
            //Remove all accents and make the string lower case
            string? output = RemoveAccents(title).ToLower();

            // Remove special characters
            output = Regex.Replace(output, @"[^A-Za-z0-9\s-]", "");

            // Remove all additional space in favor of just one
            output = Regex.Replace(output, @"\s+", " ");

            //Replace all spaces with the hyphen
            output = Regex.Replace(output, @"\s", "-");


            return output;
        }

        private static string RemoveAccents(string? title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return title!;
            }

            //Convert for Unicode
            title = title.Normalize(NormalizationForm.FormD);
            //Format unicode/ascii
            char[] chars = title.Where(c => CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark).ToArray();
            // Convert and return the new title
            return new string(chars).Normalize(NormalizationForm.FormC);
        }
    }
}
