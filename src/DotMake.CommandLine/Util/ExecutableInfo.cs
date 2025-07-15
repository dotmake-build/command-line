using System;
using System.IO;
using System.Reflection;

namespace DotMake.CommandLine.Util
{
    internal static class ExecutableInfo
    {
        static ExecutableInfo()
        {
            Assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
            AssemblyName = Assembly.GetName();

            try
            {
                ExecutablePath = Environment.GetCommandLineArgs()[0];
                ExecutableName = Path.GetFileNameWithoutExtension(ExecutablePath).Replace(" ", "");
            }
            catch
            {
                ExecutableName = AssemblyName.Name ?? "app";
                ExecutablePath = Path.Combine(AppContext.BaseDirectory, ExecutableName);
            }

            var assemblyInformationalVersion = Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            if (assemblyInformationalVersion != null)
                Version = assemblyInformationalVersion.InformationalVersion.TrimStart('v');
            else
            {
                var assemblyFileVersion = Assembly.GetCustomAttribute<AssemblyFileVersionAttribute>();
                if (assemblyFileVersion != null)
                    Version = assemblyFileVersion.Version;
                else
                {
                    Version = AssemblyName.Version?.ToString() ?? "";
                }
            }

            var assemblyProduct = Assembly.GetCustomAttribute<AssemblyProductAttribute>();
            Product = assemblyProduct != null ? assemblyProduct.Product : AssemblyName.Name;

            var assemblyCopyright = Assembly.GetCustomAttribute<AssemblyCopyrightAttribute>();
            Copyright = assemblyCopyright != null ? assemblyCopyright.Copyright : "";

            var assemblyDescription = Assembly.GetCustomAttribute<AssemblyDescriptionAttribute>();
            Description = assemblyDescription != null ? assemblyDescription.Description : "";
        }

        public static Assembly Assembly { get; }

        public static AssemblyName AssemblyName { get; }

        public static string ExecutablePath { get; }

        public static string ExecutableName { get; }

        public static string Product { get; }

        public static string Version { get; }

        public static string Copyright { get; }

        public static string Description { get; }
    }
}
