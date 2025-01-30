using System;
using System.Runtime.InteropServices;
using System.Text;

namespace DotMake.CommandLine
{
    internal static class ConsoleExtensions
    {
        private static readonly bool ColorIsSupported =
            !RuntimeInformation.IsOSPlatform(OSPlatform.Create("BROWSER"))
            && !RuntimeInformation.IsOSPlatform(OSPlatform.Create("ANDROID"))
            && !RuntimeInformation.IsOSPlatform(OSPlatform.Create("IOS"))
            && !RuntimeInformation.IsOSPlatform(OSPlatform.Create("TVOS"));

        private static readonly bool EncodingIsSupported =
            !RuntimeInformation.IsOSPlatform(OSPlatform.Create("ANDROID"))
            && !RuntimeInformation.IsOSPlatform(OSPlatform.Create("IOS"))
            && !RuntimeInformation.IsOSPlatform(OSPlatform.Create("TVOS"));

        public static void SetColor(ConsoleColor? color, ConsoleColor? defaultColor = null)
        {
            if (ColorIsSupported && !Console.IsOutputRedirected)
            {
                // https://learn.microsoft.com/en-us/dotnet/api/system.console.foregroundcolor?view=net-8.0#remarks
                // On Windows, the default color is gray (ConsoleColor.Gray).
                // On *nix-like platforms, the default color is unset ((ConsoleColor)-1).
                if (color == null)
                    Console.ForegroundColor = defaultColor ?? ConsoleColor.Gray;
                else
                    Console.ForegroundColor = color.Value;
            }
        }

        public static void SetBgColor(ConsoleColor? color, ConsoleColor? defaultColor = null)
        {
            if (ColorIsSupported && !Console.IsOutputRedirected)
            {
                // https://learn.microsoft.com/en-us/dotnet/api/system.console.backgroundcolor?view=net-8.0#remarks
                // On Windows, the default color is black (ConsoleColor.Black).
                // On *nix-like platforms, the default color is unset ((ConsoleColor)-1).
                if (color == null)
                    // Console.BackgroundColor = defaultColor ?? ConsoleColor.Black;
                    Console.BackgroundColor = defaultColor ?? (RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? ConsoleColor.Black : (ConsoleColor)(-1));
                else
                    Console.BackgroundColor = color.Value;
            }
        }

        public static void ResetColor()
        {
            if (ColorIsSupported && !Console.IsOutputRedirected)
            {
                Console.ResetColor();
            }
        }

        public static void SetOutputEncoding(Encoding encoding)
        {
            if (EncodingIsSupported && !Console.IsOutputRedirected)
            {
                Console.OutputEncoding = encoding;
            }
        }
    }
}
