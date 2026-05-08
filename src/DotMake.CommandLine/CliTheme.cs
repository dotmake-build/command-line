using System;

namespace DotMake.CommandLine
{
    /// <summary>
    /// Represents the theme used by the <see cref="Cli" />. These color and formatting option are mainly used by the help output.
    /// </summary>
    public class CliTheme
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="CliTheme" /> class.
        /// </summary>
        public CliTheme()
        {

        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CliTheme" /> class with a base theme to override.
        /// </summary>
        /// <param name="baseTheme">The base theme to override.</param>
        public CliTheme(CliTheme baseTheme)
        {
            DefaultStyle = baseTheme.DefaultStyle;
            SynopsisStyle = baseTheme.SynopsisStyle;
            HeadingStyle = baseTheme.HeadingStyle;
            HeadingCasing = baseTheme.HeadingCasing;
            HeadingNoColon = baseTheme.HeadingNoColon;
            FirstColumnStyle = baseTheme.FirstColumnStyle;
            SecondColumnStyle = baseTheme.SecondColumnStyle;
        }

        /// <summary>
        /// Gets or sets the default style (color and text decoration) used by the app.
        /// <para>Default is <see langword="null"/>, which means terminal's current style.</para>
        /// </summary>
        public CliStyle DefaultStyle { get; init; } = new();

        /// <summary>
        /// Gets or sets the style (color and text decoration) used for the synopsis section in help output.
        /// <para>Default is <see langword="null"/> which also means <see cref="DefaultStyle"/>.</para>
        /// <para>Synopsis section is similar to:</para>
        /// <code language="console">
        /// DotMake Command-Line TestApp v1.6.0
        /// Copyright © 2023-2024 DotMake
        ///
        /// A root cli command with nested children
        /// </code>
        /// </summary>
        public CliStyle? SynopsisStyle { get; init; }

        /// <summary>
        /// Gets or sets the style (color and text decoration) used for a heading in help output.
        /// <para>Default is <see langword="null"/> which also means <see cref="DefaultStyle"/>.</para>
        /// <para>Heading is similar to:</para>
        /// <code language="console">
        /// Usage:
        /// Arguments:
        /// Options:
        /// Commands:
        /// </code>
        /// </summary>
        public CliStyle? HeadingStyle { get; init; }

        /// <summary>
        /// Gets or sets the casing used for a heading in help output.
        /// <para>Default is <see cref="CliNameCasingConvention.None"/>.</para>
        /// <para>For example, uppercase heading is similar to:</para>
        /// <code language="console">
        /// USAGE:
        /// ARGUMENTS:
        /// OPTIONS:
        /// COMMANDS:
        /// </code>
        /// </summary>
        public CliNameCasingConvention HeadingCasing { get; init; }

        /// <summary>
        /// Gets or sets whether colon character at the end, is used for a heading in help output.
        /// <para>Default is <see langword="false"/>.</para>
        /// <para>For example, no colon and uppercase heading is similar to:</para>
        /// <code language="console">
        /// USAGE
        /// ARGUMENTS
        /// OPTIONS
        /// COMMANDS
        /// </code>
        /// </summary>
        public bool HeadingNoColon { get; init; }

        /// <summary>
        /// Gets or sets the style (color and text decoration) used for a first column in help output.
        /// <para>Default is <see langword="null"/> which also means <see cref="DefaultStyle"/>.</para>
        /// <para>First column is similar to:</para>
        /// <code language="console">
        ///   &lt;argument-1&gt;
        ///
        ///   -o, --option-1 &lt;option-1&gt;
        ///   -v, --version
        ///   -?, -h, --help
        ///
        ///   sub-command
        /// </code>
        /// </summary>
        public CliStyle? FirstColumnStyle { get; init; }

        /// <summary>
        /// Gets or sets the style (color and text decoration) used for a second column in help output.
        /// <para>Default is <see langword="null"/> which also means <see cref="DefaultStyle"/>.</para>
        /// <para>Second column is similar to:</para>
        /// <code language="console">
        ///                       Description for Argument1 [required]
        ///
        ///                       Description for Option1 [default: DefaultForOption1]
        ///                       Show version information
        ///                       Show help and usage information
        ///
        ///                       A nested level 1 sub-command
        /// </code>
        /// </summary>
        public CliStyle? SecondColumnStyle { get; init; }

        #region Static

        /// <summary>Gets the no-color theme.</summary>
        public static CliTheme NoColor { get; } = new();

        /// <summary>Gets the default theme.</summary>
        public static CliTheme Default { get; } = new()
        {
            FirstColumnStyle = new CliStyle().Decoration(CliDecoration.Bold)
        };

        /// <summary>Gets the Red theme.</summary>
        public static CliTheme Red { get; } = new()
        {
            HeadingStyle = new CliStyle().Foreground(ConsoleColor.Magenta),
            FirstColumnStyle = new CliStyle().Foreground(ConsoleColor.Red)
        };

        /// <summary>Gets the Dark Red theme.</summary>
        public static CliTheme DarkRed { get; } = new()
        {
            DefaultStyle = new CliStyle().Decoration(CliDecoration.Dim),
            HeadingStyle = new CliStyle().Foreground(ConsoleColor.DarkMagenta),
            FirstColumnStyle = new CliStyle().Foreground(ConsoleColor.DarkRed)
        };

        /// <summary>Gets the Green theme.</summary>
        public static CliTheme Green { get; } = new()
        {
            HeadingStyle = new CliStyle().Foreground(ConsoleColor.Yellow),
            FirstColumnStyle = new CliStyle().Foreground(ConsoleColor.Green)
        };

        /// <summary>Gets the Dark Green theme.</summary>
        public static CliTheme DarkGreen { get; } = new()
        {
            DefaultStyle = new CliStyle().Decoration(CliDecoration.Dim),
            HeadingStyle = new CliStyle().Foreground(ConsoleColor.DarkYellow),
            FirstColumnStyle = new CliStyle().Foreground(ConsoleColor.DarkGreen)
        };

        /// <summary>Gets the Blue theme.</summary>
        public static CliTheme Blue { get; } = new()
        {
            HeadingStyle = new CliStyle().Foreground(ConsoleColor.Cyan),
            FirstColumnStyle = new CliStyle().Foreground(ConsoleColor.Blue)
        };

        /// <summary>Gets the Dark Blue theme.</summary>
        public static CliTheme DarkBlue { get; } = new()
        {
            DefaultStyle = new CliStyle().Decoration(CliDecoration.Dim),
            HeadingStyle = new CliStyle().Foreground(ConsoleColor.DarkCyan),
            FirstColumnStyle = new CliStyle().Foreground(ConsoleColor.DarkBlue)
        };

        #endregion
    }
}
