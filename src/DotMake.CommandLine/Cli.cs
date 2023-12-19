using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.Text;
using System.Threading.Tasks;

namespace DotMake.CommandLine
{
    /// <summary>
    /// Provides methods for parsing command line input and running an indicated command.
    /// </summary>
    public static class Cli
    {
        /// <summary>
        /// Gets a command line builder for the indicated command.
        /// </summary>
        /// <typeparam name="TDefinition">The definition class for the command. A command builder for this class should be automatically generated.</typeparam>
        /// <param name="configureBuilder">A delegate to further configure the command line builder.</param>
        /// <param name="useBuilderDefaults">
        /// Whether to use the default DotMake Cli configuration for the command line builder.
        /// Default is <see langword="true" />.
        /// <para>
        /// Setting this value to <see langword="true" /> is the equivalent to calling:
        /// <code>
        ///   commandLineBuilder
        ///     .UseOutputEncoding(Encoding.UTF8)
        ///     .UseVersionOption(commandBuilder.NamePrefixConvention,
        ///         commandBuilder.ShortFormPrefixConvention,
        ///         commandBuilder.ShortFormAutoGenerate)
        ///     .UseHelp(commandBuilder.NamePrefixConvention)
        ///     .UseHelpBuilder(bindingContext => new CliHelpBuilder(...))
        ///     .UseEnvironmentVariableDirective()
        ///     .UseParseDirective()
        ///     .UseSuggestDirective()
        ///     .RegisterWithDotnetSuggest()
        ///     .UseTypoCorrections()
        ///     .UseParseErrorReporting()
        ///     .CancelOnProcessTermination();
        /// </code>
        /// </para>
        /// </param>
        /// <returns>A <see cref="CommandLineBuilder" /> providing details about command line configurations.</returns>
        public static CommandLineBuilder GetBuilder<TDefinition>(Action<CommandLineBuilder> configureBuilder = null, bool useBuilderDefaults = true)
        {
            var commandBuilder = CliCommandBuilder.Get<TDefinition>();
            var command = commandBuilder.Build();
            var commandLineBuilder = new CommandLineBuilder(command);

            if (useBuilderDefaults)
            {
                commandLineBuilder
                    .UseOutputEncoding(Encoding.UTF8)
                    .UseVersionOption(commandBuilder.NamePrefixConvention,
                        commandBuilder.ShortFormPrefixConvention,
                        commandBuilder.ShortFormAutoGenerate)
                    .UseHelp(commandBuilder.NamePrefixConvention)
                    .UseHelpBuilder(bindingContext => new CliHelpBuilder(bindingContext.ParseResult.CommandResult.LocalizationResources,
                        maxWidth: ConsoleExtensions.GetWindowWidth(null), console: bindingContext.Console))
                    .UseEnvironmentVariableDirective()
                    .UseParseDirective()
                    .UseSuggestDirective()
                    .RegisterWithDotnetSuggest()
                    .UseTypoCorrections()
                    .UseParseErrorReporting()
                    //.UseExceptionHandler() //This registers an exception handler even with null parameter so disabling it to let exceptions go through.
                    .CancelOnProcessTermination();
            }

            if (configureBuilder != null)
                configureBuilder(commandLineBuilder);

            return commandLineBuilder;
        }

        /*
        Note about documentation, VS intellisense wants
            /// <inheritdoc cref="GetBuilder{TDefinition}" path="/param" />
        but SHFB wants (for some reason VS is confused for second parameter, repeats the first one's contents)
            /// <param name="useBuilderDefaults"><inheritdoc cref="GetBuilder{TDefinition}" Path="param/[@name='useBuilderDefaults']/node()" /></param>
        This happens probably the method is generic?
        */

        /// <summary>
        /// Parses a command line string value and runs the handler for the indicated command.
        /// </summary>
        /// <typeparam name="TDefinition">The definition class for the command. A command builder for this class should be automatically generated.</typeparam>
        /// <param name="commandLine">The command line string input will be split into tokens as if it had been passed on the command line.</param>
        /// <inheritdoc cref="GetBuilder{TDefinition}" path="/param" />
        /// <param name="configureBuilder"><inheritdoc cref="GetBuilder{TDefinition}" Path="param/[@name='configureBuilder']/node()" /></param>
        /// <param name="useBuilderDefaults"><inheritdoc cref="GetBuilder{TDefinition}" Path="param/[@name='useBuilderDefaults']/node()" /></param>
        /// <param name="console">A console to which output can be written. By default, <see cref="Console" /> is used.</param>
        /// <returns>The exit code for the invocation.</returns>
        public static int Run<TDefinition>(string commandLine, Action<CommandLineBuilder> configureBuilder = null, bool useBuilderDefaults = true, IConsole console = null)
        {
            var parser = GetBuilder<TDefinition>(configureBuilder, useBuilderDefaults).Build();

            return parser.Invoke(commandLine, console);
        }

        /// <summary>
        /// Parses a command line string array and runs the handler for the indicated command.
        /// </summary>
        /// <typeparam name="TDefinition">The definition class for the command. A command builder for this class should be automatically generated.</typeparam>
        /// <param name="args">The string array typically passed to a program's <c>Main</c> method.</param>
        /// <inheritdoc cref="GetBuilder{TDefinition}" path="/param" />
        /// <param name="configureBuilder"><inheritdoc cref="GetBuilder{TDefinition}" Path="param/[@name='configureBuilder']/node()" /></param>
        /// <param name="useBuilderDefaults"><inheritdoc cref="GetBuilder{TDefinition}" Path="param/[@name='useBuilderDefaults']/node()" /></param>
        /// <param name="console">A console to which output can be written. By default, <see cref="Console" /> is used.</param>
        /// <returns>The exit code for the invocation.</returns>
        public static int Run<TDefinition>(string[] args, Action<CommandLineBuilder> configureBuilder = null, bool useBuilderDefaults = true, IConsole console = null)
        {
            var parser = GetBuilder<TDefinition>(configureBuilder, useBuilderDefaults).Build();

            return parser.Invoke(args, console);
        }

        /// <summary>
        /// Parses a command line string value and runs the handler asynchronously for the indicated command.
        /// </summary>
        /// <typeparam name="TDefinition">The definition class for the command. A command builder for this class should be automatically generated.</typeparam>
        /// <param name="commandLine">The command line string input will be split into tokens as if it had been passed on the command line.</param>
        /// <inheritdoc cref="GetBuilder{TDefinition}" path="/param" />
        /// <param name="configureBuilder"><inheritdoc cref="GetBuilder{TDefinition}" Path="param/[@name='configureBuilder']/node()" /></param>
        /// <param name="useBuilderDefaults"><inheritdoc cref="GetBuilder{TDefinition}" Path="param/[@name='useBuilderDefaults']/node()" /></param>
        /// <param name="console">A console to which output can be written. By default, <see cref="System.Console" /> is used.</param>
        /// <returns>The exit code for the invocation.</returns>
        public static async Task<int> RunAsync<TDefinition>(string commandLine, Action<CommandLineBuilder> configureBuilder = null, bool useBuilderDefaults = true, IConsole console = null)
        {
            var parser = GetBuilder<TDefinition>(configureBuilder, useBuilderDefaults).Build();

            return await parser.InvokeAsync(commandLine, console);
        }

        /// <summary>
        /// Parses a command line string array and runs the handler asynchronously for the indicated command.
        /// </summary>
        /// <typeparam name="TDefinition">The definition class for the command. A command builder for this class should be automatically generated.</typeparam>
        /// <param name="args">The string array typically passed to a program's <c>Main</c> method.</param>
        /// <inheritdoc cref="GetBuilder{TDefinition}" path="/param" />
        /// <param name="configureBuilder"><inheritdoc cref="GetBuilder{TDefinition}" Path="param/[@name='configureBuilder']/node()" /></param>
        /// <param name="useBuilderDefaults"><inheritdoc cref="GetBuilder{TDefinition}" Path="param/[@name='useBuilderDefaults']/node()" /></param>
        /// <param name="console">A console to which output can be written. By default, <see cref="System.Console" /> is used.</param>
        /// <returns>The exit code for the invocation.</returns>
        public static async Task<int> RunAsync<TDefinition>(string[] args, Action<CommandLineBuilder> configureBuilder = null, bool useBuilderDefaults = true, IConsole console = null)
        {
            var parser = GetBuilder<TDefinition>(configureBuilder, useBuilderDefaults).Build();

            return await parser.InvokeAsync(args, console);
        }

        /// <summary>
        /// Parses a command line string, and also provides the parse result.
        /// </summary>
        /// <typeparam name="TDefinition">The definition class for the command. A command builder for this class should be automatically generated.</typeparam>
        /// <param name="commandLine">The command line string input will be split into tokens as if it had been passed on the command line.</param>
        /// <param name="parseResult">A <see cref="ParseResult" /> providing details about the parse operation.</param>
        /// <returns>An instance of the definition class whose properties were bound/populated from the parse result.</returns>
        public static TDefinition Parse<TDefinition>(string commandLine, out ParseResult parseResult)
        {
            var commandBuilder = CliCommandBuilder.Get<TDefinition>();
            var command = commandBuilder.Build();

            parseResult = command.Parse(commandLine);

            return (TDefinition)commandBuilder.Bind(parseResult);
        }

        /// <summary>
        /// Parses a command line string.
        /// </summary>
        /// <typeparam name="TDefinition">The definition class for the command. A command builder for this class should be automatically generated.</typeparam>
        /// <param name="commandLine">The command line string input will be split into tokens as if it had been passed on the command line.</param>
        /// <returns>An instance of the definition class whose properties were bound/populated from the parse result.</returns>
        public static TDefinition Parse<TDefinition>(string commandLine)
        {
            return Parse<TDefinition>(commandLine, out _);
        }

        /// <summary>
        /// Parses a command line string array, and also provides the parse result.
        /// </summary>
        /// <typeparam name="TDefinition">The definition class for the command. A command builder for this class should be automatically generated.</typeparam>
        /// <param name="args">The string array typically passed to a program's <c>Main</c> method.</param>
        /// <param name="parseResult">A <see cref="ParseResult" /> providing details about the parse operation.</param>
        /// <returns>An instance of the definition class whose properties were bound/populated from the parse result.</returns>
        public static TDefinition Parse<TDefinition>(string[] args, out ParseResult parseResult)
        {
            var commandBuilder = CliCommandBuilder.Get<TDefinition>();
            var command = commandBuilder.Build();

            parseResult = command.Parse(args);

            return (TDefinition)commandBuilder.Bind(parseResult);
        }

        /// <summary>
        /// Parses a command line string array, and also provides the parse result.
        /// </summary>
        /// <typeparam name="TDefinition">The definition class for the command. A command builder for this class should be automatically generated.</typeparam>
        /// <param name="args">The string array typically passed to a program's <c>Main</c> method.</param>
        /// <returns>An instance of the definition class whose properties were bound/populated from the parse result.</returns>
        public static TDefinition Parse<TDefinition>(string[] args)
        {
            return Parse<TDefinition>(args, out _);
        }
    }
}
