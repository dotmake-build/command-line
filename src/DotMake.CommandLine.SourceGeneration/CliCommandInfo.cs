using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DotMake.CommandLine.SourceGeneration
{
    public class CliCommandInfo : CliSymbolInfo, IEquatable<CliCommandInfo>
    {
        public static readonly string AttributeFullName = typeof(CliCommandAttribute).FullName;
        public static readonly string[] Suffixes = { "RootCliCommand", "RootCommand", "SubCliCommand", "SubCommand", "CliCommand", "Command", "Cli" };
        public const string RootCommandClassName = "CliRootCommand";
        public const string CommandClassName = "CliCommand";
        public const string CommandClassNamespace = "System.CommandLine";
        public const string DiagnosticName = "CLI command";
        public const string GeneratedSubNamespace = "GeneratedCode";
        public const string GeneratedClassSuffix = "Builder";
        public static readonly string CommandBuilderFullName = "DotMake.CommandLine.CliCommandBuilder";
        public static readonly Dictionary<string, string> PropertyMappings = new Dictionary<string, string>
        {
            //{ nameof(CliCommandAttribute.Hidden), "IsHidden"}
        };
        public readonly HashSet<string> UsedAliases = new HashSet<string>(StringComparer.Ordinal);

        public CliCommandInfo(ISymbol symbol, SyntaxNode syntaxNode, AttributeData attributeData, SemanticModel semanticModel, CliCommandInfo parent)
            : base(symbol, syntaxNode, semanticModel)
        {
            Symbol = (INamedTypeSymbol)symbol;
            Parent = parent;

            AttributeArguments = new AttributeArguments(attributeData);
            Settings = CliCommandSettings.Parse(Symbol, AttributeArguments);
            if (parent != null) //Nested class for sub-command
            {
                Settings.ParentSettings = parent.Settings;
                Settings.ParentSymbol = parent.Symbol;
            }
            else //External class for sub-command
            {
                Settings.PopulateParentTree();
            }

            IsRoot = (parent == null) && (Settings.ParentSymbol == null);
            IsExternalChild = (parent == null) && (Settings.ParentSymbol != null);

            GeneratedClassName = symbol.Name + GeneratedClassSuffix;
            GeneratedClassNamespace = symbol.GetNamespaceOrEmpty();
            if (!GeneratedClassNamespace.EndsWith(GeneratedSubNamespace))
                GeneratedClassNamespace = SymbolExtensions.CombineNameParts(GeneratedClassNamespace, GeneratedSubNamespace);
            GeneratedClassFullName = Settings.IsParentContaining
                ? SymbolExtensions.CombineNameParts(Settings.GetContainingTypeFullName(GeneratedSubNamespace, GeneratedClassSuffix), GeneratedClassName)
                : SymbolExtensions.CombineNameParts(GeneratedClassNamespace, GeneratedClassName);
            
            ReferenceDependantInfo = (parent != null)
                ? parent.ReferenceDependantInfo
                : new ReferenceDependantInfo(semanticModel.Compilation);

            Analyze();

            if (HasProblem)
                return;

            var addedPropertyNames = new HashSet<string>(StringComparer.Ordinal);
            var visitedProperties = new Dictionary<string, ISymbol>(StringComparer.Ordinal);
            var parentCommandsByType = Settings
                .GetParentTree()
                .Select(settings => settings.Symbol)
                .ToImmutableHashSet(SymbolEqualityComparer.Default);

            //Loop through all own and then inherited members (not distinct)
            foreach (var member in Symbol.GetAllMembers())
            {
                if (member is IPropertySymbol property)
                {
                    if (addedPropertyNames.Contains(property.Name))
                        continue;

                    //Property with [CliOption] or [CliArgument] attribute
                    foreach (var propertyAttributeData in property.GetAttributes())
                    {
                        var attributeFullName = propertyAttributeData.AttributeClass?.ToCompareString();

                        //visitedProperty is used to inherit [CliOption] or [CliArgument] attribute
                        //for example, property does not have an attribute in child class but has one in a parent class.
                        //The property attribute and the property initializer from the most derived class in the hierarchy
                        //will be used (they will override the base ones).
                        //future note: non-existing property initializer in derived may be overriding an existing one in base
                        visitedProperties.TryGetValue(property.Name, out var visitedProperty);

                        //having both attributes doesn't make sense as the binding would override the previous one's value
                        //user should better have separate properties for each attribute
                        //so stop (break) when one of the attributes found first on a property.
                        if (attributeFullName == CliOptionInfo.AttributeFullName)
                        {
                            options.Add(new CliOptionInfo(visitedProperty ?? property, null, propertyAttributeData, SemanticModel, this));
                            addedPropertyNames.Add(property.Name);

                            break; 
                        }
                        if (attributeFullName == CliArgumentInfo.AttributeFullName)
                        {
                            arguments.Add(new CliArgumentInfo(visitedProperty ?? property, null, propertyAttributeData, SemanticModel, this));
                            addedPropertyNames.Add(property.Name);

                            break;
                        }
                    }

                    //Property with a type that refers to a parent command
                    if (!addedPropertyNames.Contains(property.Name)
                        && parentCommandsByType.Contains(property.Type))
                    {
                        parentCommandAccessors.Add(new CliParentCommandAccessorInfo(property, null, SemanticModel));
                        addedPropertyNames.Add(property.Name);
                    }

                    if (!visitedProperties.ContainsKey(property.Name))
                        visitedProperties.Add(property.Name, property);
                }
                else if (member is IMethodSymbol method)
                {
                    if (CliCommandHandlerInfo.HasCorrectName(method))
                    {
                        var possibleHandler = new CliCommandHandlerInfo(method, null, SemanticModel, this);
                        if (possibleHandler.HasCorrectSignature
                            && (Handler == null || possibleHandler.SignaturePriority > Handler.SignaturePriority))
                            Handler = possibleHandler;
                    }
                }
            }

            //Disable warning for missing handler, instead show help when no handler
            //if (Handler == null)
            //    AddDiagnostic(DiagnosticDescriptors.WarningClassHasNotHandler, false, CliCommandHandlerInfo.DiagnosticName);

            foreach (var nestedType in Symbol.GetTypeMembers())
            {
                foreach (var memberAttributeData in nestedType.GetAttributes())
                {
                    if (memberAttributeData.AttributeClass?.ToCompareString() == AttributeFullName)
                        subcommands.Add(new CliCommandInfo(nestedType, null, memberAttributeData, SemanticModel, this));
                }
            }
        }

        public static bool IsMatch(SyntaxNode syntaxNode)
        {
            return syntaxNode is ClassDeclarationSyntax
                   //skip nested classes as they will be handled by the parent classes
                   && !(syntaxNode.Parent is TypeDeclarationSyntax);
        }

        public static CliCommandInfo From(GeneratorAttributeSyntaxContext attributeSyntaxContext)
        {
            return new(attributeSyntaxContext.TargetSymbol,
                attributeSyntaxContext.TargetNode,
                attributeSyntaxContext.Attributes[0],
                attributeSyntaxContext.SemanticModel,
                null);
        }

        public new INamedTypeSymbol Symbol { get; }

        public AttributeArguments AttributeArguments { get; }

        public CliCommandSettings Settings { get; }

        public CliCommandInfo Parent { get; }

        public bool IsRoot { get; }

        public bool IsExternalChild { get; }

        public CliCommandHandlerInfo Handler { get; }

        public string GeneratedClassName { get; }

        public string GeneratedClassNamespace { get; }

        public string GeneratedClassFullName { get; }

        public ReferenceDependantInfo ReferenceDependantInfo { get; }

        public IReadOnlyList<CliOptionInfo> Options => options;
        private readonly List<CliOptionInfo> options = new List<CliOptionInfo>();

        public IReadOnlyList<CliArgumentInfo> Arguments => arguments;
        private readonly List<CliArgumentInfo> arguments = new List<CliArgumentInfo>();

        public IReadOnlyList<CliCommandInfo> Subcommands => subcommands;
        private readonly List<CliCommandInfo> subcommands = new List<CliCommandInfo>();

        public IReadOnlyList<CliParentCommandAccessorInfo> ParentCommandAccessors => parentCommandAccessors;
        private readonly List<CliParentCommandAccessorInfo> parentCommandAccessors = new List<CliParentCommandAccessorInfo>();

        private void Analyze()
        {
            if ((Symbol.DeclaredAccessibility != Accessibility.Public && Symbol.DeclaredAccessibility != Accessibility.Internal)
                || Symbol.IsStatic)
                AddDiagnostic(DiagnosticDescriptors.WarningClassNotPublicNonStatic, DiagnosticName);
            else
            {
                if (Symbol.IsAbstract || Symbol.IsGenericType)
                    AddDiagnostic(DiagnosticDescriptors.ErrorClassNotNonAbstractNonGeneric, DiagnosticName);

                if (!ReferenceDependantInfo.HasMsDependencyInjectionAbstractions
                    && !Symbol.InstanceConstructors.Any(c =>
                        c.Parameters.IsEmpty
                        && (c.DeclaredAccessibility == Accessibility.Public || c.DeclaredAccessibility == Accessibility.Internal)
                    ))
                    AddDiagnostic(DiagnosticDescriptors.ErrorClassHasNotPublicDefaultConstructor, DiagnosticName);

                if (IsExternalChild)
                {
                    var circularParent = Settings.GetParentTree().Prepend(Settings).FirstOrDefault(s => s.IsParentCircular);
                    if (circularParent != null)
                        AddDiagnostic(DiagnosticDescriptors.ErrorClassCircularDependency, circularParent.Symbol.Name);
                    else if (Settings.ParentSettings == null)
                        AddDiagnostic(DiagnosticDescriptors.ErrorParentClassHasNotAttribute, DiagnosticName, nameof(CliCommandAttribute));
                }
            }
        }

        public override void ReportDiagnostics(SourceProductionContext sourceProductionContext)
        {
            base.ReportDiagnostics(sourceProductionContext); //self

            Handler?.ReportDiagnostics(sourceProductionContext);

            foreach (var child in Options)
                child.ReportDiagnostics(sourceProductionContext);

            foreach (var child in Arguments)
                child.ReportDiagnostics(sourceProductionContext);

            foreach (var child in Subcommands)
                child.ReportDiagnostics(sourceProductionContext);

            foreach (var child in ParentCommandAccessors)
                child.ReportDiagnostics(sourceProductionContext);
        }

        public void AppendCSharpDefineString(CodeStringBuilder sb, bool addNamespaceBlock)
        {
            var optionsWithoutProblem = Options.Where(c => !c.HasProblem).ToArray();
            var argumentsWithoutProblem = Arguments.Where(c => !c.HasProblem).ToArray();
            var subcommandsWithoutProblem = Subcommands.Where(c => !c.HasProblem).ToArray();
            var parentCommandAccessorsWithoutProblem = ParentCommandAccessors.Where(c => !c.HasProblem).ToArray();
            var handlerWithoutProblem = (Handler != null && !Handler.HasProblem) ? Handler : null;
            var memberHasRequiredModifier = optionsWithoutProblem.Any(o => o.Symbol.IsRequired)
                                            || argumentsWithoutProblem.Any(a => a.Symbol.IsRequired)
                                            || parentCommandAccessorsWithoutProblem.Any(r => r.Symbol.IsRequired);

            if (string.IsNullOrEmpty(GeneratedClassNamespace))
                addNamespaceBlock = false;

            using var namespaceBlock = addNamespaceBlock ? sb.AppendBlockStart($"namespace {GeneratedClassNamespace}") : null;
            sb.AppendLine("/// <inheritdoc />");
            using (sb.AppendBlockStart($"public class {GeneratedClassName} : {CommandBuilderFullName}"))
            {
                var varCommand = (IsRoot ? "rootCommand" : "command");
                var definitionClass = Symbol.ToReferenceString();
                var parentDefinitionClass = IsRoot ? null : Settings.ParentSymbol.ToReferenceString();
                var parentDefinitionType = (parentDefinitionClass != null) ? $"typeof({parentDefinitionClass})" : "null";

                sb.AppendLine("/// <inheritdoc />");
                using (sb.AppendBlockStart($"public {GeneratedClassName}()"))
                {
                    sb.AppendLine($"DefinitionType = typeof({definitionClass});");
                    sb.AppendLine($"ParentDefinitionType = {parentDefinitionType};");

                    sb.AppendLine($"NameCasingConvention = {EnumUtil<CliNameCasingConvention>.ToFullName(Settings.NameCasingConvention)};");
                    sb.AppendLine($"NamePrefixConvention = {EnumUtil<CliNamePrefixConvention>.ToFullName(Settings.NamePrefixConvention)};");
                    sb.AppendLine($"ShortFormPrefixConvention = {EnumUtil<CliNamePrefixConvention>.ToFullName(Settings.ShortFormPrefixConvention)};");
                    sb.AppendLine($"ShortFormAutoGenerate = {Settings.ShortFormAutoGenerate.ToString().ToLowerInvariant()};");

                }
                sb.AppendLine();

                using (sb.AppendBlockStart($"private {definitionClass} CreateInstance()"))
                {
                    if (ReferenceDependantInfo.HasMsDependencyInjectionAbstractions || ReferenceDependantInfo.HasMsDependencyInjection)
                    {
                        sb.AppendLine(ReferenceDependantInfo.HasMsDependencyInjection
                            ? "var serviceProvider = DotMake.CommandLine.CliServiceCollectionExtensions.GetServiceProviderOrDefault(null);"
                            : "var serviceProvider = DotMake.CommandLine.CliServiceProviderExtensions.GetServiceProvider(null);");
                        sb.AppendLine("if (serviceProvider != null)");
                        sb.AppendIndent();
                        sb.AppendLine("return Microsoft.Extensions.DependencyInjection.ActivatorUtilities");
                        sb.AppendIndent();
                        sb.AppendIndent();
                        sb.AppendLine($".CreateInstance<{definitionClass}>(serviceProvider);");
                        //in case serviceProvider is null (i.e. not set with SetServiceProvider)
                        //call Activator.CreateInstance which will throw exception if class has no default constructor
                        //but at least it avoids compile time error in generated code with new()
                        sb.AppendLine();
                        sb.AppendLine($"return System.Activator.CreateInstance<{definitionClass}>();");
                    }
                    else
                        sb.AppendLine(memberHasRequiredModifier
                            ? $"return System.Activator.CreateInstance<{definitionClass}>();"
                            : $"return new {definitionClass}();");
                }
                sb.AppendLine();

                sb.AppendLine("/// <inheritdoc />");
                using (sb.AppendBlockStart($"public override {CommandClassNamespace}.{CommandClassName} Build()"))
                {
                    AppendCSharpCreateString(sb, varCommand);

                    sb.AppendLine();
                    var varDefaultClass = "defaultClass";
                    sb.AppendLine($"var {varDefaultClass} = CreateInstance();");

                    for (var index = 0; index < optionsWithoutProblem.Length; index++)
                    {
                        sb.AppendLine();

                        var cliOptionInfo = optionsWithoutProblem[index];
                        var varOption = $"option{index}";
                        cliOptionInfo.AppendCSharpCreateString(sb, varOption,
                            $"{varDefaultClass}.{cliOptionInfo.Symbol.Name}");
                        sb.AppendLine($"{varCommand}.Add({varOption});");
                    }

                    for (var index = 0; index < argumentsWithoutProblem.Length; index++)
                    {
                        sb.AppendLine();

                        var cliArgumentInfo = argumentsWithoutProblem[index];
                        var varArgument = $"argument{index}";
                        cliArgumentInfo.AppendCSharpCreateString(sb, varArgument,
                            $"{varDefaultClass}.{cliArgumentInfo.Symbol.Name}");
                        sb.AppendLine($"{varCommand}.Add({varArgument});");
                    }

                    sb.AppendLine();
                    sb.AppendLine("// Add nested or external registered children");
                    using (sb.AppendBlockStart("foreach (var child in Children)"))
                    {
                        sb.AppendLine($"{varCommand}.Add(child.Build());");
                    }

                    sb.AppendLine();
                    var varParseResult = "parseResult";
                    using (sb.AppendBlockStart($"Binder = ({varParseResult}) =>", ";"))
                    {
                        var varTargetClass = "targetClass";

                        sb.AppendLine($"var {varTargetClass} = CreateInstance();");

                        sb.AppendLine();
                        sb.AppendLine("//  Set the parsed or default values for the options");
                        for (var index = 0; index < optionsWithoutProblem.Length; index++)
                        {
                            var cliOptionInfo = optionsWithoutProblem[index];
                            var varOption = $"option{index}";
                            sb.AppendLine($"{varTargetClass}.{cliOptionInfo.Symbol.Name} = GetValueForOption({varParseResult}, {varOption});");
                        }

                        sb.AppendLine();
                        sb.AppendLine("//  Set the parsed or default values for the arguments");
                        for (var index = 0; index < argumentsWithoutProblem.Length; index++)
                        {
                            var cliArgumentInfo = argumentsWithoutProblem[index];
                            var varArgument = $"argument{index}";
                            sb.AppendLine($"{varTargetClass}.{cliArgumentInfo.Symbol.Name} = GetValueForArgument({varParseResult}, {varArgument});");
                        }

                        sb.AppendLine();
                        sb.AppendLine("//  Set the values for the parent command accessors");
                        foreach (var cliParentCommandAccessorInfo in parentCommandAccessorsWithoutProblem)
                        {
                            sb.AppendLine($"{varTargetClass}.{cliParentCommandAccessorInfo.Symbol.Name} = DotMake.CommandLine.ParseResultExtensions");
                            sb.AppendIndent();
                            sb.AppendLine($".Bind<{cliParentCommandAccessorInfo.Symbol.Type.ToReferenceString()}>({varParseResult});");
                        }

                        sb.AppendLine();
                        sb.AppendLine($"return {varTargetClass};");
                    }

                    sb.AppendLine();
                    var varCancellationToken = "cancellationToken";
                    var varCliContext = "cliContext";
                    var isAsync = (handlerWithoutProblem != null && handlerWithoutProblem.IsAsync);
                    using (sb.AppendBlockStart(isAsync
                               ? $"{varCommand}.SetAction(async ({varParseResult}, {varCancellationToken}) =>"
                               : $"{varCommand}.SetAction({varParseResult} =>",
                    ");"))
                    {
                        var varTargetClass = "targetClass";

                        sb.AppendLine($"var {varTargetClass} = ({definitionClass}) Bind({varParseResult});");
                        sb.AppendLine();

                        sb.AppendLine("//  Call the command handler");
                        sb.AppendLine(isAsync
                            ? $"var {varCliContext} = new DotMake.CommandLine.CliContext({varParseResult}, {varCancellationToken});"
                            : $"var {varCliContext} = new DotMake.CommandLine.CliContext({varParseResult});");
                        sb.AppendLine("var exitCode = 0;");
                        if (handlerWithoutProblem != null)
                        {
                            sb.AppendLineStart();
                            if (handlerWithoutProblem.ReturnsValue)
                                sb.Append("exitCode = ");
                            if (handlerWithoutProblem.IsAsync)
                                sb.Append("await ");
                            sb.Append($"{varTargetClass}.");
                            handlerWithoutProblem.AppendCSharpCallString(sb, varCliContext);
                            sb.Append(";");
                            sb.AppendLineEnd();
                        }
                        else
                        {
                            sb.AppendLine($"{varCliContext}.ShowHelp();");
                        }
                        sb.AppendLine("return exitCode;");
                    }

                    sb.AppendLine();
                    sb.AppendLine($"return {varCommand};");
                }

                sb.AppendLine();
                sb.AppendLine("[System.Runtime.CompilerServices.ModuleInitializerAttribute]");
                using (sb.AppendBlockStart("internal static void Initialize()"))
                {
                    var varCommandBuilder = "commandBuilder";
                    sb.AppendLine($"var {varCommandBuilder} = new {GeneratedClassFullName}();");

                    sb.AppendLine();
                    sb.AppendLine("// Register this command builder so that it can be found by the definition class");
                    sb.AppendLine("// and it can be found by the parent definition class if it's a nested/external child.");
                    sb.AppendLine($"{varCommandBuilder}.Register();");
                }

                foreach (var nestedCliCommandInfo in subcommandsWithoutProblem)
                {
                    sb.AppendLine();
                    nestedCliCommandInfo.AppendCSharpDefineString(sb, false);
                }
            }
        }

        public void AppendCSharpCreateString(CodeStringBuilder sb, string varName)
        {
            var attributeResourceArguments = AttributeArguments.GetResourceArguments(SemanticModel);

            var commandClass = $"{CommandClassNamespace}.{(IsRoot ? RootCommandClassName : CommandClassName)}";

            var commandName = AttributeArguments.TryGetValue(nameof(CliCommandAttribute.Name), out var nameValue)
                                       && !string.IsNullOrWhiteSpace(nameValue.ToString())
                ? nameValue.ToString().Trim()
                : null;

            IDisposable block;

            sb.AppendLine($"// Command for '{Symbol.Name}' class");

            if (IsRoot)
            {
                block = sb.AppendBlockStart($"var {varName} = new {commandClass}()", ";");
                if (commandName != null)
                    sb.AppendLine($"Name = \"{commandName}\",");
            }
            else
            {
                if (commandName == null)
                    commandName = Symbol.Name.StripSuffixes(Suffixes).ToCase(Settings.NameCasingConvention);

                block = sb.AppendBlockStart($"var {varName} = new {commandClass}(\"{commandName}\")", ";");
            }

            foreach (var kvp in AttributeArguments)
            {
                switch (kvp.Key)
                {
                    case nameof(CliCommandAttribute.Description):
                    case nameof(CliCommandAttribute.Hidden):
                    case nameof(CliCommandAttribute.TreatUnmatchedTokensAsErrors):
                        if (!PropertyMappings.TryGetValue(kvp.Key, out var propertyName))
                            propertyName = kvp.Key;

                        if (attributeResourceArguments.TryGetValue(kvp.Key, out var resourceProperty))
                            sb.AppendLine($"{propertyName} = {resourceProperty.ToReferenceString()},");
                        else
                            sb.AppendLine($"{propertyName} = {kvp.Value.ToCSharpString()},");
                        break;
                }
            }
            block.Dispose();

            UsedAliases.Clear(); //Reset
            if (AttributeArguments.TryGetValues(nameof(CliCommandAttribute.Aliases), out var aliasesValues))
            {
                foreach (var aliasValue in aliasesValues)
                {
                    var alias = aliasValue?.ToString();
                    if (!UsedAliases.Contains(alias))
                    {
                        sb.AppendLine($"{varName}.Aliases.Add(\"{alias}\");");
                        UsedAliases.Add(alias);
                    }
                }
            }
        }

        public bool Equals(CliCommandInfo other)
        {
            return base.Equals(other);
        }
    }
}
