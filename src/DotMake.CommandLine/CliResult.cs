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

        /// <summary>
        /// Gets a value indicating whether called command is specified with any arguments or options.
        /// <para>
        /// Note that this may return <see langword="false"/> even if any arguments or options were specified for parent commands.
        /// because only arguments or options specified for the called command, are checked.
        /// </para>
        /// <para>
        /// Note that arguments and options should be optional, if they are required (no default values),
        /// then handler will not run and missing error message will be shown.
        /// </para>
        /// </summary>
        /// <returns><see langword="true"/> if called command is specified with any arguments or options, <see langword="false"/> if not.</returns>
        //For RootCommandResult, Tokens does not include directives but Children does
        //So use !CommandResult.Children.Any() instead of CommandResult.Tokens.Count == 0
        public bool HasArgs => (ParseResult.CommandResult.Tokens.Count > 0);

        /// <summary>
        /// Gets a value indicating whether root command is specified with any subcommands, directives, options or arguments.
        /// <para>
        /// Note that arguments and options should be optional, if they are required (no default values),
        /// then handler will not run and missing error message will be shown.
        /// </para>
        /// </summary>
        /// <returns><see langword="true"/> if root command is specified with any subcommands, directives, options or arguments, <see langword="false"/> if not.</returns>
        public bool HasTokens => (ParseResult.Tokens.Count > 0);


        /// <inheritdoc cref="CliBindingContext.Create{TDefinition}" />
        public TDefinition Create<TDefinition>()
        {
            return bindingContext.Create<TDefinition>();
        }

        /// <inheritdoc cref="CliBindingContext.Create" />
        public object Create(Type definitionType)
        {
            return bindingContext.Create(definitionType);
        }


        /// <inheritdoc cref="CliBindingContext.Bind{TDefinition}" />
        public TDefinition Bind<TDefinition>(bool returnEmpty = false)
        {
            return bindingContext.Bind<TDefinition>(ParseResult, returnEmpty);
        }

        /// <inheritdoc cref="CliBindingContext.Bind" />
        public object Bind(Type definitionType, bool returnEmpty = false)
        {
            return bindingContext.Bind(ParseResult, definitionType, returnEmpty);
        }
        

        /// <inheritdoc cref="CliBindingContext.BindCalled" />
        public object BindCalled()
        {
            return bindingContext.BindCalled(ParseResult);
        }

        /// <inheritdoc cref="CliBindingContext.BindAll" />
        public object[] BindAll()
        {
            return bindingContext.BindAll(ParseResult);
        }


        /// <inheritdoc cref="CliBindingContext.IsCalled{TDefinition}" />
        public bool IsCalled<TDefinition>()
        {
            return bindingContext.IsCalled<TDefinition>(ParseResult);
        }

        /// <inheritdoc cref="CliBindingContext.IsCalled" />
        public bool IsCalled(Type definitionType)
        {
            return bindingContext.IsCalled(ParseResult, definitionType);
        }

        /// <inheritdoc cref="CliBindingContext.Contains{TDefinition}" />
        public bool Contains<TDefinition>()
        {
            return bindingContext.Contains<TDefinition>(ParseResult);
        }

        /// <inheritdoc cref="CliBindingContext.Contains" />
        public bool Contains(Type definitionType)
        {
            return bindingContext.Contains(ParseResult, definitionType);
        }
    }
}
