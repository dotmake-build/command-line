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
        private int maxWidth = -1;

        /// <summary>
        /// The maximum width in characters after which help output is wrapped.
        /// </summary>
        /// <remarks>It defaults to <see cref="Console.WindowWidth"/> if output is not redirected.</remarks>
        public int MaxWidth
        {
            get
            {
                if (maxWidth < 0)
                {
                    try
                    {
                        maxWidth = Console.IsOutputRedirected ? int.MaxValue : Console.WindowWidth;
                    }
                    catch (Exception)
                    {
                        maxWidth = int.MaxValue;
                    }
                }

                return maxWidth;
            }
            set
            {
                if (value <= 0)
                {
                    throw new ArgumentOutOfRangeException(nameof(value));
                }

                maxWidth = value;
            }
        }

        /// <summary>
        /// Specifies an <see cref="Builder"/> to be used to format help output when help is requested.
        /// </summary>
        public HelpBuilder Builder
        {
            get => builder ??= new HelpBuilder(MaxWidth);
            set => builder = value ?? throw new ArgumentNullException(nameof(value));
        }

        /// <inheritdoc />
        public override int Invoke(ParseResult parseResult)
        {
            var output = parseResult.InvocationConfiguration.Output;

            var helpContext = new HelpContext(Builder,
                parseResult.CommandResult.Command,
                output,
                parseResult);

            Builder.Write(helpContext);

            return 0;
        }

        //Important: After 2.0.0-rc.1.25451.107 this new property to should be set to true for custom actions like Help and Version
        /// <inheritdoc />
        public override bool ClearsParseErrors => true;
    }
}
