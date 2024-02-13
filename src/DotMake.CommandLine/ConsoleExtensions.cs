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

        public static void SetForegroundColor(ConsoleColor color)
        {
            if (ColorIsSupported && !Console.IsOutputRedirected)
            {
                Console.ForegroundColor = color;
            }
        }

        public static void ResetForegroundColor()
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
