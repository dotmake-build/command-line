using System;
using System.IO;
using System.Reflection;

namespace DotMake.CommandLine.Util
{
    internal static class ExecutableInfo
    {
        static ExecutableInfo()
        {
            AssemblyInfo = new AssemblyInfo(Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly());

            try
            {
                ExecutablePath = Environment.GetCommandLineArgs()[0];
                ExecutableName = Path.GetFileNameWithoutExtension(ExecutablePath).Replace(" ", "");
            }
            catch
            {
                ExecutableName = AssemblyInfo.AssemblyName.Name ?? "app";
                ExecutablePath = Path.Combine(AppContext.BaseDirectory, ExecutableName);
            }

        }
        public static AssemblyInfo AssemblyInfo { get; }

        public static string ExecutablePath { get; }

        public static string ExecutableName { get; }
    }
}
