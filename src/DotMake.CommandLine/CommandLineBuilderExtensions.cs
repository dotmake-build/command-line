using System.Collections.Generic;
using System.CommandLine.Builder;
using System.CommandLine.Invocation;
using System.Text;

namespace DotMake.CommandLine
{
    /// <summary>
    /// Provides extension methods for <see cref="CommandLineBuilder" />.
    /// </summary>
    public static class CommandLineBuilderExtensions
    {
        /// <summary>
        /// Enables the use of the version option (defaulting to the alias <c>--version</c>) which when specified in command line input will short circuit normal command handling and instead write out version information before exiting.
        /// </summary>
        /// <param name="commandLineBuilder">A command line builder.</param>
        /// <param name="namePrefixConvention">The prefix convention to use for the option name.</param>
        /// <param name="shortFormPrefixConvention">The prefix convention to use for the short form option aliases.</param>
        /// <param name="shortFormAutoGenerate">A value which indicates whether short form aliases are added for the option.</param>
        /// <returns>The same instance of <see cref="CommandLineBuilder" />.</returns>
        public static CommandLineBuilder UseVersionOption(this CommandLineBuilder commandLineBuilder,
            CliNamePrefixConvention namePrefixConvention, CliNamePrefixConvention shortFormPrefixConvention,
            bool shortFormAutoGenerate)
        {
            var aliases = new List<string>
            {
                "version".AddPrefix(namePrefixConvention)
            };

            if (shortFormAutoGenerate)
                aliases.Add("v".AddPrefix(shortFormPrefixConvention));

            return commandLineBuilder.UseVersionOption(aliases.ToArray());
        }

        /// <summary>
        /// Configures the application to show help when one of the following options are specified on the command line:
        /// <code>
        ///    -h
        ///    /h
        ///    --help
        ///    -?
        ///    /?
        /// </code>
        /// </summary>
        /// <param name="commandLineBuilder">A command line builder.</param>
        /// <param name="namePrefixConvention">The prefix convention to use for the option name.</param>
        /// <returns>The same instance of <see cref="CommandLineBuilder" />.</returns>
        public static CommandLineBuilder UseHelp(this CommandLineBuilder commandLineBuilder,
            CliNamePrefixConvention namePrefixConvention)
        {
            var aliases = new[]
            {
                "help".AddPrefix(namePrefixConvention),
				//Regardless of convention, add all short-form aliases as help is a special option
				"-h",
                "/h",
                "-?",
                "/?"
            };

            return commandLineBuilder.UseHelp(aliases);
        }

        /// <summary>
        /// Configures the application to use a specific console output encoding.
        /// </summary>
        /// <param name="commandLineBuilder">A command line builder.</param>
        /// <param name="encoding">The output encoding to use.</param>
        /// <returns>The same instance of <see cref="CommandLineBuilder" />.</returns>
        public static CommandLineBuilder UseOutputEncoding(this CommandLineBuilder commandLineBuilder, Encoding encoding)
        {
            return commandLineBuilder.AddMiddleware(
                invocationContext => invocationContext.Console.SetOutputEncoding(encoding),
                MiddlewareOrder.Configuration
            );
        }
    }
}
