using System;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.IO;

namespace DotMake.CommandLine
{
    /// <summary>
    /// Represents the settings used by the <see cref="Cli" />.
    /// </summary>
    public class CliSettings
    {
        /// <summary>
        /// Enables the parser to recognize and expand POSIX-style bundled options.
        /// <para>Default is <see langword="true" />.</para>
        /// </summary>
        /// <param name="value"><see langword="true" /> to parse POSIX bundles; otherwise, <see langword="false" />.</param>
        /// <remarks>
        /// POSIX conventions recommend that single-character options be allowed to be specified together after a single <c>-</c> prefix. When <see cref="P:System.CommandLine.CliConfiguration.EnablePosixBundling" /> is set to <see langword="true" />, the following command lines are equivalent:
        ///
        /// <code>
        /// &gt; myapp -a -b -c
        /// &gt; myapp -abc
        /// </code>
        ///
        /// If an argument is provided after an option bundle, it applies to the last option in the bundle.
        /// When <see cref="P:System.CommandLine.CliConfiguration.EnablePosixBundling" /> is set to <see langword="true" />,
        /// all of the following command lines are equivalent:
        /// <code>
        /// &gt; myapp -a -b -c arg
        /// &gt; myapp -abc arg
        /// &gt; myapp -abcarg
        /// </code>
        ///
        /// </remarks>
        public bool EnablePosixBundling { get; set; } = true;

        /// <summary>
        /// Enables a default exception handler to catch any unhandled exceptions thrown during invocation.
        /// Default exception handler prints the exception in red color to console.
        /// <para>Default is <see langword="false" />.</para>
        /// </summary>
        public bool EnableDefaultExceptionHandler { get; set; }

        /// <summary>
        /// Enables the use of the <c>[suggest]</c> directive which when specified in command line input short circuits normal
        /// command handling and writes a newline-delimited list of suggestions suitable for use by most shells to provide command line completions.
        /// <para>Default is <see langword="true" />.</para>
        /// </summary>
        /// <remarks>The <c>dotnet-suggest</c> tool requires the suggest directive to be enabled for an application to provide completions.</remarks>
        public bool EnableSuggestDirective { get; set; } = true;

        /// <summary>
        /// Enables the use of the <c>[diagram]</c> directive, which when specified on the command line will short 
        /// circuit normal command handling and display a diagram explaining the parse result for the command line input.
        /// <para>Default is <see langword="false" />.</para>
        /// </summary>
        public bool EnableDiagramDirective { get; set; }

        /// <summary>
        /// Enables the use of the <c>[env:key=value]</c> directive, allowing environment variables to be set from the command line during invocation.
        /// <para>Default is <see langword="false" />.</para>
        /// </summary>
        public bool EnableEnvironmentVariablesDirective { get; set; }

        /// <summary>
        /// Enables signaling and handling of process termination (Ctrl+C, SIGINT, SIGTERM) via a <see cref="T:System.Threading.CancellationToken" /> 
        /// that can be passed to a <see cref="T:System.CommandLine.Invocation.CliAction" /> during invocation.
        /// If not provided, a default timeout of 2 seconds is enforced.
        /// </summary>
        public TimeSpan? ProcessTerminationTimeout { get; set; } = TimeSpan.FromSeconds(2.0);

        /// <summary>
        /// Gets or sets the response file token replacer, enabled by default.
        /// To disable response files support, this property needs to be set to null.
        /// </summary>
        /// <remarks>
        /// When enabled, any token prefixed with <c>@</c> can be replaced with zero or more other tokens.
        /// This is mostly commonly used to expand tokens from response files and interpolate them into a command line prior to parsing.
        /// </remarks>
        public TryReplaceToken ResponseFileTokenReplacer { get; set; } = DefaultConfiguration.ResponseFileTokenReplacer;

        /// <summary>
        /// Gets or sets the standard output. Used by Help and other facilities that write non-error information.
        /// <para>Default is <see langword="null" /> which means <see cref="P:System.Console.Out" /> with encoding set to UTF8, will be used.</para>
        /// For testing purposes, it can be set to a new instance of <see cref="T:System.IO.StringWriter" />.
        /// If you want to disable the output, please set it to <see cref="F:System.IO.TextWriter.Null" />.
        /// </summary>
        public TextWriter Output { get; set; }

        /// <summary>
        /// Gets or sets the standard error. Used for printing error information like parse errors.
        /// <para>Default is <see langword="null" /> which means <see cref="P:System.Console.Error" />, will be used.</para>
        /// For testing purposes, it can be set to a new instance of <see cref="T:System.IO.StringWriter" />.
        /// If you want to disable the output, please set it to <see cref="F:System.IO.TextWriter.Null" />.
        /// </summary>
        public TextWriter Error { get; set; }

        /// <summary>
        /// Gets or sets the theme used by the <see cref="Cli" />. These color and formatting option are mainly used by the help output.
        /// <para>Default is <see  cref="CliTheme.Default"/>.</para>
        /// </summary>
        public CliTheme Theme { get; set; } = CliTheme.Default;

        private static readonly CommandLineConfiguration DefaultConfiguration = new CommandLineConfiguration(new RootCommand());
    }
}
