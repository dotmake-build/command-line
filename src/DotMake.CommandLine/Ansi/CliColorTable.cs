using System;
using System.Collections.Generic;
#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
#pragma warning disable IDE0130

namespace DotMake.CommandLine;

internal static partial class CliColorTable
{
    private static readonly Dictionary<int, string> _nameLookup;
    private static readonly Dictionary<string, int> _numberLookup;

    static CliColorTable()
    {
        _numberLookup = GenerateTable();
        _nameLookup = new Dictionary<int, string>();

        foreach (var pair in _numberLookup)
        {
            //_nameLookup.TryAdd(pair.Value, pair.Key);
            if (!_nameLookup.ContainsKey(pair.Value))
                _nameLookup.Add(pair.Value, pair.Key);
        }
    }

    public static CliColor GetColor(int number)
    {
        if (number < 0 || number > 255)
        {
            throw new InvalidOperationException("Color number must be between 0 and 255");
        }

        return CliColorPalette.EightBit[number];
    }

    public static CliColor? GetColor(string name)
    {
        if (!_numberLookup.TryGetValue(name, out var number))
        {
            return null;
        }

        if (number > CliColorPalette.EightBit.Count - 1)
        {
            return null;
        }

        return CliColorPalette.EightBit[number];
    }

    public static string? GetName(int number)
    {
        _nameLookup.TryGetValue(number, out var name);
        return name;
    }
}
