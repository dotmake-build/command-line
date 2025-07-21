using System;
using System.CommandLine;
using System.CommandLine.Completions;
using System.CommandLine.Help;
using System.CommandLine.Invocation;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using DotMake.CommandLine.Help;
using DotMake.CommandLine.Util;

namespace DotMake.CommandLine
{
    /// <summary>
    /// Represents a CLI parser configured for a specific command with grammar and behaviors.
    /// </summary>
    public class CliParser
    {
        private readonly CliBindingContext bindingContext = new();
        private readonly CliSettings settings;
        private readonly CommandLineConfiguration configuration;

        internal CliParser(Type definitionType, CliSettings settings = null)
        {
            var commandBuilder = CliCommandBuilder.Get(definitionType);
            var command = commandBuilder.BuildWithHierarchy(bindingContext, out var rootCommand);

            settings ??= new CliSettings();
            this.settings = settings;

            configuration = new CommandLineConfiguration(command)
            {
                EnablePosixBundling = settings.EnablePosixBundling,
                EnableDefaultExceptionHandler = settings.EnableDefaultExceptionHandler,
                ProcessTerminationTimeout = settings.ProcessTerminationTimeout,
                ResponseFileTokenReplacer = settings.ResponseFileTokenReplacer
            };
            if (settings.Output != null) //Console.Out is NOT being used
                configuration.Output = settings.Output;
            if (settings.Error != null) //Console.Error is NOT being used
                configuration.Error = settings.Error;

            if (rootCommand != null)
            {
                //CliRootCommand constructor already adds HelpOption and VersionOption so remove them
                foreach (var option in rootCommand.Options.Where(option => option is HelpOption or VersionOption).ToArray())
                    rootCommand.Options.Remove(option);

                var namePrefixConvention = commandBuilder.NamePrefixConvention ?? CliCommandAttribute.Default.NamePrefixConvention;
                var shortFormPrefixConvention = commandBuilder.ShortFormPrefixConvention ?? CliCommandAttribute.Default.ShortFormPrefixConvention;
                var shortFormAutoGenerate = commandBuilder.ShortFormAutoGenerate ?? CliCommandAttribute.Default.ShortFormAutoGenerate;

                var helpOption = new HelpOption(
                    CliStringUtil.AddPrefix("help", namePrefixConvention),
                    //Regardless of convention, add all short form aliases as help is a special option
                    "-h", "/h", "-?", "/?"
                )
                {
                    Action = new CustomHelpAction
                    {
                        Builder = new CliHelpBuilder(settings.Theme, Console.IsOutputRedirected ? int.MaxValue : Console.WindowWidth)
                    }
                };
                rootCommand.Options.Add(helpOption);

                var versionOption = new VersionOption(
                    CliStringUtil.AddPrefix("version", namePrefixConvention),
                    (shortFormAutoGenerate.HasFlag(CliNameAutoGenerate.Options))
                        ? new[] { CliStringUtil.AddPrefix("v", shortFormPrefixConvention) }
                        : Array.Empty<string>()
                )
                {
                    Action = new VersionOptionAction()
                };
                rootCommand.Options.Add(versionOption);

                //CliRootCommand constructor already adds SuggestDirective so remove it
                foreach (var directive in rootCommand.Directives.Where(directive => directive is SuggestDirective).ToArray())
                    rootCommand.Directives.Remove(directive);

                if (settings.EnableSuggestDirective)
                    rootCommand.Directives.Add(new SuggestDirective());
                if (settings.EnableDiagramDirective)
                    rootCommand.Directives.Add(new DiagramDirective());
                if (settings.EnableEnvironmentVariablesDirective)
                    rootCommand.Directives.Add(new EnvironmentVariablesDirective());
            }
        }


        /// <summary>
        /// Parses a command line string array and returns the parse result for the indicated command.
        /// </summary>
        /// <param name="args">
        /// The string array typically passed to a program. This is usually
        /// the special variable <c>args</c> available in <c>Program.cs</c> (new style with top-level statements)
        /// or the string array passed to the program's <c>Main</c> method (old style).
        /// If not specified or <see langword="null"/>, <c>args</c> will be retrieved automatically from the current process via <see cref="GetArgs"/>.
        /// </param>
        /// <returns>A <see cref="CliResult" /> providing details about the parse operation and methods for binding.</returns>
        /*
        /// <example>
        ///     <code source="..\TestApp\CliExamples.cs" region="CliParseWithResult" language="cs" />
        /// </example>
        */
        public CliResult Parse(string[] args = null)
        {
            var parseResult = configuration.Parse(FixArgs(args) ?? GetArgs());
            return new CliResult(bindingContext, parseResult);
        }

        /// <summary>
        /// Parses a command line string and returns the parse result for the indicated command.
        /// </summary>
        /// <param name="commandLine">The command line string that will be split into tokens as if it had been passed on the command line. Useful for testing command line input by just specifying it as a single string.</param>
        /// <returns><inheritdoc cref="Parse(string[])" path="/returns/node()" /></returns>
        /*
        /// <example>
        ///     <code source="..\TestApp\CliExamples.cs" region="CliParseStringWithResult" language="cs" />
        /// </example>
        */
        public CliResult Parse(string commandLine)
        {
            var parseResult = configuration.Parse(commandLine);
            return new CliResult(bindingContext, parseResult);
        }


        /// <summary>
        /// Parses a command line string array and runs the handler for the indicated command.
        /// </summary>
        /// <param name="args"><inheritdoc cref="Parse(string[])" path="/param[@name='args']/node()" /></param>
        /// <returns>The exit code for the invocation.</returns>
        /*
        /// <example>
        ///     <code source="..\TestApp\CliExamples.cs" region="CliRun" language="cs" />
        ///     <code source="..\TestApp\CliExamples.cs" region="CliRunWithReturn" language="cs" />
        /// </example>
        */
        public int Run(string[] args = null)
        {
            using (new CliSession(settings))
                return configuration.Invoke(FixArgs(args) ?? GetArgs());
        }

        /// <summary>
        /// Parses a command line string value and runs the handler for the indicated command.
        /// </summary>
        /// <param name="commandLine"><inheritdoc cref="Parse(string)" path="/param[@name='commandLine']/node()" /></param>
        /// <returns><inheritdoc cref="Run(string[])" path="/returns/node()" /></returns>
        /*
        /// <example>
        ///     <code source="..\TestApp\CliExamples.cs" region="CliRunString" language="cs" />
        ///     <code source="..\TestApp\CliExamples.cs" region="CliRunStringWithReturn" language="cs" />
        /// </example>
        */
        public int Run(string commandLine)
        {
            using (new CliSession(settings))
                return configuration.Invoke(commandLine);
        }

        /// <summary>
        /// Parses a command line string array and runs the handler asynchronously for the indicated command.
        /// </summary>
        /// <param name="args"><inheritdoc cref="Parse(string[])" path="/param[@name='args']/node()" /></param>
        /// <param name="cancellationToken">The token to monitor for cancellation requests.</param>
        /// <returns><inheritdoc cref="Run(string[])" path="/returns/node()" /></returns>
        /*
        /// <example>
        ///     <code source="..\TestApp\CliExamples.cs" region="CliRunAsync" language="cs" />
        ///     <code source="..\TestApp\CliExamples.cs" region="CliRunAsyncWithReturn" language="cs" />
        /// </example>
        */
        public async Task<int> RunAsync(string[] args = null, CancellationToken cancellationToken = default)
        {
            using (new CliSession(settings))
                return await configuration.InvokeAsync(FixArgs(args) ?? GetArgs(), cancellationToken);
        }

        /// <summary>
        /// Parses a command line string value and runs the handler asynchronously for the indicated command.
        /// </summary>
        /// <param name="commandLine"><inheritdoc cref="Parse(string)" path="/param[@name='commandLine']/node()" /></param>
        /// <param name="cancellationToken"><inheritdoc cref="RunAsync(string[], CancellationToken)" path="/param[@name='cancellationToken']/node()" /></param>
        /// <returns><inheritdoc cref="Run(string[])" path="/returns/node()" /></returns>
        /*
        /// <example>
        ///     <code source="..\TestApp\CliExamples.cs" region="CliRunAsyncString" language="cs" />
        ///     <code source="..\TestApp\CliExamples.cs" region="CliRunAsyncStringWithReturn" language="cs" />
        /// </example>
        */
        public async Task<int> RunAsync(string commandLine, CancellationToken cancellationToken = default)
        {
            using (new CliSession(settings))
                return await configuration.InvokeAsync(commandLine, cancellationToken);
        }


        /// <summary>
        /// Returns a string array containing the command-line arguments for the current process.
        /// <para>
        /// Uses <see cref="Environment.GetCommandLineArgs"/> but skips the first element which is the executable file name,
        /// so the following zero or more elements that contain the remaining command-line arguments are returned,
        /// i.e. returns the same as the special variable <c>args</c> available in <c>Program.cs</c> (new style with top-level statements)
        /// or as the string array passed to the program's <c>Main</c> method (old style).
        /// </para>
        /// <para>Also on Windows platform, backslash + double quote (<c>\&quot;</c>) at the end of an argument,
        /// is usually a path separator and not an escape for double quote, so it will be trimmed to prevent unnecessary path errors.</para>
        /// </summary>
        /// <returns>An array of strings where each element contains a command-line argument.</returns>
        internal static string[] GetArgs()
        {
            if (Environment.GetCommandLineArgs() is { Length: > 0 } args)
                return FixArgs(args.Skip(1).ToArray());

            return Array.Empty<string>();
        }

        private static string[] FixArgs(string[] args)
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows)
                && args != null)
            {
                /*
                  On Windows, trim ending double quote:
                  For example, if a path parameter is passed like this:
                   --source "C:\myfiles\"
                  The value comes as
                   C:\myfiles"
                  due to CommandLineToArgvW reading backslash as an escape for double quote character.
                  As on Windows, backslash at the end is usually a path separator, trim it to prevent unnecessary errors.
                  Note that this is not required for commandLine string as in that case SplitCommandLine is used,
                  and it already trims double quote characters

                  https://devblogs.microsoft.com/oldnewthing/20100917-00/?p=12833
                  https://github.com/dotnet/command-line-api/issues/2334
                  https://github.com/dotnet/command-line-api/issues/2276
                  https://github.com/dotnet/command-line-api/issues/354
                */
                for (var index = 0; index < args.Length; index++)
                {
                    args[index] = args[index].TrimEnd('"');
                }
            }

            return args;
        }

        private sealed class VersionOptionAction : SynchronousCommandLineAction
        {
            public override int Invoke(ParseResult parseResult)
            {
                parseResult.Configuration.Output.WriteLine(ExecutableInfo.Version);
                return 0;
            }
        }
    }
}
