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
                //Color 07 will set it to the default scheme that cmd.exe uses.
                //0 = Black
                //7 = White  (ConsoleColor.Gray is 7)
                //https://superuser.com/a/158769
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
                //Color 07 will set it to the default scheme that cmd.exe uses.
                //0 = Black
                //7 = White  (ConsoleColor.Gray is 7)
                //https://superuser.com/a/158769
                if (color == null)
                    Console.BackgroundColor = defaultColor ?? ConsoleColor.Black;
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
