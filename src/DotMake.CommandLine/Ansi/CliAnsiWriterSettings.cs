#pragma warning disable IDE0130

namespace DotMake.CommandLine;

/// <summary>
/// Represents settings for <see cref="CliAnsiWriter"/>.
/// </summary>
public sealed class CliAnsiWriterSettings
{
    /// <summary>
    /// Gets or sets a value indicating whether or
    /// not ANSI escape sequences are supported.
    /// </summary>
    /// <remarks>Defaults to <see cref="CliAnsiSupport.Detect"/></remarks>
    public CliAnsiSupport AnsiSupport { get; init; } = CliAnsiSupport.Detect;

    /// <summary>
    /// Gets or sets the color system to use.
    /// </summary>
    /// <remarks>Defaults to <see cref="CliColorSystemSupport.Detect"/></remarks>
    public CliColorSystemSupport ColorSystem { get; init; } = CliColorSystemSupport.Detect;
}
