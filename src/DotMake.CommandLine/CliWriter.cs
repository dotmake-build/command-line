using System;
using System.IO;
using System.Runtime.CompilerServices;
using DotMake.CommandLine.Util;

namespace DotMake.CommandLine
{
    /// <summary>
    /// Represents a CLI writer that provides styled terminal output (color and text decoration),
    /// using ANSI escape sequences when supported and console-native fallbacks otherwise.
    /// </summary>
    public class CliWriter : DelegatingTextWriter
    {
        private readonly bool isConsole;
        private readonly CliAnsiWriter ansiWriter;
        private readonly object sync = new();
        private CliStyle currentStyle = CliStyle.Plain;

        /// <summary>Initializes a new instance of the <see cref="CliWriter" /> class.</summary>
        /// <param name="inner">
        /// The underlying <see cref="TextWriter"/> to which output is delegated.
        /// </param>
        /// <param name="leaveOpen">
        /// Whether to leave the underlying <see cref="TextWriter"/> open when this instance is disposed.
        /// </param>
        public CliWriter(TextWriter inner, bool leaveOpen = true)
            : base(inner, leaveOpen)
        {
            if ((ConsoleExtensions.IsStandardOut(inner) && !Console.IsOutputRedirected)
                || (ConsoleExtensions.IsStandardError(inner) && !Console.IsErrorRedirected))
            {
                isConsole = true;

                var capabilities = CliAnsiCapabilities.Create(inner);
                if (capabilities.Ansi)
                    ansiWriter = new CliAnsiWriter(inner, capabilities);
            }
        }


        /// <summary>
        /// Writes a string with the specified style (color and text decoration), to the terminal.
        /// </summary>
        /// <param name="style">The style (color and text decoration).</param>
        /// <param name="value">The string to write.</param>
        /// <returns>The same instance so that multiple calls can be chained.</returns>
        public CliWriter Write(CliStyle style, string value)
        {
            lock (sync)
            {
                WriteInternal(value, style);

                if (currentStyle != style)
                    SetStyleInternal(currentStyle);
            }

            return this;
        }

        /// <summary>
        /// Writes a formatted string with the specified style (color and text decoration), to the terminal.
        /// </summary>
        /// <param name="style">The style (color and text decoration).</param>
        /// <param name="format">A composite format string.</param>
        /// <param name="arg">An object array that contains zero or more objects to format and write.</param>
        /// <returns>The same instance so that multiple calls can be chained.</returns>
        public CliWriter Write(CliStyle style, string format, params object[] arg)
        {
            return Write(style, string.Format(FormatProvider, format, arg));
        }


        /// <summary>
        /// Writes a string with the specified style (color and text decoration),
        /// followed by the current line terminator, to the terminal.
        /// </summary>
        /// <param name="style">The style (color and text decoration).</param>
        /// <param name="value">The string to write.</param>
        /// <returns>The same instance so that multiple calls can be chained.</returns>
        public CliWriter WriteLine(CliStyle style, string value)
        {
            lock (sync)
            {
                WriteInternal(value, style);
                Inner.WriteLine();

                if (currentStyle != style)
                    SetStyleInternal(currentStyle);
            }

            return this;
        }

        /// <summary>
        /// Writes a formatted string with the specified style (color and text decoration),
        /// followed by the current line terminator, to the terminal.
        /// </summary>
        /// <param name="style">The style (color and text decoration).</param>
        /// <param name="format">A composite format string.</param>
        /// <param name="arg">An object array that contains zero or more objects to format and write.</param>
        /// <returns>The same instance so that multiple calls can be chained.</returns>
        public CliWriter WriteLine(CliStyle style, string format, params object[] arg)
        {
            return WriteLine(style, string.Format(FormatProvider, format, arg));
        }


        /// <summary>
        /// Writes a string as link with URL with the specified style (color and text decoration), to the terminal.
        /// </summary>
        /// <param name="link">The link with URL.</param>
        /// <param name="style">The style (color and text decoration).</param>
        /// <param name="value">The string to write.</param>
        /// <returns>The same instance so that multiple calls can be chained.</returns>
        public CliWriter WriteLink(CliLink link, CliStyle style, string value)
        {
            lock (sync)
            {
                WriteInternal(value, style, link);

                if (currentStyle != style)
                    SetStyleInternal(currentStyle);
            }

            return this;
        }

        /// <summary>
        /// Writes a formatted string as link with URL with the specified style (color and text decoration), to the terminal.
        /// </summary>
        /// <param name="link">The link with URL.</param>
        /// <param name="style">The style (color and text decoration).</param>
        /// <param name="format">A composite format string.</param>
        /// <param name="arg">An object array that contains zero or more objects to format and write.</param>
        /// <returns>The same instance so that multiple calls can be chained.</returns>
        public CliWriter WriteLink(CliLink link, CliStyle style, string format, params object[] arg)
        {
            return WriteLink(link, style, string.Format(FormatProvider, format, arg));
        }

        /// <summary>
        /// Writes a string as link with URL, to the terminal.
        /// </summary>
        /// <param name="link">The link with URL.</param>
        /// <param name="value">The string to write.</param>
        /// <returns>The same instance so that multiple calls can be chained.</returns>
        public CliWriter WriteLink(CliLink link, string value)
        {
            return WriteLink(link, CliStyle.Plain, value);
        }

        /// <summary>
        /// Writes a formatted string as link with URL, to the terminal.
        /// </summary>
        /// <param name="link">The link with URL.</param>
        /// <param name="format">A composite format string.</param>
        /// <param name="arg">An object array that contains zero or more objects to format and write.</param>
        /// <returns>The same instance so that multiple calls can be chained.</returns>
        public CliWriter WriteLink(CliLink link, string format, params object[] arg)
        {
            return WriteLink(link, CliStyle.Plain, string.Format(FormatProvider, format, arg));
        }


        /// <summary>
        /// Applies the specified style (color and text decoration) to subsequent terminal output.
        /// </summary>
        /// <param name="style">The style (color and text decoration).</param>
        /// <returns>The same instance so that multiple calls can be chained.</returns>
        public CliWriter SetStyle(CliStyle style)
        {
            lock (sync)
            {
                SetStyleInternal(style);

                currentStyle = style;
            }

            return this;
        }

        /// <summary>
        /// Resets terminal output styling (color and text decoration) to the default style.
        /// </summary>
        /// <returns>The same instance so that multiple calls can be chained.</returns>
        public CliWriter ResetStyle()
        {
            lock (sync)
            {
                ResetStyleInternal();

                currentStyle = CliStyle.Plain;
            }

            return this;
        }


        private void WriteInternal(string value, CliStyle style, CliLink link = null)
        {
            if (ansiWriter != null)
            {
                ansiWriter.Write(value, style, link);
            }
            else if (isConsole)
            {
                SetStyleInternal(style);

                Inner.Write(value);
            }
            else
                Inner.Write(value);
        }

        private void SetStyleInternal(CliStyle style)
        {
            if (ansiWriter != null)
            {
                ResetStyleInternal();

                ansiWriter.Style(style);
            }
            else if (isConsole)
            {
                ResetStyleInternal();

                ConsoleExtensions.SetColor(style.Foreground, style.Background);
            }
        }

        private void ResetStyleInternal()
        {
            if (ansiWriter != null)
            {
                ansiWriter.ResetStyle();
            }
            else if (isConsole)
            {
                ConsoleExtensions.ResetColor();
            }
        }

        #region Static

        private static readonly ConditionalWeakTable<TextWriter, Lazy<CliWriter>> Cache = new();

        /// <summary>
        /// Gets a cached <see cref="CliWriter"/> instance for a <see cref="TextWriter"/> instance.
        /// This way ANSI capabilities will be detected only once (which may be an expensive operation)
        /// for <see cref="Console.Out"/>, for example.
        /// The cache entry will be removed when the <see cref="TextWriter"/> instance becomes unreachable.
        /// </summary>
        /// <param name="output"></param>
        /// <returns>A <see cref="CliWriter"/> instance.</returns>
        public static CliWriter GetCached(TextWriter output)
        {
            if (output is null)
                throw new ArgumentNullException(nameof(output));

            var lazy = Cache.GetValue(output, writer =>
                new Lazy<CliWriter>(() => new CliWriter(writer))
            );

            return lazy.Value;
        }

        #endregion
    }
}
