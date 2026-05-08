using System;
using System.Collections.Generic;
using System.Linq;

#pragma warning disable IDE0130

namespace DotMake.CommandLine;

internal static partial class CliColorPalette
{
    public static IReadOnlyList<CliColor> Legacy { get; }
    public static IReadOnlyList<CliColor> Standard { get; }
    public static IReadOnlyList<CliColor> EightBit { get; }

    static CliColorPalette()
    {
        Legacy = GenerateLegacyPalette();
        Standard = GenerateStandardPalette(Legacy);
        EightBit = GenerateEightBitPalette(Standard);
    }

    public static CliColor ExactOrClosest(CliColorSystem system, CliColor color)
    {
        var exact = Exact(system, color);
        return exact ?? Closest(system, color);
    }

    private static CliColor? Exact(CliColorSystem system, CliColor color)
    {
        if (system == CliColorSystem.TrueColor)
        {
            return color;
        }

        var palette = system switch
        {
            CliColorSystem.Legacy => Legacy,
            CliColorSystem.Standard => Standard,
            CliColorSystem.EightBit => EightBit,
            _ => throw new NotSupportedException(),
        };

        return palette
            .Where(c => c.Equals(color))
            .Cast<CliColor?>()
            .FirstOrDefault();
    }

    private static CliColor Closest(CliColorSystem system, CliColor color)
    {
        if (system == CliColorSystem.TrueColor)
        {
            return color;
        }

        var palette = system switch
        {
            CliColorSystem.Legacy => Legacy,
            CliColorSystem.Standard => Standard,
            CliColorSystem.EightBit => EightBit,
            _ => throw new NotSupportedException(),
        };

        // https://stackoverflow.com/a/9085524
        static double Distance(CliColor first, CliColor second)
        {
            var rmean = ((float)first.R + second.R) / 2;
            var r = first.R - second.R;
            var g = first.G - second.G;
            var b = first.B - second.B;
            return Math.Sqrt(
                ((int)((512 + rmean) * r * r) >> 8)
                + (4 * g * g)
                + ((int)((767 - rmean) * b * b) >> 8));
        }

        return Enumerable.Range(0, int.MaxValue)
            .Zip(palette, (id, other) => (Distance: Distance(other, color), Id: id, Color: other))
            .OrderBy(x => x.Distance)
            .FirstOrDefault().Color;
    }
}
