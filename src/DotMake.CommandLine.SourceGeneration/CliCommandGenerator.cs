using Microsoft.CodeAnalysis;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Diagnostics;
using Microsoft.CodeAnalysis.Text;
using System.IO;
using System.Text;

namespace DotMake.CommandLine.SourceGeneration
{
    [Generator]
    public class CliCommandGenerator : IIncrementalGenerator
    {
        private static readonly Type Type = typeof(CliCommandGenerator);
        private static readonly string Version = Type.Assembly.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version;
        private static readonly string RoslynVersion = typeof(IIncrementalGenerator).Assembly.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version;
        private static readonly Dictionary<string, int> GenerationCounts = new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase);

        public void Initialize(IncrementalGeneratorInitializationContext initializationContext)
        {
            var analyzerConfigOptions = initializationContext.AnalyzerConfigOptionsProvider
                .Select((provider, _) => provider.GlobalOptions);

            var referenceDependantInfo = initializationContext.CompilationProvider
                .Select((compilation, _) => new ReferenceDependantInfo(compilation));

            var cliCommandInfos = initializationContext.SyntaxProvider.ForAttributeWithMetadataName(
                CliCommandInfo.AttributeFullName,
                (syntaxNode, _) => CliCommandInfo.IsMatch(syntaxNode),
                (attributeSyntaxContext, _) => CliCommandInfo.From(attributeSyntaxContext)
            );

            var cliCommandAsDelegateInfos = initializationContext.SyntaxProvider.CreateSyntaxProvider(
                (syntaxNode, _) => CliCommandAsDelegateInfo.IsMatch(syntaxNode),
                (generatorSyntaxContext, _) => CliCommandAsDelegateInfo.From(generatorSyntaxContext)
            );

            initializationContext.RegisterSourceOutput(
                referenceDependantInfo.Combine(analyzerConfigOptions),
                static (sourceProductionContext, tuple) => GenerateReferenceDependantSourceCode(sourceProductionContext, tuple.Left, tuple.Right)
            );
            initializationContext.RegisterSourceOutput(
                cliCommandInfos.Combine(analyzerConfigOptions),
                static (sourceProductionContext, tuple) => GenerateCommandBuilderSourceCode(sourceProductionContext, tuple.Left, tuple.Right)
            );
            initializationContext.RegisterSourceOutput(
                cliCommandAsDelegateInfos.Combine(analyzerConfigOptions),
                static (sourceProductionContext, tuple) => GenerateCliCommandAsDelegateSourceCode(sourceProductionContext, tuple.Left, tuple.Right)
            );
        }
        
        private static void GenerateReferenceDependantSourceCode(SourceProductionContext sourceProductionContext, ReferenceDependantInfo referenceDependantInfo, AnalyzerConfigOptions analyzerConfigOptions)
        {
            try
            {
                //Console.Beep(1000, 200); // For testing, how many times the generator is hit

                if (!CheckLanguage(referenceDependantInfo.Compilation))
                {
                    sourceProductionContext.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.ErrorUnsupportedLanguage, Location.None));
                    return;
                }
                if (!CheckLanguageVersion(referenceDependantInfo.Compilation.SyntaxTrees.FirstOrDefault()?.Options, out var currentVersion, out var supportedMinVersion))
                {
                    sourceProductionContext.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.ErrorUnsupportedLanguageVersion, Location.None, supportedMinVersion));
                    return;
                }

                //Note for sort order we use (FileName) instead of [FileName] for special classes so that they are displayed at top also in VS Analyzers node.
                //[] looked better, and it's on top in VS Folder nodes (which uses Windows File Explorer sorting rules) but not in VS Analyzers node (which seems to use ascii code order)
                //https://superuser.com/a/1560527

                //For supporting ModuleInitializerAttribute in projects before net5.0 (net472, netstandard2.0)
                if (!referenceDependantInfo.HasModuleInitializer)
                    sourceProductionContext.AddSource(
                        "(ModuleInitializerAttribute).g.cs",
                        GetSourceTextFromEmbeddedResource("ModuleInitializerAttribute.cs", analyzerConfigOptions)
                    );

                //For supporting Required modifier before net7.0 (need LangVersion 11)
                if (currentVersion > LanguageVersion.CSharp10 && !referenceDependantInfo.HasRequiredMember)
                    sourceProductionContext.AddSource(
                        "(RequiredMemberAttribute).g.cs",
                        GetSourceTextFromEmbeddedResource("RequiredMemberAttribute.cs", analyzerConfigOptions)
                    );

                if (referenceDependantInfo.HasMsDependencyInjectionAbstractions)
                    sourceProductionContext.AddSource(
                        "(CliServiceProviderExtensions).g.cs",
                        GetSourceTextFromEmbeddedResource("CliServiceProviderExtensions.cs", analyzerConfigOptions)
                    );

                if (referenceDependantInfo.HasMsDependencyInjection)
                    sourceProductionContext.AddSource(
                        "(CliServiceCollectionExtensions).g.cs",
                        GetSourceTextFromEmbeddedResource("CliServiceCollectionExtensions.cs", analyzerConfigOptions)
                    );

            }
            catch (Exception exception)
            {
                var diagnosticDescriptor = DiagnosticDescriptors.Create(exception);
                var diagnostic = Diagnostic.Create(diagnosticDescriptor, Location.None);

                sourceProductionContext.ReportDiagnosticSafe(diagnostic);
            }
        }


        private static void GenerateCommandBuilderSourceCode(SourceProductionContext sourceProductionContext, CliCommandInfo cliCommandInfo, AnalyzerConfigOptions analyzerConfigOptions)
        {
            try
            {
                //Console.Beep(1000, 200); // For testing, how many times the generator is hit

                if (!CheckLanguage(cliCommandInfo.SemanticModel.Compilation)
                    || !CheckLanguageVersion(cliCommandInfo.SyntaxNode.SyntaxTree.Options))
                    return;

                cliCommandInfo.ReportDiagnostics(sourceProductionContext);

                if (cliCommandInfo.HasProblem)
                    return;

                var sb = new CodeStringBuilder();
                AppendGeneratedCodeHeader(sb, cliCommandInfo.GeneratedClassFullName, analyzerConfigOptions);
                cliCommandInfo.AppendCSharpDefineString(sb, true);

                var generatedClassSourceCode = sb.ToString();

                //We need to use a stable hash to have a unique and short hintName.
                //Counting generated file names is not reliable, seems to sometimes run in parallel.
                //Using class full name can still collide because AddSource uses OrdinalIgnoreCase,
                //e.g. Namespace.Class1 and Namespace.class1 would collide
                //https://github.com/dotnet/roslyn/issues/48833
                var hash = cliCommandInfo.GeneratedClassFullName.GetStableStringHashCode32();
                var generatedFileName = $"{cliCommandInfo.GeneratedClassName}-{hash}.g.cs";

                sourceProductionContext.AddSource(generatedFileName, generatedClassSourceCode);
            }
            catch (Exception exception)
            {
                var diagnosticDescriptor = DiagnosticDescriptors.Create(exception);
                var diagnostic = Diagnostic.Create(diagnosticDescriptor, cliCommandInfo.Symbol.Locations.FirstOrDefault());

                sourceProductionContext.ReportDiagnosticSafe(diagnostic);
            }
        }

        private static void GenerateCliCommandAsDelegateSourceCode(SourceProductionContext sourceProductionContext, CliCommandAsDelegateInfo cliCommandAsDelegateInfo, AnalyzerConfigOptions analyzerConfigOptions)
        {
            try
            {
                //Console.Beep(1000, 200); // For testing, how many times the generator is hit

                if (cliCommandAsDelegateInfo == null)
                    return;

                if (!CheckLanguage(cliCommandAsDelegateInfo.SemanticModel.Compilation)
                    || !CheckLanguageVersion(cliCommandAsDelegateInfo.SyntaxNode.SyntaxTree.Options))
                    return;

                cliCommandAsDelegateInfo.ReportDiagnostics(sourceProductionContext);
                
                if (cliCommandAsDelegateInfo.HasProblem)
                    return;

                var sb = new CodeStringBuilder();
                AppendGeneratedCodeHeader(sb, cliCommandAsDelegateInfo.GeneratedClassFullName, analyzerConfigOptions);
                cliCommandAsDelegateInfo.AppendCSharpDefineString(sb);

                var generatedClassSourceCode = sb.ToString();

                sourceProductionContext.AddSource($"{cliCommandAsDelegateInfo.GeneratedClassName}.g.cs", generatedClassSourceCode);

                //Parse generated class to generate a builder for it
                var syntaxTree = CSharpSyntaxTree.ParseText(
                    generatedClassSourceCode,
                    (CSharpParseOptions)cliCommandAsDelegateInfo.SyntaxNode.SyntaxTree.Options
                );
                var compilation = cliCommandAsDelegateInfo.SemanticModel.Compilation.AddSyntaxTrees(syntaxTree);
                var generatedSymbol = compilation.GetTypeByMetadataName(cliCommandAsDelegateInfo.GeneratedClassFullName);
                var cliCommandInfo = new CliCommandInfo(
                    generatedSymbol,
                    generatedSymbol?.DeclaringSyntaxReferences.FirstOrDefault().GetSyntax(),
                    generatedSymbol?.GetAttributes().FirstOrDefault(a => a.AttributeClass.ToCompareString() == CliCommandInfo.AttributeFullName),
                    compilation.GetSemanticModel(syntaxTree),
                    null
                    );
                GenerateCommandBuilderSourceCode(sourceProductionContext, cliCommandInfo, analyzerConfigOptions);
            }
            catch (Exception exception)
            {
                var diagnosticDescriptor = DiagnosticDescriptors.Create(exception);
                var diagnostic = Diagnostic.Create(diagnosticDescriptor, Location.None);

                sourceProductionContext.ReportDiagnosticSafe(diagnostic);
            }
        }

        private static bool CheckLanguage(Compilation compilation)
        {
            return (compilation.Language == LanguageNames.CSharp);
        }

        private static bool CheckLanguageVersion(ParseOptions parseOptions)
        {
            return CheckLanguageVersion(parseOptions, out _, out _);

        }

        private static bool CheckLanguageVersion(ParseOptions parseOptions, out LanguageVersion currentVersion, out string supportedMinVersion)
        {
            currentVersion = 0;
            supportedMinVersion = "9.0";

            // Error CS8370	Feature 'nullable reference types' is not available in C# 7.3. Please use language version 8.0 or greater.
            // Error CSS8370 Feature 'module initializers' is not available in C# 7.3. Please use language version 9.0 or greater.
            return (parseOptions is CSharpParseOptions options
                    && (currentVersion = options.LanguageVersion) >= LanguageVersion.CSharp9);
        }

        private static void AppendGeneratedCodeHeader(CodeStringBuilder sb, string generationKey, AnalyzerConfigOptions analyzerConfigOptions)
        {
            //Ensure generation is counted separately for different projects and target frameworks.
            if (analyzerConfigOptions.TryGetValue("build_property.projectdir", out var projectDir))
                generationKey += "|" + projectDir;
            if (analyzerConfigOptions.TryGetValue("build_property.targetframework", out var targetFramework))
                generationKey += "|" + targetFramework;
            if (GenerationCounts.TryGetValue(generationKey, out var generationCount))
                GenerationCounts[generationKey] = ++generationCount;
            else
                GenerationCounts.Add(generationKey, ++generationCount);

            sb.AppendLine("// <auto-generated />");
            sb.AppendLine($"// Generated by {Type.Namespace} v{Version}");
            sb.AppendLine($"// Roslyn (Microsoft.CodeAnalysis) v{RoslynVersion}");
            sb.AppendLine($"// Generation: {generationCount}");
            //add time only for debug as it causes unnecessary changes in source control in TestApp with EmitCompilerGeneratedFiles
            //sb.AppendLine($"// Time: {DateTime.Now:o}, Generation: {generationCount}");
            sb.AppendLine();
        }

        private static SourceText GetSourceTextFromEmbeddedResource(string fileName, AnalyzerConfigOptions analyzerConfigOptions)
        {
            var resourceName = $"{Type.Namespace}.Embedded.{fileName}";

            using (var resourceStream = Type.Assembly.GetManifestResourceStream(resourceName))
            {
                if (resourceStream == null)
                    throw new Exception($"Embedded resource '{fileName}' is not found in assembly '{Type.Assembly}'.");

                using (var streamReader = new StreamReader(resourceStream))
                {
                    var sb = new CodeStringBuilder();

                    AppendGeneratedCodeHeader(sb, resourceName, analyzerConfigOptions);
                    sb.AppendLine(streamReader.ReadToEnd());

                    return SourceText.From(sb.ToString(), Encoding.UTF8);
                }
            }
        }
    }
}
