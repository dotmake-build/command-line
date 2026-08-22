using System;
using System.IO;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace DotMake.CommandLine.SourceGeneration.Inputs
{
    public class CliReferenceDependantInput : InputBase, IEquatable<CliReferenceDependantInput>
    {
        public const string ModuleInitializerAttributeFullName = "System.Runtime.CompilerServices.ModuleInitializerAttribute";
        public const string RequiredMemberAttributeFullName = "System.Runtime.CompilerServices.RequiredMemberAttribute";
        public const string CliServiceProviderExtensionsFullName = "DotMake.CommandLine.CliServiceProviderExtensions";
        public const string CliServiceCollectionExtensionsFullName = "DotMake.CommandLine.CliServiceCollectionExtensions";

        public CliReferenceDependantInput(Compilation compilation)
            : base(compilation)
        {
            Compilation = compilation;

            /*
                Notes:

                In the .NET compilation lifecycle, compilation.SourceModule.ReferencedAssemblies represents the metadata binaries
                handed to the compiler via a <PackageReference> or <ProjectReference>. If a user has a package reference
                to your library in their .csproj, MyLib.dll will be present in that collection even if they haven't
                written a single line of C# code that touches it.

                There is compilation.GetUsedAssemblyReferences() but it does not work reliably:
                https://github.com/dotnet/roslyn/issues/66188
                If there is <PackageReference>, the DLL also comes as used even if no reference in source code.

                It's also not feasible to find out if source code is actively referring to MyLib.dll,
                i.e. if the reference will be actually added to the output SourceModule.dll,
                because we would need to deep scan of all types, attributes etc.

                So only reliable way is to use <DotMakeSourceGenerator>disable</DotMakeSourceGenerator>
                or <ExcludeAssets>compile</ExcludeAssets> for <PackageReference>
            */

            /*
            UsesDotMakeCommandLine = compilation.GetUsedAssemblyReferences()
                //.Any(reference => reference.Display != null && reference.Display.Contains("DotMake.CommandLine"));
                .Select(compilation.GetAssemblyOrModuleSymbol)
                .Any(symbol => symbol is IAssemblySymbol assemblySymbol
                               && assemblySymbol.Identity.Name == "DotMake.CommandLine");
            */

            foreach (var referencedAssembly in compilation.SourceModule.ReferencedAssemblies)
            {
                switch (referencedAssembly.Name)
                {
                    case "DotMake.CommandLine":
                        UsesDotMakeCommandLine = true;
                        break;
                    case "Microsoft.Extensions.DependencyInjection.Abstractions":
                        if (referencedAssembly.Version >= new Version(2, 1, 1))
                            ReferencesMsDIAbstractions = true;
                        break;
                    case "Microsoft.Extensions.DependencyInjection":
                        if (referencedAssembly.Version >= new Version(2, 1, 1))
                            ReferencesMsDI = true;
                        break;
                }
            }

            //Don't bother checking others as we will not be generating any source code
            //if DotMake.CommandLine.dll is not actually referenced/consumed
            if (!UsesDotMakeCommandLine)
                return;

            HasModuleInitializerAttribute = (compilation.GetTypeByMetadataName(ModuleInitializerAttributeFullName) != null);
            HasRequiredMemberAttribute = (compilation.GetTypeByMetadataName(RequiredMemberAttributeFullName) != null);
            HasCliServiceProviderExtensions = (compilation.GetTypeByMetadataName(CliServiceProviderExtensionsFullName) != null);
            HasCliServiceCollectionExtensions = (compilation.GetTypeByMetadataName(CliServiceCollectionExtensionsFullName) != null);
        }

        public Compilation Compilation { get; }


        public bool UsesDotMakeCommandLine { get; }


        // ReSharper disable once InconsistentNaming
        public bool ReferencesMsDIAbstractions { get; }

        // ReSharper disable once InconsistentNaming
        public bool ReferencesMsDI { get; }


        public bool HasModuleInitializerAttribute { get; }

        public bool HasRequiredMemberAttribute { get; }

        public bool HasCliServiceProviderExtensions { get; }

        public bool HasCliServiceCollectionExtensions { get; }
        
        
        public override void Analyze(ISymbol symbol)
        {
        }

        public bool Equals(CliReferenceDependantInput other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;

            return UsesDotMakeCommandLine == other.UsesDotMakeCommandLine

                   && ReferencesMsDIAbstractions == other.ReferencesMsDIAbstractions
                   && ReferencesMsDI == other.ReferencesMsDI

                   && HasModuleInitializerAttribute == other.HasModuleInitializerAttribute
                   && HasRequiredMemberAttribute == other.HasRequiredMemberAttribute
                   && HasCliServiceProviderExtensions == other.HasCliServiceProviderExtensions
                   && HasCliServiceCollectionExtensions == other.HasCliServiceCollectionExtensions;
        }
    }
}
