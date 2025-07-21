using System;

namespace DotMake.CommandLine.Util
{
    internal static class HashUtil
    {
        /// <summary>
        /// Gets a stable hash code (int). 
        /// </summary>
        /// <param name="source">A string instance.</param>
        /// <returns>A <see cref="int" /> hash value.</returns>
        public static int GetStableHashCode32(string source)
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
        public static string GetStableStringHashCode32(string source)
        {
            var hashCode = GetStableHashCode32(source);

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
    }
}
