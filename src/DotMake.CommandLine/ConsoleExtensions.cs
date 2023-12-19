using System;
using System.CommandLine;
using System.Text;

namespace DotMake.CommandLine
{
    internal static class ConsoleExtensions
    {
        private static bool? isConsoleRedirectionCheckSupported;

        public static bool IsConsoleRedirectionCheckSupported
        {
            get
            {
                if (isConsoleRedirectionCheckSupported is null)
                {
                    try
                    {
                        var check = Console.IsOutputRedirected;
                        isConsoleRedirectionCheckSupported = true;
                    }

                    catch (PlatformNotSupportedException)
                    {
                        isConsoleRedirectionCheckSupported = false;
                    }
                }

                return isConsoleRedirectionCheckSupported.Value;
            }
        }

        public static void SetForegroundColor(this IConsole console, ConsoleColor color)
        {
            if (IsConsoleRedirectionCheckSupported && !Console.IsOutputRedirected)
            {
                Console.ForegroundColor = color;
            }
            else if (IsConsoleRedirectionCheckSupported)
            {
                Console.ForegroundColor = color;
            }
        }

        public static void ResetForegroundColor(this IConsole console)
        {
            if (IsConsoleRedirectionCheckSupported && !Console.IsOutputRedirected)
            {
                Console.ResetColor();
            }
            else if (IsConsoleRedirectionCheckSupported)
            {
                Console.ResetColor();
            }
        }

        public static void SetOutputEncoding(this IConsole console, Encoding encoding)
        {
            try
            {
                if (IsConsoleRedirectionCheckSupported && !Console.IsOutputRedirected)
                {
                    Console.OutputEncoding = encoding;
                }
                else if (IsConsoleRedirectionCheckSupported)
                {
                    Console.OutputEncoding = encoding;
                }
            }
            catch
            {
                //ignored
            }
        }

        public static int GetWindowWidth(this IConsole console)
        {
            try
            {
                if (IsConsoleRedirectionCheckSupported && !Console.IsOutputRedirected)
                {
                    return Console.WindowWidth;
                }
                else if (IsConsoleRedirectionCheckSupported)
                {
                    return Console.WindowWidth;
                }
            }
            catch
            {
                //ignored
            }

            return int.MaxValue;
        }
    }
}
