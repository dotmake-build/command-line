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
using DotMake.CommandLine.SourceGeneration.Inputs;
using DotMake.CommandLine.SourceGeneration.Outputs;
using DotMake.CommandLine.SourceGeneration.Util;
using DotMake.CommandLine.Util;

namespace DotMake.CommandLine.SourceGeneration
{
    [Generator]
    public class CliIncrementalGenerator : IIncrementalGenerator
    {
        private static readonly Type Type = typeof(CliIncrementalGenerator);
        private static readonly string Version = Type.Assembly.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version;
        private static readonly string RoslynVersion = typeof(IIncrementalGenerator).Assembly.GetCustomAttribute<AssemblyFileVersionAttribute>()?.Version;
        private static readonly Dictionary<string, int> GenerationCounts = new(StringComparer.OrdinalIgnoreCase);

        public void Initialize(IncrementalGeneratorInitializationContext initializationContext)
        {
            var analyzerConfigOptions = initializationContext.AnalyzerConfigOptionsProvider
                .Select((provider, _) => provider.GlobalOptions);

            var cliReferenceDependantInput = initializationContext.CompilationProvider
                .Select((compilation, _) => new CliReferenceDependantInput(compilation));

            var cliCommandInputs = initializationContext.SyntaxProvider.ForAttributeWithMetadataName(
                CliCommandInput.AttributeFullName,
                (syntaxNode, _) => CliCommandInput.IsMatch(syntaxNode),
                (attributeSyntaxContext, _) => CliCommandInput.From(attributeSyntaxContext)
            );

            var cliCommandAsDelegateInputs = initializationContext.SyntaxProvider.CreateSyntaxProvider(
                (syntaxNode, _) => CliCommandAsDelegateInput.IsMatch(syntaxNode),
                (generatorSyntaxContext, _) => CliCommandAsDelegateInput.From(generatorSyntaxContext)
            );

            initializationContext.RegisterSourceOutput(
                cliReferenceDependantInput.Combine(analyzerConfigOptions),
                static (sourceProductionContext, tuple) => GenerateReferenceDependantSourceCode(sourceProductionContext, tuple.Left, tuple.Right)
            );
            initializationContext.RegisterSourceOutput(
                cliCommandInputs.Combine(cliReferenceDependantInput).Combine(analyzerConfigOptions),
                static (sourceProductionContext, tuple) => GenerateCommandBuilderSourceCode(sourceProductionContext, tuple.Left.Left, tuple.Left.Right, tuple.Right)
            );
            initializationContext.RegisterSourceOutput(
                cliCommandAsDelegateInputs.Combine(cliReferenceDependantInput).Combine(analyzerConfigOptions),
                static (sourceProductionContext, tuple) => GenerateCliCommandAsDelegateSourceCode(sourceProductionContext, tuple.Left.Left, tuple.Left.Right, tuple.Right)
            );
        }
        
        private static void GenerateReferenceDependantSourceCode(SourceProductionContext sourceProductionContext, CliReferenceDependantInput cliReferenceDependantInput, AnalyzerConfigOptions analyzerConfigOptions)
        {
            try
            {
                //Console.Beep(1000, 200); // For testing, how many times the generator is hit

                if (!CheckLanguage(cliReferenceDependantInput.Language))
                {
                    sourceProductionContext.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.ErrorUnsupportedLanguage, Location.None));
                    return;
                }
                if (!CheckLanguageVersion(cliReferenceDependantInput.LanguageVersion, out var supportedMinVersion))
                {
                    sourceProductionContext.ReportDiagnostic(Diagnostic.Create(DiagnosticDescriptors.ErrorUnsupportedLanguageVersion, Location.None, supportedMinVersion));
                    return;
                }

                //Note for sort order we use (FileName) instead of [FileName] for special classes so that they are displayed at top also in VS Analyzers node.
                //[] looked better, and it's on top in VS Folder nodes (which uses Windows File Explorer sorting rules) but not in VS Analyzers node (which seems to use ascii code order)
                //https://superuser.com/a/1560527

                //For supporting ModuleInitializerAttribute in projects before net5.0 (net472, netstandard2.0)
                if (!cliReferenceDependantInput.HasModuleInitializer)
                {
                    var name = SymbolExtensions.GetName(CliReferenceDependantInput.ModuleInitializerAttributeFullName);
                    sourceProductionContext.AddSource(
                        $"({name}).g.cs",
                        GetSourceTextFromEmbeddedResource($"{name}.cs", analyzerConfigOptions)
                    );
                }

                //For supporting Required modifier before net7.0 (need LangVersion 11)
                if (cliReferenceDependantInput.LanguageVersion > (int)LanguageVersion.CSharp10 && !cliReferenceDependantInput.HasRequiredMember)
                {
                    var name = SymbolExtensions.GetName(CliReferenceDependantInput.RequiredMemberAttributeFullName);
                    sourceProductionContext.AddSource(
                        $"({name}).g.cs",
                        GetSourceTextFromEmbeddedResource($"{name}.cs", analyzerConfigOptions)
                    );
                }

                if (cliReferenceDependantInput.HasMsDependencyInjectionAbstractions
                    && !cliReferenceDependantInput.HasCliServiceProviderExtensions)
                {
                    var name = SymbolExtensions.GetName(CliReferenceDependantInput.CliServiceProviderExtensionsFullName);
                    sourceProductionContext.AddSource(
                        $"({name}).g.cs",
                        GetSourceTextFromEmbeddedResource($"{name}.cs", analyzerConfigOptions)
                    );
                }

                if (cliReferenceDependantInput.HasMsDependencyInjection
                    && !cliReferenceDependantInput.HasCliServiceCollectionExtensions)
                {
                    var name = SymbolExtensions.GetName(CliReferenceDependantInput.CliServiceCollectionExtensionsFullName);
                    sourceProductionContext.AddSource(
                        $"({name}).g.cs",
                        GetSourceTextFromEmbeddedResource($"{name}.cs", analyzerConfigOptions)
                    );
                }
            }
            catch (Exception exception)
            {
                var diagnosticDescriptor = DiagnosticDescriptors.Create(exception);
                var diagnostic = Diagnostic.Create(diagnosticDescriptor, Location.None);

                sourceProductionContext.ReportDiagnosticSafe(diagnostic);
            }
        }

        private static void GenerateCommandBuilderSourceCode(SourceProductionContext sourceProductionContext, CliCommandInput cliCommandInput, CliReferenceDependantInput cliReferenceDependantInput, AnalyzerConfigOptions analyzerConfigOptions)
        {
            try
            {
                //Console.Beep(1000, 200); // For testing, how many times the generator is hit

                if (!CheckLanguage(cliCommandInput.Language)
                    || !CheckLanguageVersion(cliCommandInput.LanguageVersion))
                    return;

                var cliCommandOutput = new CliCommandOutput(cliCommandInput, cliReferenceDependantInput);
                cliCommandOutput.ReportDiagnostics(sourceProductionContext);

                if (cliCommandInput.HasProblem) //This should be checked after creating CliCommandOutput, as we may add some problems there
                    return;

                var sb = new CodeStringBuilder();
                AppendGeneratedCodeHeader(sb, cliCommandOutput.GeneratedClassFullName, analyzerConfigOptions);
                cliCommandOutput.AppendCSharpDefineString(sb, true);

                var generatedClassSourceCode = sb.ToString();

                //We need to use a stable hash to have a unique and short hintName.
                //Counting generated file names is not reliable, seems to sometimes run in parallel.
                //Using class full name can still collide because AddSource uses OrdinalIgnoreCase,
                //e.g. Namespace.Class1 and Namespace.class1 would collide
                //https://github.com/dotnet/roslyn/issues/48833
                var hash = cliCommandOutput.GeneratedClassFullName.GetStableStringHashCode32();
                var generatedFileName = $"{cliCommandOutput.GeneratedClassName}-{hash}.g.cs";

                sourceProductionContext.AddSource(generatedFileName, generatedClassSourceCode);
            }
            catch (Exception exception)
            {
                var diagnosticDescriptor = DiagnosticDescriptors.Create(exception);
                var diagnostic = Diagnostic.Create(diagnosticDescriptor, cliCommandInput.Location);

                sourceProductionContext.ReportDiagnosticSafe(diagnostic);
            }
        }

        private static void GenerateCliCommandAsDelegateSourceCode(SourceProductionContext sourceProductionContext, CliCommandAsDelegateInput cliCommandAsDelegateInput, CliReferenceDependantInput cliReferenceDependantInput, AnalyzerConfigOptions analyzerConfigOptions)
        {
            try
            {
                //Console.Beep(1000, 200); // For testing, how many times the generator is hit

                if (cliCommandAsDelegateInput == null)
                    return;

                if (!CheckLanguage(cliCommandAsDelegateInput.Language)
                    || !CheckLanguageVersion(cliCommandAsDelegateInput.LanguageVersion))
                    return;

                var cliCommandAsDelegateOutput = new CliCommandAsDelegateOutput(cliCommandAsDelegateInput);
                cliCommandAsDelegateOutput.ReportDiagnostics(sourceProductionContext);
                
                if (cliCommandAsDelegateInput.HasProblem)
                    return;

                var sb = new CodeStringBuilder();
                AppendGeneratedCodeHeader(sb, cliCommandAsDelegateOutput.GeneratedClassFullName, analyzerConfigOptions);
                cliCommandAsDelegateOutput.AppendCSharpDefineString(sb);

                var generatedClassSourceCode = sb.ToString();

                sourceProductionContext.AddSource($"{cliCommandAsDelegateOutput.GeneratedClassName}.g.cs", generatedClassSourceCode);

                //Parse generated class to generate a builder for it
                var syntaxTree = CSharpSyntaxTree.ParseText(
                    generatedClassSourceCode,
                    (CSharpParseOptions)cliCommandAsDelegateInput.SyntaxNode.SyntaxTree.Options
                );
                var compilation = cliCommandAsDelegateInput.SemanticModel.Compilation.AddSyntaxTrees(syntaxTree);
                var generatedSymbol = compilation.GetTypeByMetadataName(cliCommandAsDelegateOutput.GeneratedClassFullName);
                var cliCommandInput = new CliCommandInput(
                    generatedSymbol,
                    generatedSymbol?.DeclaringSyntaxReferences.FirstOrDefault().GetSyntax(),
                    generatedSymbol?.GetAttributes().FirstOrDefault(a => a.AttributeClass.ToCompareString() == CliCommandInput.AttributeFullName),
                    compilation.GetSemanticModel(syntaxTree),
                    null
                    );
                GenerateCommandBuilderSourceCode(sourceProductionContext, cliCommandInput, cliReferenceDependantInput, analyzerConfigOptions);
            }
            catch (Exception exception)
            {
                var diagnosticDescriptor = DiagnosticDescriptors.Create(exception);
                var diagnostic = Diagnostic.Create(diagnosticDescriptor, cliCommandAsDelegateInput?.Location);

                sourceProductionContext.ReportDiagnosticSafe(diagnostic);
            }
        }

        private static bool CheckLanguage(string language)
        {
            return (language == LanguageNames.CSharp);
        }

        private static bool CheckLanguageVersion(int languageVersion)
        {
            return CheckLanguageVersion(languageVersion, out _);

        }

        private static bool CheckLanguageVersion(int languageVersion, out string supportedMinVersion)
        {
            supportedMinVersion = "9.0";

            // Error CS8370	Feature 'nullable reference types' is not available in C# 7.3. Please use language version 8.0 or greater.
            // Error CSS8370 Feature 'module initializers' is not available in C# 7.3. Please use language version 9.0 or greater.
            return (languageVersion >= (int)LanguageVersion.CSharp9);
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
