using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace DotMake.CommandLine
{
    /// <summary>
    /// Provides extension methods for <see cref="string" />.
    /// </summary>
    public static class StringExtensions
    {
        /// <summary>
        /// Strips specific suffixes from the string.
        /// </summary>
        /// <param name="value">A string instance.</param>
        /// <param name="suffixes">Suffix strings to remove.</param>
        /// <param name="ignoreCase">Whether to ignore case.</param>
        /// <returns>A new <see cref="string" /> instance.</returns>
        public static string StripSuffixes(this string value, IEnumerable<string> suffixes, bool ignoreCase = true)
        {
            if (string.IsNullOrEmpty(value))
                return value;

            foreach (var suffix in suffixes.OrderByDescending(s => s.Length))
            {
                if (string.IsNullOrEmpty(suffix)
                    || suffix.Length >= value.Length)
                    continue;

                if (value.EndsWith(suffix, ignoreCase ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal))
                    return value.Substring(0, value.Length - suffix.Length);
            }

            return value;
        }

        private static readonly Regex SplitWordsRegex = new Regex(
            @"(?<=[a-z])(?=[A-Z])" //what precedes is a lowercase, and what follows is an uppercase
            + @"|(?<=[0-9])(?=[A-Za-z])" //what precedes is a digit and what follows is a letter (or vice-versa)
            + @"|(?<=[A-Za-z])(?=[0-9])");

        /// <summary>
        /// Converts the string to a specific case.
        /// </summary>
        /// <param name="value">A string instance.</param>
        /// <param name="nameCasingConvention">The name casing convention to convert to.</param>
        /// <returns>A new <see cref="string" /> instance.</returns>
        public static string ToCase(this string value, CliNameCasingConvention nameCasingConvention)
        {
            if (string.IsNullOrWhiteSpace(value))
                return value;

            switch (nameCasingConvention)
            {
                case CliNameCasingConvention.LowerCase:
                    return value.ToLowerInvariant();
                case CliNameCasingConvention.UpperCase:
                    return value.ToUpperInvariant();
                case CliNameCasingConvention.TitleCase:
                    return string.Concat(
                        SplitWordsRegex.Split(value)
                            .Select(word => word.ToLowerInvariant().ToTitleCase())
                    );
                case CliNameCasingConvention.PascalCase:
                    return string.Concat(
                        SplitWordsRegex.Split(value.Trim())
                            .Select(word => Regex.Replace(word.ToLowerInvariant().ToTitleCase(), @"\s+", ""))
                    );
                case CliNameCasingConvention.CamelCase:
                    return string.Concat(
                        SplitWordsRegex.Split(value.Trim())
                            .Select((word, index) =>
                            {
                                word = (index == 0) ? word.ToLowerInvariant() : word.ToLowerInvariant().ToTitleCase();
                                return Regex.Replace(word, @"\s+", "");
                            })
                    );
                case CliNameCasingConvention.KebabCase:
                    return string.Join("-",
                        SplitWordsRegex.Split(value.Trim())
                            .Select(word => Regex.Replace(word.ToLowerInvariant(), @"\s+", "-"))
                    );
                case CliNameCasingConvention.SnakeCase:
                    return string.Join("_",
                        SplitWordsRegex.Split(value.Trim())
                            .Select(word => Regex.Replace(word.ToLowerInvariant(), @"\s+", "_"))
                    );
                default:
                    return value;
            }
        }

        private static string ToTitleCase(this string value)
        {
            return CultureInfo.InvariantCulture.TextInfo.ToTitleCase(value);
        }

        /// <summary>
        /// Adds a specific prefix to the string.
        /// </summary>
        /// <param name="alias">A string instance.</param>
        /// <param name="namePrefixConvention">The prefix convention to use.</param>
        /// <returns>A new <see cref="string" /> instance.</returns>
        public static string AddPrefix(this string alias, CliNamePrefixConvention namePrefixConvention)
        {
            var prefixLength = alias.GetPrefixLength();

            if (prefixLength > 0) //Has prefix
                return alias;

            switch (namePrefixConvention)
            {
                case CliNamePrefixConvention.SingleHyphen:
                    return "-" + alias;
                default:
                case CliNamePrefixConvention.DoubleHyphen:
                    return "--" + alias;
                case CliNamePrefixConvention.ForwardSlash:
                    return "/" + alias;
            }
        }

        /// <summary>
        /// Removes prefixes (-, --, /) from the string.
        /// </summary>
        /// <param name="alias">A string instance.</param>
        /// <returns>A new <see cref="string" /> instance.</returns>
        public static string RemovePrefix(this string alias)
        {
            var prefixLength = alias.GetPrefixLength();

            if (prefixLength > 0) //Has prefix
                return alias.Substring(prefixLength);

            return alias;
        }

        private static int GetPrefixLength(this string alias)
        {
            if (string.IsNullOrWhiteSpace(alias))
                return 0;

            if (alias[0] == '-')
            {
                if (alias.Length > 1 && alias[1] == '-')
                    return 2;

                return 1;
            }

            if (alias[0] == '/')
            {
                return 1;
            }

            return 0;
        }

        internal static (string Prefix, string Alias) SplitPrefix(this string rawAlias)
        {
            if (rawAlias[0] == '/')
            {
                return ("/", rawAlias.Substring(1));
            }
            else if (rawAlias[0] == '-')
            {
                if (rawAlias.Length > 1 && rawAlias[1] == '-')
                {
                    return ("--", rawAlias.Substring(2));
                }

                return ("-", rawAlias.Substring(1));
            }

            return (null, rawAlias);
        }

        /// <summary>
        /// Gets a stable hash code (int). 
        /// </summary>
        /// <param name="source">A string instance.</param>
        /// <returns>A <see cref="int" /> hash value.</returns>
        public static int GetStableHashCode32(this string source)
        {
            var span = source.AsSpan();

            // FNV-1a
            // For its performance, collision resistance, and outstanding distribution:
            // https://softwareengineering.stackexchange.com/a/145633
            unchecked
            {
                // Inspiration: https://gist.github.com/RobThree/25d764ea6d4849fdd0c79d15cda27d61
                // Confirmation: https://gist.github.com/StephenCleary/4f6568e5ab5bee7845943fdaef8426d2

                const uint fnv32Offset = 2166136261;
                const uint fnv32Prime = 16777619;

                var result = fnv32Offset;

                foreach (var t in span)
                    result = (result ^ t) * fnv32Prime;

                return (int)result;
            }
        }

        /// <summary>
        /// Gets a stable int hash code as string.
        /// </summary>
        /// <param name="source">A string instance.</param>
        /// <returns>A base32 encoded <see cref="string" /> hash value.</returns>
        public static string GetStableStringHashCode32(this string source)
        {
            var hashCode = source.GetStableHashCode32();

            var bytes = new byte[8];

            for (var i = 0; i < 4; i++)
                bytes[i] = (byte)(hashCode >> 8 * i);

            var chars = new char[13];
            ToBase32Chars8(bytes, chars.AsSpan());
            var result = new string(chars, 0, 7);

            return result;
        }

        private static char[] Base32Alphabet { get; } = "0123456789abcdefghjkmnpqrstvwxyz".ToCharArray();

        /// <summary>
        /// Converts the given 8 bytes to 13 base32 chars.
        /// </summary>
        private static void ToBase32Chars8(ReadOnlySpan<byte> bytes, Span<char> chars)
        {
            var ulongValue = 0UL;
            for (var i = 0; i < 8; i++) ulongValue = (ulongValue << 8) | bytes[i];

            // Can encode 8 bytes as 13 chars
            for (var i = 13 - 1; i >= 0; i--)
            {
                var quotient = ulongValue / 32UL;
                var remainder = ulongValue - 32UL * quotient;
                ulongValue = quotient;
                chars[i] = Base32Alphabet[(int)remainder];
            }
        }

        /// <summary>
        /// Gets an escaped string literal. Value is enclosed with double quotes and Unicode and ASCII non-printable characters are escaped.
        /// </summary>
        /// <param name="value">A string instance.</param>
        /// <param name="quoteChar">The enclosing quote char, by default it's double quotes.</param>
        /// <returns>A new <see cref="string" /> instance.</returns>
        public static string ToLiteral(this string value, char quoteChar = '"')
        {
            //https://stackoverflow.com/a/14087738

            var literal = new StringBuilder(value.Length + 2);

            literal.Append(quoteChar);

            foreach (var c in value)
            {
                switch (c)
                {
                    case '\"': literal.Append("\\\""); break;
                    case '\\': literal.Append(@"\\"); break;
                    case '\0': literal.Append(@"\0"); break;
                    case '\a': literal.Append(@"\a"); break;
                    case '\b': literal.Append(@"\b"); break;
                    case '\f': literal.Append(@"\f"); break;
                    case '\n': literal.Append(@"\n"); break;
                    case '\r': literal.Append(@"\r"); break;
                    case '\t': literal.Append(@"\t"); break;
                    case '\v': literal.Append(@"\v"); break;
                    default:
                        // ASCII printable character
                        if (c >= 0x20 && c <= 0x7e)
                        {
                            literal.Append(c);
                            
                        }
                        // UTF16 control characters
                        else if (char.GetUnicodeCategory(c) == UnicodeCategory.Control)
                        {
                            literal.Append(@"\u");
                            literal.Append(((int)c).ToString("x4"));
                        }
                        else
                        {
                            literal.Append(c);
                        }
                        break;
                }
            }

            literal.Append(quoteChar);

            return literal.ToString();
        }
    }
}
