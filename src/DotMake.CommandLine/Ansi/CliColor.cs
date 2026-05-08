using System;
using System.Diagnostics;
using System.Globalization;
using System.Linq;
#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
#pragma warning disable IDE0130

namespace DotMake.CommandLine;

/// <summary>
/// Represents a color.
/// </summary>
public readonly partial struct CliColor : IEquatable<CliColor>
{
    /// <summary>
    /// Gets the default color.
    /// </summary>
    public static CliColor Default { get; } = new(0, 0, 0, 0, true);

    /// <summary>
    /// Gets the red component.
    /// </summary>
    public byte R { get; }

    /// <summary>
    /// Gets the green component.
    /// </summary>
    public byte G { get; }

    /// <summary>
    /// Gets the blue component.
    /// </summary>
    public byte B { get; }

    /// <summary>
    /// Gets the number of the color, if any.
    /// </summary>
    internal byte? Number { get; }

    /// <summary>
    /// Gets a value indicating whether or not this is the default color.
    /// </summary>
    internal bool IsDefault { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="CliColor"/> struct.
    /// </summary>
    /// <param name="red">The red component.</param>
    /// <param name="green">The green component.</param>
    /// <param name="blue">The blue component.</param>
    public CliColor(byte red, byte green, byte blue)
    {
        R = red;
        G = green;
        B = blue;
        IsDefault = false;
        Number = null;
    }

    /// <summary>
    /// Blends two colors.
    /// </summary>
    /// <param name="other">The other color.</param>
    /// <param name="factor">The blend factor.</param>
    /// <returns>The resulting color.</returns>
    public CliColor Blend(CliColor other, float factor)
    {
        // https://github.com/willmcgugan/rich/blob/f092b1d04252e6f6812021c0f415dd1d7be6a16a/rich/color.py#L494
        return new CliColor(
            (byte)(R + ((other.R - R) * factor)),
            (byte)(G + ((other.G - G) * factor)),
            (byte)(B + ((other.B - B) * factor)));
    }

    /// <summary>
    /// Gets the hexadecimal representation of the color.
    /// </summary>
    /// <returns>The hexadecimal representation of the color.</returns>
    public string ToHex()
    {
        return string.Format(
            CultureInfo.InvariantCulture,
            "{0}{1}{2}",
            R.ToString("X2", CultureInfo.InvariantCulture),
            G.ToString("X2", CultureInfo.InvariantCulture),
            B.ToString("X2", CultureInfo.InvariantCulture));
    }

    /// <summary>
    /// Gets the exact or closest color in the specified <see cref="CliColorSystem"/>.
    /// </summary>
    /// <param name="system">The color system.</param>
    /// <returns>The exact or closest color in the specified <see cref="CliColorSystem"/>.</returns>
    public CliColor ExactOrClosest(CliColorSystem system)
    {
        return CliColorPalette.ExactOrClosest(system, this);
    }

    /// <inheritdoc/>
    public override int GetHashCode()
    {
        unchecked
        {
            var hash = (int)2166136261;
            hash = (hash * 16777619) ^ R.GetHashCode();
            hash = (hash * 16777619) ^ G.GetHashCode();
            hash = (hash * 16777619) ^ B.GetHashCode();
            return hash;
        }
    }

    /// <inheritdoc/>
    public override bool Equals(object? obj)
    {
        return obj is CliColor color && Equals(color);
    }

    /// <inheritdoc/>
    public bool Equals(CliColor other)
    {
        return (IsDefault && other.IsDefault) ||
               (IsDefault == other.IsDefault && R == other.R && G == other.G && B == other.B);
    }

    /// <summary>
    /// Checks if two <see cref="CliColor"/> instances are equal.
    /// </summary>
    /// <param name="left">The first color instance to compare.</param>
    /// <param name="right">The second color instance to compare.</param>
    /// <returns><c>true</c> if the two colors are equal, otherwise <c>false</c>.</returns>
    public static bool operator ==(CliColor left, CliColor right)
    {
        return left.Equals(right);
    }

    /// <summary>
    /// Checks if two <see cref="CliColor"/> instances are not equal.
    /// </summary>
    /// <param name="left">The first color instance to compare.</param>
    /// <param name="right">The second color instance to compare.</param>
    /// <returns><c>true</c> if the two colors are not equal, otherwise <c>false</c>.</returns>
    public static bool operator !=(CliColor left, CliColor right)
    {
        return !(left == right);
    }

    /// <summary>
    /// Converts a <see cref="int"/> to a <see cref="CliColor"/>.
    /// </summary>
    /// <param name="number">The color number to convert.</param>
    public static implicit operator CliColor(int number)
    {
        return FromInt32(number);
    }

    /// <summary>
    /// Converts a <see cref="ConsoleColor"/> to a <see cref="CliColor"/>.
    /// </summary>
    /// <param name="color">The color to convert.</param>
    public static implicit operator CliColor(ConsoleColor color)
    {
        return FromConsoleColor(color);
    }

    /// <summary>
    /// Converts a <see cref="CliColor"/> to a <see cref="ConsoleColor"/>.
    /// </summary>
    /// <param name="color">The console color to convert.</param>
    public static implicit operator ConsoleColor(CliColor color)
    {
        return ToConsoleColor(color);
    }

    /// <summary>
    /// Converts a <see cref="CliColor"/> to a <see cref="ConsoleColor"/>.
    /// </summary>
    /// <param name="color">The color to convert.</param>
    /// <returns>A <see cref="ConsoleColor"/> representing the <see cref="CliColor"/>.</returns>
    public static ConsoleColor ToConsoleColor(CliColor color)
    {
        if (color.IsDefault)
        {
            return (ConsoleColor)(-1);
        }

        if (color.Number == null || color.Number.Value >= 16)
        {
            color = CliColorPalette.ExactOrClosest(CliColorSystem.Standard, color);
        }

        // Should not happen, but this will make things easier if we mess things up...
        Debug.Assert(
            color.Number >= 0 && color.Number < 16,
            "Color does not fall inside the standard palette range.");

        return color.Number.Value switch
        {
            0 => ConsoleColor.Black,
            1 => ConsoleColor.DarkRed,
            2 => ConsoleColor.DarkGreen,
            3 => ConsoleColor.DarkYellow,
            4 => ConsoleColor.DarkBlue,
            5 => ConsoleColor.DarkMagenta,
            6 => ConsoleColor.DarkCyan,
            7 => ConsoleColor.Gray,
            8 => ConsoleColor.DarkGray,
            9 => ConsoleColor.Red,
            10 => ConsoleColor.Green,
            11 => ConsoleColor.Yellow,
            12 => ConsoleColor.Blue,
            13 => ConsoleColor.Magenta,
            14 => ConsoleColor.Cyan,
            15 => ConsoleColor.White,
            _ => throw new InvalidOperationException("Cannot convert color to console color."),
        };
    }

    /// <summary>
    /// Converts a color number into a <see cref="CliColor"/>.
    /// </summary>
    /// <param name="number">The color number.</param>
    /// <returns>The color representing the specified color number.</returns>
    public static CliColor FromInt32(int number)
    {
        return CliColorTable.GetColor(number);
    }

    /// <summary>
    /// Creates a color from a hexadecimal string representation.
    /// </summary>
    /// <param name="hex">The hexadecimal string representation of the color.</param>
    /// <returns>The color created from the hexadecimal string.</returns>
    public static CliColor FromHex(string hex)
    {
        if (hex == null)
            throw new ArgumentNullException(nameof(hex));

        if (hex.StartsWith("#"))
        {
            hex = hex.Substring(1);
        }

        // 3 digit hex codes are expanded to 6 digits
        // by doubling each digit, conform to CSS color codes
        if (hex.Length == 3)
        {
            hex = string.Concat(hex.Select(c => new string(c, 2)));
        }

        var r = byte.Parse(hex.Substring(0, 2), NumberStyles.HexNumber);
        var g = byte.Parse(hex.Substring(2, 2), NumberStyles.HexNumber);
        var b = byte.Parse(hex.Substring(4, 2), NumberStyles.HexNumber);

        return new CliColor(r, g, b);
    }

    /// <summary>
    /// Tries to convert a hexadecimal color code to a <see cref="CliColor"/> object.
    /// </summary>
    /// <param name="hex">The hexadecimal color code.</param>
    /// <param name="color">When this method returns, contains the <see cref="CliColor"/> equivalent of the hexadecimal color code, if the conversion succeeded, or <see cref="CliColor.Default"/> if the conversion failed.</param>
    /// <returns><c>true</c> if the conversion succeeded; otherwise, <c>false</c>.</returns>
    public static bool TryFromHex(string hex, out CliColor color)
    {
        try
        {
            color = FromHex(hex);
            return true;
        }
        catch
        {
            color = CliColor.Default;
            return false;
        }
    }

    /// <summary>
    /// Gets a <see cref="CliColor"/> from its name.
    /// </summary>
    /// <param name="name">The name of the color.</param>
    /// <returns>The requested <see cref="CliColor"/> or <c>null</c> if not found.</returns>
    public static CliColor? FromName(string name)
    {
        return CliColorTable.GetColor(name);
    }

    /// <summary>
    /// Converts a <see cref="ConsoleColor"/> to a <see cref="CliColor"/>.
    /// </summary>
    /// <param name="color">The color to convert.</param>
    /// <returns>A <see cref="CliColor"/> representing the <see cref="ConsoleColor"/>.</returns>
    public static CliColor FromConsoleColor(ConsoleColor color)
    {
        return color switch
        {
            ConsoleColor.Black => Black,
            ConsoleColor.Blue => Blue,
            ConsoleColor.Cyan => Aqua,
            ConsoleColor.DarkBlue => Navy,
            ConsoleColor.DarkCyan => Teal,
            ConsoleColor.DarkGray => Grey,
            ConsoleColor.DarkGreen => Green,
            ConsoleColor.DarkMagenta => Purple,
            ConsoleColor.DarkRed => Maroon,
            ConsoleColor.DarkYellow => Olive,
            ConsoleColor.Gray => Silver,
            ConsoleColor.Green => Lime,
            ConsoleColor.Magenta => Fuchsia,
            ConsoleColor.Red => Red,
            ConsoleColor.White => White,
            ConsoleColor.Yellow => Yellow,
            _ => Default,
        };
    }

    /// <summary>
    /// Converts the color to a markup string.
    /// </summary>
    /// <returns>A <see cref="string"/> representing the color as markup.</returns>
    public string ToMarkup()
    {
        if (IsDefault)
        {
            return "default";
        }

        if (Number != null)
        {
            var name = CliColorTable.GetName(Number.Value);
            if (!string.IsNullOrWhiteSpace(name))
            {
                return name;
            }
        }

        return string.Format(CultureInfo.InvariantCulture, "#{0:X2}{1:X2}{2:X2}", R, G, B);
    }

    /// <inheritdoc/>
    public override string ToString()
    {
        if (IsDefault)
        {
            return "default";
        }

        if (Number != null)
        {
            var name = CliColorTable.GetName(Number.Value);
            if (!string.IsNullOrWhiteSpace(name))
            {
                return name;
            }
        }

        return string.Format(CultureInfo.InvariantCulture, "#{0:X2}{1:X2}{2:X2} (RGB={0},{1},{2})", R, G, B);
    }
}
