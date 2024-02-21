using System;
using Microsoft.CodeAnalysis;

namespace DotMake.CommandLine.SourceGeneration
{
    public class ReferenceDependantInfo : IEquatable<ReferenceDependantInfo>
    {
        public ReferenceDependantInfo(Compilation compilation)
        {
            Compilation = compilation;
            HasModuleInitializer = (compilation.GetTypeByMetadataName("System.Runtime.CompilerServices.ModuleInitializerAttribute") != null);
            HasRequiredMember = (compilation.GetTypeByMetadataName("System.Runtime.CompilerServices.RequiredMemberAttribute") != null);

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

        public bool Equals(ReferenceDependantInfo other)
        {
            if (ReferenceEquals(null, other))
                return false;
            if (ReferenceEquals(this, other))
                return true;

            return HasModuleInitializer == other.HasModuleInitializer
                   && HasRequiredMember == other.HasRequiredMember
                   && HasMsDependencyInjectionAbstractions == other.HasMsDependencyInjectionAbstractions
                   && HasMsDependencyInjection == other.HasMsDependencyInjection;
        }
    }
}
