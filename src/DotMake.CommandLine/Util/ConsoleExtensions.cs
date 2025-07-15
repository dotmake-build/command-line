using System;
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


        public static void SetColor(ConsoleColor? color, ConsoleColor? fallbackColor = null)
        {
            if (ColorIsSupported && !Console.IsOutputRedirected)
            {
                color ??= fallbackColor;

                // https://learn.microsoft.com/en-us/dotnet/api/system.console.foregroundcolor?view=net-8.0#remarks
                // On Windows, the default color is ConsoleColor.Gray.
                // On Unix-like platforms, the default color is (ConsoleColor)-1 (unset/unknown).
                Console.ForegroundColor = FixColorVisibility(color);
            }
        }

        public static void SetBgColor(ConsoleColor? color, ConsoleColor? fallbackColor = null)
        {
            if (ColorIsSupported && !Console.IsOutputRedirected)
            {
                color ??= fallbackColor;

                // https://learn.microsoft.com/en-us/dotnet/api/system.console.backgroundcolor?view=net-8.0#remarks
                // On Windows, the default background color is ConsoleColor.Black.
                // On Unix-like platforms, the default background color is (ConsoleColor)-1 (unset/unknown).
                if (color == null)
                    //only way to reset BackgroundColor, but it also resets ForegroundColor
                    //so SetColor should be called after SetBgColor.
                    //The better way would be combining color setting methods into one with ResetColor at top
                    //but for now we call SetBgColor once anyway.
                    Console.ResetColor(); 
                else
                    Console.BackgroundColor = color.Value;
            }
        }

        public static ConsoleColor FixColorVisibility(ConsoleColor? foregroundColor)
        {
            /*
                It's impossible to detect terminal colors so we will make assumptions.
                Even in Windows Terminal, Console.BackgroundColor and Console.ForegroundColor will not return the theme colors,
                they always return Black and Gray. Only way to change the values was to use "color" command in prompt:
                e.g. "color f0" to test black on white, "color 07" to reset to defaults

                0 = Black       8 = Gray
                1 = Blue        9 = Light Blue
                2 = Green       A = Light Green
                3 = Aqua        B = Light Aqua
                4 = Red         C = Light Red
                5 = Purple      D = Light Purple
                6 = Yellow      E = Light Yellow
                7 = White       F = Bright White

                https://superuser.com/a/158769
            */


            var backgroundColor = Console.BackgroundColor;
            if (backgroundColor == (ConsoleColor)(-1))
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    foregroundColor = ConsoleColor.Black;
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    foregroundColor = ConsoleColor.White;
            }

            if (foregroundColor == null)
                foregroundColor = (ConsoleColor)(-1);
            if (foregroundColor == (ConsoleColor)(-1))
            {
                if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
                    foregroundColor = ConsoleColor.Gray;
                else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
                    foregroundColor = ConsoleColor.Black;
            }

            if (foregroundColor == backgroundColor)
                switch (backgroundColor)
                {
                    case ConsoleColor.Black:
                        foregroundColor =  ConsoleColor.Gray;
                        break;
                    case ConsoleColor.Gray: //White
                        foregroundColor =  ConsoleColor.Black;
                        break;
                    case ConsoleColor.White: //Bright White
                        foregroundColor =  ConsoleColor.Black;
                        break;
                    case ConsoleColor.DarkGray:
                        foregroundColor =  ConsoleColor.Gray;
                        break;
                    case ConsoleColor.Red:
                        foregroundColor =  ConsoleColor.DarkRed;
                        break;
                    case ConsoleColor.DarkRed:
                        foregroundColor =  ConsoleColor.Red;
                        break;
                    case ConsoleColor.Blue:
                        foregroundColor =  ConsoleColor.DarkBlue;
                        break;
                    case ConsoleColor.DarkBlue:
                        foregroundColor =  ConsoleColor.Blue;
                        break;
                    case ConsoleColor.Green:
                        foregroundColor =  ConsoleColor.DarkGreen;
                        break;
                    case ConsoleColor.DarkGreen:
                        foregroundColor =  ConsoleColor.Green ;
                        break;
                    case ConsoleColor.Yellow:
                        foregroundColor =  ConsoleColor.DarkYellow;
                        break;
                    case ConsoleColor.DarkYellow:
                        foregroundColor =  ConsoleColor.Yellow ;
                        break;
                    case ConsoleColor.Cyan:
                        foregroundColor =  ConsoleColor.DarkCyan;
                        break;
                    case ConsoleColor.DarkCyan:
                        foregroundColor =  ConsoleColor.Cyan;
                        break;
                    case ConsoleColor.Magenta:
                        foregroundColor =  ConsoleColor.DarkMagenta;
                        break;
                    case ConsoleColor.DarkMagenta:
                        foregroundColor =  ConsoleColor.Magenta;
                        break;
                }

            return (ConsoleColor)foregroundColor;
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
