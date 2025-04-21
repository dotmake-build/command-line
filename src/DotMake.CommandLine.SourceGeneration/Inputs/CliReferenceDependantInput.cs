using System;
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
            HasModuleInitializer = (compilation.GetTypeByMetadataName(ModuleInitializerAttributeFullName) != null);
            HasRequiredMember = (compilation.GetTypeByMetadataName(RequiredMemberAttributeFullName) != null);
            HasCliServiceProviderExtensions = (compilation.GetTypeByMetadataName(CliServiceProviderExtensionsFullName) != null);
            HasCliServiceCollectionExtensions = (compilation.GetTypeByMetadataName(CliServiceCollectionExtensionsFullName) != null);

            foreach (var referencedAssembly in compilation.SourceModule.ReferencedAssemblies)
            {
                switch (referencedAssembly.Name)
                {
                    case "Microsoft.Extensions.DependencyInjection.Abstractions":
                        if (referencedAssembly.Version >= new Version(2, 1, 1))
                            HasMsDependencyInjectionAbstractions = true;
                        break;
                    case "Microsoft.Extensions.DependencyInjection":
                        if (referencedAssembly.Version >= new Version(2, 1, 1))
                           HasMsDependencyInjection = true;
                        break;
                }
            }
        }

        public Compilation Compilation { get; }


        public bool HasModuleInitializer { get; }

        public bool HasRequiredMember { get; }

        public bool HasMsDependencyInjectionAbstractions { get; }

        public bool HasMsDependencyInjection { get; }

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

            return HasModuleInitializer == other.HasModuleInitializer
                   && HasRequiredMember == other.HasRequiredMember
                   && HasMsDependencyInjectionAbstractions == other.HasMsDependencyInjectionAbstractions
                   && HasMsDependencyInjection == other.HasMsDependencyInjection
                   && HasCliServiceProviderExtensions == other.HasCliServiceProviderExtensions
                   && HasCliServiceCollectionExtensions == other.HasCliServiceCollectionExtensions;
        }
    }
}
