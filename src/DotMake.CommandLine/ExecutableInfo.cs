using System.Reflection;

namespace DotMake.CommandLine
{
    internal static class ExecutableInfo
    {
        static ExecutableInfo()
        {
            Assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
            AssemblyName = Assembly.GetName();

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
            Product = (assemblyProduct != null) ? assemblyProduct.Product : AssemblyName.Name;

            var assemblyCopyright = Assembly.GetCustomAttribute<AssemblyCopyrightAttribute>();
            Copyright = (assemblyCopyright != null) ? assemblyCopyright.Copyright : "";
        }

        public static Assembly Assembly { get; }

        public static AssemblyName AssemblyName { get; }

        public static string Product { get; }

        public static string Version { get; }

        public static string Copyright { get; }
    }
}
