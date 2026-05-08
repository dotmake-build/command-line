using System;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;

namespace DotMake.CommandLine.Util
{
    internal static class ConsoleExtensions
    {
        private static readonly bool NoColorEnvironment = Environment.GetEnvironmentVariables().Contains("NO_COLOR");

        private static readonly bool ColorIsSupported =
            !NoColorEnvironment
            && !RuntimeInformation.IsOSPlatform(OSPlatform.Create("BROWSER"))
            && !RuntimeInformation.IsOSPlatform(OSPlatform.Create("ANDROID"))
            && !RuntimeInformation.IsOSPlatform(OSPlatform.Create("IOS"))
            && !RuntimeInformation.IsOSPlatform(OSPlatform.Create("TVOS"));

        private static readonly bool EncodingIsSupported =
            !RuntimeInformation.IsOSPlatform(OSPlatform.Create("ANDROID"))
            && !RuntimeInformation.IsOSPlatform(OSPlatform.Create("IOS"))
            && !RuntimeInformation.IsOSPlatform(OSPlatform.Create("TVOS"));


        public static void SetColor(ConsoleColor color, ConsoleColor? backgroundColor = null)
        {
            if (ColorIsSupported && !Console.IsOutputRedirected)
            {
                if (color != (ConsoleColor)(-1))
                    Console.ForegroundColor = color;

                if (backgroundColor.HasValue && backgroundColor != (ConsoleColor)(-1))
                    Console.BackgroundColor = backgroundColor.Value;
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

        public static bool IsStandardOut(TextWriter writer)
        {
            try
            {
                return writer == Console.Out;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsStandardError(TextWriter writer)
        {
            try
            {
                return writer == Console.Error;
            }
            catch
            {
                return false;
            }
        }
    }
}
