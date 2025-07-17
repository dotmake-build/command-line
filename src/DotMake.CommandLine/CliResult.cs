using System;
using System.CommandLine;

namespace DotMake.CommandLine
{
    /// <summary>
    /// Describes the results of parsing a command line input based on a specific parser configuration
    /// and provides methods for binding the result to definition classes.
    /// </summary>
    public class CliResult
    {
        private readonly CliBindingContext bindingContext;

        internal CliResult(CliBindingContext bindingContext, ParseResult parseResult)
        {
            this.bindingContext = bindingContext;
            ParseResult = parseResult;
        }

        /// <summary>
        /// Get the results of parsing a command line input based on a specific parser configuration.
        /// </summary>
        public ParseResult ParseResult { get; }

        /// <inheritdoc cref="CliBindingContext.Bind{TDefinition}" />
        public TDefinition Bind<TDefinition>()
        {
            return bindingContext.Bind<TDefinition>(ParseResult);
        }

        /// <inheritdoc cref="CliBindingContext.Bind(ParseResult, Type)" />
        public object Bind(Type definitionType)
        {
            return bindingContext.Bind(ParseResult, definitionType);
        }
    }
}
