using System;
using System.CommandLine;
using System.Reflection;

namespace DotMake.CommandLine
{
    internal static class OptionExtensions
    {
        private static readonly PropertyInfo IsGlobalProperty =
            typeof(Option).GetProperty("IsGlobal", BindingFlags.Instance | BindingFlags.NonPublic);
        private static readonly PropertyInfo ArgumentProperty =
            typeof(Option).GetProperty("Argument", BindingFlags.Instance | BindingFlags.NonPublic);

        public static bool GetIsGlobal(this Option option)
        {
            var value = IsGlobalProperty.GetValue(option);
            if (value == null)
                throw new NullReferenceException(nameof(IsGlobalProperty));

            return (bool)value;
        }

        public static Argument GetArgument(this Option option)
        {
            var value = ArgumentProperty.GetValue(option);
            if (value == null)
                throw new NullReferenceException(nameof(ArgumentProperty));

            return (Argument)value;
        }
    }
}
