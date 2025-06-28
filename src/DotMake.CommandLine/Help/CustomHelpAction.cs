using System;
using System.CommandLine;
using System.CommandLine.Invocation;

#nullable enable

namespace DotMake.CommandLine.Help
{
    /// <summary>
    /// Provides command line help.
    /// </summary>
    public sealed class CustomHelpAction : SynchronousCommandLineAction
    {
        private HelpBuilder? builder;

        /// <summary>
        /// Specifies an <see cref="Builder"/> to be used to format help output when help is requested.
        /// </summary>
        public HelpBuilder Builder
        {
            get => builder ??= new HelpBuilder(Console.IsOutputRedirected ? int.MaxValue : Console.WindowWidth);
            set => builder = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <inheritdoc />
        public override int Invoke(ParseResult parseResult)
        {
            var output = parseResult.Configuration.Output;

            var helpContext = new HelpContext(Builder,
                parseResult.CommandResult.Command,
                output,
                parseResult);

            Builder.Write(helpContext);

            return 0;
        }
    }
}
