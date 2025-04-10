using System;
using System.Collections.Generic;
using Microsoft.CodeAnalysis;

namespace DotMake.CommandLine.SourceGeneration.Inputs
{
    public class CliReferenceDependantInput : InputBase, IEquatable<CliReferenceDependantInput>
    {
        private static readonly HashSet<string> VisitedCompilationAssemblies = new (StringComparer.OrdinalIgnoreCase);

        public CliReferenceDependantInput(Compilation compilation)
            : base(compilation)
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
                    default:
                        /*
                          We need to inject feature extensions once in child project because if they are also
                          injected in parent projects, class conflict errors occur (due to same namespace in different assemblies)
                          CliServiceProviderExtensions and CliServiceCollectionExtensions needs this check as they are used by the user.
                          ModuleInitializerAttribute and RequiredMemberAttribute does not need this check as they are used by compiler only,
                          and they are needed for each assembly.

                          This can't be fixed via PackageReference because;
                          Default value for PrivateAssets in PackageReference is "contentfiles;analyzers;build"
                          However, source generator still flows to the parent project via ProjectReference.
                          One solution is to use
                            <PackageReference Include="DotMake.CommandLine" PrivateAssets="all" />
                          Although this prevents flow of source generator, it also prevents flow of "compile"
                          so it becomes useless.

                          First attempt to fix (in v1.8.3), was making CliServiceProviderExtensions and CliServiceCollectionExtensions
                          classes internal but the problem resurfaces if user adds InternalsVisibleTo attribute in child project.

                          So we finally solve this problem, by detecting if current compilation is a parent project in the solution,
                          if so we do not inject feature extensions as they already come transitively from the child project.
                          This way we can also keep CliServiceProviderExtensions and CliServiceCollectionExtensions classes public.
                        */
                        if (VisitedCompilationAssemblies.Contains(referencedAssembly.ToString()))
                            IsParentCompilation = true;
                        break;
                }
            }

            VisitedCompilationAssemblies.Add(compilation.Assembly.Identity.ToString());
        }

        public Compilation Compilation { get; }

        public bool HasModuleInitializer { get; }

        public bool HasRequiredMember { get; }

        public bool HasMsDependencyInjectionAbstractions { get; }

        public bool HasMsDependencyInjection { get; }

        public bool IsParentCompilation { get; }

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
                   && IsParentCompilation == other.IsParentCompilation;
        }
    }
}
