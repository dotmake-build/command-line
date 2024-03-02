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
            DefaultColor = baseTheme.DefaultColor;
            DefaultBgColor = baseTheme.DefaultBgColor;
            SynopsisColor = baseTheme.SynopsisColor;
            HeadingColor = baseTheme.HeadingColor;
            HeadingCasing = baseTheme.HeadingCasing;
            HeadingNoColon = baseTheme.HeadingNoColon;
            FirstColumnColor = baseTheme.FirstColumnColor;
            SecondColumnColor = baseTheme.SecondColumnColor;
        }

        /// <summary>
        /// Gets or sets the default color used by the app.
        /// <para>Default is <see langword="null"/> which also means <see cref="ConsoleColor.Gray"/>.</para>
        /// </summary>
        public ConsoleColor? DefaultColor { get; init; }

        /// <summary>
        /// Gets or sets the default background color used by the app.
        /// <para>Default is <see langword="null"/> which also means <see cref="ConsoleColor.Black"/>.</para>
        /// </summary>
        public ConsoleColor? DefaultBgColor { get; init; }

        /// <summary>
        /// Gets or sets the color used for the synopsis section in help output.
        /// <para>Default is <see langword="null"/> which also means <see cref="DefaultColor"/>.</para>
        /// <para>Synopsis section is similar to:</para>
        /// <code language="console">
        /// DotMake Command-Line TestApp v1.6.0
        /// Copyright © 2023-2024 DotMake
        ///
        /// A root cli command with nested children
        /// </code>
        /// </summary>
        public ConsoleColor? SynopsisColor { get; init; }

        /// <summary>
        /// Gets or sets the color used for a heading in help output.
        /// <para>Default is <see langword="null"/> which also means <see cref="DefaultColor"/>.</para>
        /// <para>Heading is similar to:</para>
        /// <code language="console">
        /// Usage:
        /// Arguments:
        /// Options:
        /// Commands:
        /// </code>
        /// </summary>
        public ConsoleColor? HeadingColor { get; init; }

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
        /// Gets or sets the color used for a first column in help output.
        /// <para>Default is <see langword="null"/> which also means <see cref="DefaultColor"/>.</para>
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
        public ConsoleColor? FirstColumnColor { get; init; }

        /// <summary>
        /// Gets or sets the color used for a second column in help output.
        /// <para>Default is <see langword="null"/> which also means <see cref="DefaultColor"/>.</para>
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
        public ConsoleColor? SecondColumnColor { get; init; }

        #region Static

        /// <summary>Gets the no-color theme.</summary>
        public static CliTheme NoColor { get; } = new();

        /// <summary>Gets the default theme.</summary>
        public static CliTheme Default { get; } = new()
        {
            FirstColumnColor = ConsoleColor.White
        };

        /// <summary>Gets the Red theme.</summary>
        public static CliTheme Red { get; } = new()
        {
            DefaultColor = ConsoleColor.White,
            HeadingColor = ConsoleColor.Magenta,
            FirstColumnColor = ConsoleColor.Red
        };

        /// <summary>Gets the Dark Red theme.</summary>
        public static CliTheme DarkRed { get; } = new()
        {
            DefaultColor = ConsoleColor.Gray,
            HeadingColor = ConsoleColor.DarkMagenta,
            FirstColumnColor = ConsoleColor.DarkRed
        };

        /// <summary>Gets the Green theme.</summary>
        public static CliTheme Green { get; } = new()
        {
            DefaultColor = ConsoleColor.White,
            HeadingColor = ConsoleColor.Yellow,
            FirstColumnColor = ConsoleColor.Green
        };

        /// <summary>Gets the Dark Green theme.</summary>
        public static CliTheme DarkGreen { get; } = new()
        {
            DefaultColor = ConsoleColor.Gray,
            HeadingColor = ConsoleColor.DarkYellow,
            FirstColumnColor = ConsoleColor.DarkGreen
        };

        /// <summary>Gets the Blue theme.</summary>
        public static CliTheme Blue { get; } = new()
        {
            DefaultColor = ConsoleColor.White,
            HeadingColor = ConsoleColor.Cyan,
            FirstColumnColor = ConsoleColor.Blue
        };

        /// <summary>Gets the Dark Blue theme.</summary>
        public static CliTheme DarkBlue { get; } = new()
        {
            DefaultColor = ConsoleColor.Gray,
            HeadingColor = ConsoleColor.DarkCyan,
            FirstColumnColor = ConsoleColor.DarkBlue
        };

        #endregion
    }
}
