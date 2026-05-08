using System;
using System.Threading;

#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
#pragma warning disable IDE0130

namespace DotMake.CommandLine;

/// <summary>
/// Represents a link.
/// </summary>
public sealed class CliLink : IEquatable<CliLink>
{
    /// <summary>
    /// Represents a link.
    /// </summary>
    /// <param name="url">The link URL.</param>
    public CliLink(string url)
    {
        Url = url;
    }

    /// <summary>
    /// Gets the link ID.
    /// </summary>
    public int? Id { get; } = Random.Value.Next(0, int.MaxValue);
    private static readonly ThreadLocal<Random> Random =
        new ThreadLocal<Random>(() => new Random());

    /// <summary>
    /// Gets the url.
    /// </summary>
    public string Url { get; }

    /// <inheritdoc />
    public bool Equals(CliLink? other)
    {
        if (other is null)
        {
            return false;
        }

        if (ReferenceEquals(this, other))
        {
            return true;
        }

        return Id == other.Id && Url == other.Url;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj)
    {
        return ReferenceEquals(this, obj) || obj is CliLink other && Equals(other);
    }

    /// <inheritdoc />
    public override int GetHashCode()
    {
        unchecked
        {
            return (Id.GetHashCode() * 397) ^ Url.GetHashCode();
        }
    }
}
