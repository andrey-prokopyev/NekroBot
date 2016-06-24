namespace NekroBot.Messages
{
    using System;
    using System.Text.RegularExpressions;

    internal class Formatter
    {
        private static readonly Regex CleanNonBmpUnicodeChars = new Regex(@"\p{Cs}", RegexOptions.Compiled);
        private static readonly Regex StripTags = new Regex(@"<[^>]*(>|$)", RegexOptions.Compiled);

        public string Format(MessageUpdate update)
        {
            var text = CleanNonBmpUnicodeChars.Replace(update.Text, string.Empty);
            text = StripTags.Replace(text, String.Empty);
            text = text.Trim();
            if (!string.IsNullOrEmpty(text) && !string.IsNullOrEmpty(update.Sender))
            {
                return $"{update.Sender}: {text}";
            }

            return text;
        }
    }
}