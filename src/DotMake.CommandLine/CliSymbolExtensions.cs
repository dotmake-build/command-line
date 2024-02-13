using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Reflection;

namespace DotMake.CommandLine
{
    internal static class CliSymbolExtensions
    {
        private static readonly PropertyInfo ArgumentProperty =
            typeof(CliOption).GetProperty("Argument", BindingFlags.Instance | BindingFlags.NonPublic);

        public static CliArgument GetArgument(this CliOption option)
        {
            var value = ArgumentProperty.GetValue(option);
            if (value == null)
                throw new NullReferenceException(nameof(ArgumentProperty));

            return (CliArgument)value;
        }

        internal static IList<CliArgument> Arguments(this CliSymbol symbol)
        {
            switch (symbol)
            {
                case CliOption option:
                    return new[]
                    {
                        option.GetArgument()
                    };
                case CliCommand command:
                    return command.Arguments;
                case CliArgument argument:
                    return new[]
                    {
                        argument
                    };
                default:
                    throw new NotSupportedException();
            }
        }

        internal static bool HasArguments(this CliCommand command)
        {
            return command.Arguments.Count > 0;
        }

        internal static bool HasOptions(this CliCommand command)
        {
            return command.Options.Count > 0;
        }

        internal static bool HasSubcommands(this CliCommand command)
        {
            return command.Subcommands.Count > 0;
        }
    }
}
