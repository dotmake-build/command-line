using System;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotMake.CommandLine
{
    /// <summary>
    /// Provides methods for parsing command line input and running an indicated command.
    /// </summary>
    /// <example>
    ///     <code id="gettingStartedDelegate" source="..\TestApp\CliExamples.cs" region="CliRunDelegate" language="cs" />
    ///     <code id="gettingStartedClass">
    ///         <code source="..\TestApp\Commands\RootCliCommand.cs" region="RootCliCommand" language="cs" />
    ///         <code source="..\TestApp\CliExamples.cs" region="CliRun" language="cs" />
    ///         <code source="..\TestApp\CliExamples.cs" region="CliParse" language="cs" />
    ///     </code>
    ///     <code>
    ///         <code source="..\TestApp\CliExamples.cs" region="CliRunWithReturn" language="cs" />
    ///         <code source="..\TestApp\CliExamples.cs" region="CliRunAsync" language="cs" />
    ///         <code source="..\TestApp\CliExamples.cs" region="CliRunAsyncWithReturn" language="cs" />
    ///         <code source="..\TestApp\CliExamples.cs" region="CliParseWithResult" language="cs" />
    ///     </code>
    ///     <code source="..\TestApp\CliExamples.cs" region="CliRunExceptions" language="cs" />
    ///     <code>
    ///         <code source="..\TestApp.NugetDI\Program.cs" region="Namespace" language="cs" />
    ///         <code source="..\TestApp.NugetDI\Program.cs" region="ConfigureServices" language="cs" />
    ///         <code source="..\TestApp.NugetDI\Commands\RootCliCommand.cs" region="RootCliCommand" language="cs" />
    ///     </code>
    /// </example>
    public static class Cli
    {
        /// <summary>
        /// <inheritdoc cref="CliExtensions" path="/summary/node()" />
        /// </summary>
        public static CliExtensions Ext { get; } = new CliExtensions();

        /// <summary>
        /// Returns a string array containing the command-line arguments for the current process.
        /// Uses <see cref="Environment.GetCommandLineArgs"/> but skips the first element which is the executable file name,
        /// so the following zero or more elements that contain the remaining command-line arguments are returned,
        /// i.e. returns the same as the special variable <c>args</c> available in <c>Program.cs</c> (new style with top-level statements)
        /// or as the string array passed to the program's <c>Main</c> method (old style).
        /// </summary>
        /// <returns>An array of strings where each element contains a command-line argument.</returns>
        public static string[] GetArgs()
        {
            if (Environment.GetCommandLineArgs() is { Length: > 0 } args)
                return args.Skip(1).ToArray();

            return Array.Empty<string>();
        }



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
            var definitionType = typeof(TDefinition);

            return GetBuilder(definitionType, configureBuilder, useBuilderDefaults);
        }

        /// <inheritdoc cref = "GetBuilder{TDefinition}" />
        /// <param name="definitionType">The definition class type for the command. A command builder for this class should be automatically generated.</param>
        /// <param name="configureBuilder"><inheritdoc cref="GetBuilder{TDefinition}" path="/param[@name='configureBuilder']/node()" /></param>
        /// <param name="useBuilderDefaults"><inheritdoc cref="GetBuilder{TDefinition}" path="/param[@name='useBuilderDefaults']/node()" /></param>
        public static CommandLineBuilder GetBuilder(Type definitionType, Action<CommandLineBuilder> configureBuilder = null, bool useBuilderDefaults = true)
        {
            var commandBuilder = CliCommandBuilder.Get(definitionType);
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



        /// <summary>
        /// Parses a command line string array and runs the handler for the indicated command.
        /// </summary>
        /// <typeparam name="TDefinition"><inheritdoc cref="GetBuilder{TDefinition}" path="/typeparam[@name='TDefinition']/node()" /></typeparam>
        /// <param name="args">
        /// The string array typically passed to a program. This is usually
        /// the special variable <c>args</c> available in <c>Program.cs</c> (new style with top-level statements)
        /// or the string array passed to the program's <c>Main</c> method (old style).
        /// If not specified or <see langword="null"/>, <c>args</c> will be retrieved automatically from the current process via <see cref="GetArgs"/>.
        /// </param>
        /// <param name="configureBuilder"><inheritdoc cref="GetBuilder{TDefinition}" path="/param[@name='configureBuilder']/node()" /></param>
        /// <param name="useBuilderDefaults"><inheritdoc cref="GetBuilder{TDefinition}" path="/param[@name='useBuilderDefaults']/node()" /></param>
        /// <param name="console">A console to which output can be written. By default, <see cref="Console" /> is used.</param>
        /// <returns>The exit code for the invocation.</returns>
        /// <example>
        ///     <code source="..\TestApp\CliExamples.cs" region="CliRun" language="cs" />
        ///     <code source="..\TestApp\CliExamples.cs" region="CliRunWithReturn" language="cs" />
        /// </example>
        public static int Run<TDefinition>(string[] args = null, Action<CommandLineBuilder> configureBuilder = null, bool useBuilderDefaults = true, IConsole console = null)
        {
            var parser = GetBuilder<TDefinition>(configureBuilder, useBuilderDefaults).Build();

            return parser.Invoke(args ?? GetArgs(), console);
        }

        /// <summary>
        /// Parses a command line string value and runs the handler for the indicated command.
        /// </summary>
        /// <typeparam name="TDefinition"><inheritdoc cref="GetBuilder{TDefinition}" path="/typeparam[@name='TDefinition']/node()" /></typeparam>
        /// <param name="commandLine">The command line string that will be split into tokens as if it had been passed on the command line. Useful for testing command line input by just specifying it as a single string.</param>
        /// <param name="configureBuilder"><inheritdoc cref="GetBuilder{TDefinition}" path="/param[@name='configureBuilder']/node()" /></param>
        /// <param name="useBuilderDefaults"><inheritdoc cref="GetBuilder{TDefinition}" path="/param[@name='useBuilderDefaults']/node()" /></param>
        /// <param name="console"><inheritdoc cref="Run{TDefinition}(string[], Action{CommandLineBuilder}, bool, IConsole)" path="/param[@name='console']/node()" /></param>
        /// <returns><inheritdoc cref="Run{TDefinition}(string[], Action{CommandLineBuilder}, bool, IConsole)" path="/returns/node()" /></returns>
        /// <example>
        ///     <code source="..\TestApp\CliExamples.cs" region="CliRunString" language="cs" />
        ///     <code source="..\TestApp\CliExamples.cs" region="CliRunStringWithReturn" language="cs" />
        /// </example>
        public static int Run<TDefinition>(string commandLine, Action<CommandLineBuilder> configureBuilder = null, bool useBuilderDefaults = true, IConsole console = null)
        {
            var parser = GetBuilder<TDefinition>(configureBuilder, useBuilderDefaults).Build();

            return parser.Invoke(commandLine, console);
        }

        /// <summary>
        /// Parses the command line arguments and runs the indicated command as delegate.
        /// </summary>
        /// <param name="cliCommandAsDelegate">
        /// The command as delegate.
        /// <code>
        /// ([CliArgument] string argument1, bool option1) => { }
        ///
        /// ([CliArgument] string argument1, bool option1) => { return 0; }
        ///
        /// async ([CliArgument] string argument1, bool option1) => { await Task.Delay(1000); }
        /// 
        /// MethodReference
        /// </code>
        /// </param>
        /// <returns><inheritdoc cref="Run{TDefinition}(string[], Action{CommandLineBuilder}, bool, IConsole)" path="/returns/node()" /></returns>
        /// <example>
        ///     <code source="..\TestApp\CliExamples.cs" region="CliRunDelegate" language="cs" />
        ///     <code source="..\TestApp\CliExamples.cs" region="CliRunDelegateWithReturn" language="cs" />
        /// </example>
        public static int Run(Delegate cliCommandAsDelegate)
        {
            var definitionType = CliCommandAsDelegateDefinition.Get(cliCommandAsDelegate);
            var parser = GetBuilder(definitionType).Build();

            return parser.Invoke(GetArgs());
        }



        /// <summary>
        /// Parses a command line string array and runs the handler asynchronously for the indicated command.
        /// </summary>
        /// <typeparam name="TDefinition"><inheritdoc cref="GetBuilder{TDefinition}" path="/typeparam[@name='TDefinition']/node()" /></typeparam>
        /// <param name="args"><inheritdoc cref="Run{TDefinition}(string[], Action{CommandLineBuilder}, bool, IConsole)" path="/param[@name='args']/node()" /></param>
        /// <param name="configureBuilder"><inheritdoc cref="GetBuilder{TDefinition}" path="/param[@name='configureBuilder']/node()" /></param>
        /// <param name="useBuilderDefaults"><inheritdoc cref="GetBuilder{TDefinition}" path="/param[@name='useBuilderDefaults']/node()" /></param>
        /// <param name="console"><inheritdoc cref="Run{TDefinition}(string[], Action{CommandLineBuilder}, bool, IConsole)" path="/param[@name='console']/node()" /></param>
        /// <returns><inheritdoc cref="Run{TDefinition}(string[], Action{CommandLineBuilder}, bool, IConsole)" path="/returns/node()" /></returns>
        /// <example>
        ///     <code source="..\TestApp\CliExamples.cs" region="CliRunAsync" language="cs" />
        ///     <code source="..\TestApp\CliExamples.cs" region="CliRunAsyncWithReturn" language="cs" />
        /// </example>
        public static async Task<int> RunAsync<TDefinition>(string[] args = null, Action<CommandLineBuilder> configureBuilder = null, bool useBuilderDefaults = true, IConsole console = null)
        {
            var parser = GetBuilder<TDefinition>(configureBuilder, useBuilderDefaults).Build();

            return await parser.InvokeAsync(args ?? GetArgs(), console);
        }

        /// <summary>
        /// Parses a command line string value and runs the handler asynchronously for the indicated command.
        /// </summary>
        /// <typeparam name="TDefinition"><inheritdoc cref="GetBuilder{TDefinition}" path="/typeparam[@name='TDefinition']/node()" /></typeparam>
        /// <param name="commandLine"><inheritdoc cref="Run{TDefinition}(string, Action{CommandLineBuilder}, bool, IConsole)" path="/param[@name='commandLine']/node()" /></param>
        /// <param name="configureBuilder"><inheritdoc cref="GetBuilder{TDefinition}" path="/param[@name='configureBuilder']/node()" /></param>
        /// <param name="useBuilderDefaults"><inheritdoc cref="GetBuilder{TDefinition}" path="/param[@name='useBuilderDefaults']/node()" /></param>
        /// <param name="console"><inheritdoc cref="Run{TDefinition}(string[], Action{CommandLineBuilder}, bool, IConsole)" path="/param[@name='console']/node()" /></param>
        /// <returns><inheritdoc cref="Run{TDefinition}(string[], Action{CommandLineBuilder}, bool, IConsole)" path="/returns/node()" /></returns>
        /// <example>
        ///     <code source="..\TestApp\CliExamples.cs" region="CliRunAsyncString" language="cs" />
        ///     <code source="..\TestApp\CliExamples.cs" region="CliRunAsyncStringWithReturn" language="cs" />
        /// </example>
        public static async Task<int> RunAsync<TDefinition>(string commandLine, Action<CommandLineBuilder> configureBuilder = null, bool useBuilderDefaults = true, IConsole console = null)
        {
            var parser = GetBuilder<TDefinition>(configureBuilder, useBuilderDefaults).Build();

            return await parser.InvokeAsync(commandLine, console);
        }

        /// <summary>
        /// Parses the command line arguments and runs the indicated command as delegate.
        /// </summary>
        /// <param name="cliCommandAsDelegate"><inheritdoc cref="Run(Delegate)" path="/param[@name='cliCommandAsDelegate']/node()" /></param>
        /// <returns><inheritdoc cref="Run{TDefinition}(string[], Action{CommandLineBuilder}, bool, IConsole)" path="/returns/node()" /></returns>
        /// <example>
        ///     <code source="..\TestApp\CliExamples.cs" region="CliRunAsyncDelegate" language="cs" />
        ///     <code source="..\TestApp\CliExamples.cs" region="CliRunAsyncDelegateWithReturn" language="cs" />
        /// </example>
        public static async Task<int> RunAsync(Delegate cliCommandAsDelegate)
        {
            var definitionType = CliCommandAsDelegateDefinition.Get(cliCommandAsDelegate);
            var parser = GetBuilder(definitionType).Build();

            return await parser.InvokeAsync(GetArgs());
        }



        /// <summary>
        /// Parses a command line string array, and also provides the parse result.
        /// </summary>
        /// <typeparam name="TDefinition"><inheritdoc cref="GetBuilder{TDefinition}" path="/typeparam[@name='TDefinition']/node()" /></typeparam>
        /// <param name="args"><inheritdoc cref="Run{TDefinition}(string[], Action{CommandLineBuilder}, bool, IConsole)" path="/param[@name='args']/node()" /></param>
        /// <param name="parseResult">A <see cref="ParseResult" /> providing details about the parse operation.</param>
        /// <returns>An instance of the definition class whose properties were bound/populated from the parse result.</returns>
        /// <example>
        ///     <code source="..\TestApp\CliExamples.cs" region="CliParseWithResult" language="cs" />
        /// </example>
        public static TDefinition Parse<TDefinition>(string[] args, out ParseResult parseResult)
        {
            var commandBuilder = CliCommandBuilder.Get<TDefinition>();
            var command = commandBuilder.Build();

            parseResult = command.Parse(args ?? GetArgs());

            return (TDefinition)commandBuilder.Bind(parseResult);
        }

        /// <summary>
        /// Parses a command line string array.
        /// </summary>
        /// <typeparam name="TDefinition"><inheritdoc cref="GetBuilder{TDefinition}" path="/typeparam[@name='TDefinition']/node()" /></typeparam>
        /// <param name="args"><inheritdoc cref="Run{TDefinition}(string[], Action{CommandLineBuilder}, bool, IConsole)" path="/param[@name='args']/node()" /></param>
        /// <returns><inheritdoc cref="Parse{TDefinition}(string[], out ParseResult)" path="/returns/node()" /></returns>
        /// <example>
        ///     <code source="..\TestApp\CliExamples.cs" region="CliParse" language="cs" />
        /// </example>
        public static TDefinition Parse<TDefinition>(string[] args = null)
        {
            return Parse<TDefinition>(args, out _);
        }
        
        /// <summary>
        /// Parses the command-line arguments for the current process, and also provides the parse result
        /// (<c>args</c> will be retrieved automatically from the current process via <see cref="GetArgs"/>).
        /// </summary>
        /// <typeparam name="TDefinition"><inheritdoc cref="GetBuilder{TDefinition}" path="/typeparam[@name='TDefinition']/node()" /></typeparam>
        /// <param name="parseResult"><inheritdoc cref="Parse{TDefinition}(string[], out ParseResult)" path="/param[@name='parseResult']/node()" /></param>
        /// <returns><inheritdoc cref="Parse{TDefinition}(string[], out ParseResult)" path="/returns/node()" /></returns>
        /// <example>
        ///     <code source="..\TestApp\CliExamples.cs" region="CliParseWithResult" language="cs" />
        /// </example>
        public static TDefinition Parse<TDefinition>(out ParseResult parseResult)
        {
            return Parse<TDefinition>(GetArgs(), out parseResult);
        }

        /// <summary>
        /// Parses a command line string, and also provides the parse result.
        /// </summary>
        /// <typeparam name="TDefinition"><inheritdoc cref="GetBuilder{TDefinition}" path="/typeparam[@name='TDefinition']/node()" /></typeparam>
        /// <param name="commandLine"><inheritdoc cref="Run{TDefinition}(string, Action{CommandLineBuilder}, bool, IConsole)" path="/param[@name='commandLine']/node()" /></param>
        /// <param name="parseResult">A <see cref="ParseResult" /> providing details about the parse operation.</param>
        /// <returns><inheritdoc cref="Parse{TDefinition}(string[], out ParseResult)" path="/returns/node()" /></returns>
        /// <example>
        ///     <code source="..\TestApp\CliExamples.cs" region="CliParseStringWithResult" language="cs" />
        /// </example>
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
        /// <typeparam name="TDefinition"><inheritdoc cref="GetBuilder{TDefinition}" path="/typeparam[@name='TDefinition']/node()" /></typeparam>
        /// <param name="commandLine"><inheritdoc cref="Run{TDefinition}(string, Action{CommandLineBuilder}, bool, IConsole)" path="/param[@name='commandLine']/node()" /></param>
        /// <returns><inheritdoc cref="Parse{TDefinition}(string[], out ParseResult)" path="/returns/node()" /></returns>
        /// <example>
        ///     <code source="..\TestApp\CliExamples.cs" region="CliParseString" language="cs" />
        /// </example>
        public static TDefinition Parse<TDefinition>(string commandLine)
        {
            return Parse<TDefinition>(commandLine, out _);
        }
    }
}
