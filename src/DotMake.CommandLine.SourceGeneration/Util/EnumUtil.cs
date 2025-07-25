using System;
using System.Collections.Generic;
using System.Linq;

namespace DotMake.CommandLine.SourceGeneration.Util
{
    public static class EnumUtil<TEnum> where TEnum : Enum
    {
        private static readonly bool IsFlagsEnum = typeof(TEnum).IsDefined(typeof(FlagsAttribute), false);
        private static readonly Dictionary<TEnum, EnumInfo> Cache;

        static EnumUtil()
        {
            var type = typeof(TEnum);

            Cache = Enum
                .GetValues(type)
                .Cast<TEnum>()
                .ToDictionary(value => value, value => new EnumInfo
                {
                    Type = type,
                    Name = Enum.GetName(type, value),
                    Value = value
                });
        }

        public static string ToFullName(TEnum value)
        {
            var fullName = string.Empty;

            if (Cache.TryGetValue(value, out var enumInfo))
            {
                fullName = enumInfo.FullName;
            }
            else if (IsFlagsEnum)
            {
                var flags =
                    Cache.Values
                        .Where(x => !EqualityComparer<TEnum>.Default.Equals(x.Value, default) && value.HasFlag(x.Value))
                        .Select(x => x.FullName);

                fullName = string.Join(" | ", flags);
            }

            return fullName;
        }

        public static IEnumerable<EnumInfo> Enumerate()
        {
            return Cache.Values;
        }

        public class EnumInfo
        {
            public Type Type { get; set; }

            public string Name { get; set; }

            public string FullName => $"{Type.FullName}.{Name}";

            public TEnum Value { get; set; }
        }
    }
}
