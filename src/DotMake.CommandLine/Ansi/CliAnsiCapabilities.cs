using System;
using System.IO;
#pragma warning disable IDE0130

namespace DotMake.CommandLine;

/// <summary>
/// Represents ANSI capabilities.
/// </summary>
public class CliAnsiCapabilities
{
    /// <summary>
    /// Gets or sets the color system.
    /// </summary>
    public CliColorSystem ColorSystem { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether or not
    /// the console supports VT/ANSI control codes.
    /// </summary>
    public bool Ansi { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether or not
    /// the console support links.
    /// </summary>
    public bool Links { get; init; }

    /// <summary>
    /// Gets or sets a value indicating whether
    /// or not the console supports alternate buffers.
    /// </summary>
    public bool AlternateBuffer { get; init; }

    /// <summary>
    /// Creates a <see cref="CliAnsiCapabilities"/> instance from the provided arguments.
    /// </summary>
    /// <param name="writer">The text writer to use.</param>
    /// <returns>A <see cref="CliAnsiCapabilities"/> instance.</returns>
    public static CliAnsiCapabilities Create(TextWriter writer)
    {
        return Create(writer, new CliAnsiWriterSettings());
    }

    /// <summary>
    /// Creates a <see cref="CliAnsiCapabilities"/> instance from the provided arguments.
    /// </summary>
    /// <param name="writer">The text writer to use.</param>
    /// <param name="settings">The settings to use.</param>
    /// <returns>A <see cref="CliAnsiCapabilities"/> instance.</returns>
    public static CliAnsiCapabilities Create(TextWriter writer, CliAnsiWriterSettings settings)
    {
        if (writer == null)
            throw new ArgumentNullException(nameof(writer));

        // Detect if the terminal support ANSI or not
        var (supportsAnsi, legacyConsole) = CliAnsiDetector.Detect(writer, settings.AnsiSupport);

        // Get the color system
        var colorSystem = settings.ColorSystem == CliColorSystemSupport.Detect
            ? CliColorSystemDetector.Detect(supportsAnsi)
            : (CliColorSystem)settings.ColorSystem;

        return new CliAnsiCapabilities
        {
            ColorSystem = colorSystem,
            Ansi = supportsAnsi,
            Links = supportsAnsi && !legacyConsole,
            AlternateBuffer = supportsAnsi && !legacyConsole,
        };
    }
}
