using System;
using System.Collections.Generic;
using System.CommandLine;
using System.Reflection;

namespace DotMake.CommandLine
{
    internal static class CliSymbolExtensions
    {
        private static readonly PropertyInfo ArgumentProperty =
            typeof(Option).GetProperty("Argument", BindingFlags.Instance | BindingFlags.NonPublic);

        public static Argument GetArgument(this Option option)
        {
            var value = ArgumentProperty.GetValue(option);
            if (value == null)
                throw new NullReferenceException(nameof(ArgumentProperty));

            return (Argument)value;
        }

        internal static IList<Argument> Arguments(this Symbol symbol)
        {
            switch (symbol)
            {
                case Option option:
                    return new[]
                    {
                        option.GetArgument()
                    };
                case Command command:
                    return command.Arguments;
                case Argument argument:
                    return new[]
                    {
                        argument
                    };
                default:
                    throw new NotSupportedException();
            }
        }

        internal static bool HasArguments(this Command command)
        {
            return command.Arguments.Count > 0;
        }

        internal static bool HasOptions(this Command command)
        {
            return command.Options.Count > 0;
        }

        internal static bool HasSubcommands(this Command command)
        {
            return command.Subcommands.Count > 0;
        }
    }
}
