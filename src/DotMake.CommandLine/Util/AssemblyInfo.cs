using System.Reflection;

namespace DotMake.CommandLine.Util
{
    internal class AssemblyInfo
    {
        public AssemblyInfo(Assembly assembly)
        {
            Assembly = assembly;
            AssemblyName = assembly.GetName();

            var assemblyInformationalVersion = Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
            if (assemblyInformationalVersion != null)
            {
                var parts = assemblyInformationalVersion.InformationalVersion.Split('+');
                Version = parts[0].TrimStart('v');
                SourceRevisionId = (parts.Length > 1) ?  parts[1].Trim() : "";
            }
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

        public Assembly Assembly { get; }

        public AssemblyName AssemblyName { get; }

        public string Product { get; }

        public string Version { get; }

        public string SourceRevisionId { get; }

        public string Copyright { get; }

        public string Description { get; }
    }
}
