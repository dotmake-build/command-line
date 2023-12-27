using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace DotMake.CommandLine.SourceGeneration
{
    public class CliCommandInfo : CliSymbolInfo, IEquatable<CliCommandInfo>
    {
        public static readonly string AttributeFullName = typeof(CliCommandAttribute).FullName;
        public const string AttributeNameProperty = nameof(CliCommandAttribute.Name);
        public const string AttributeAliasesProperty = nameof(CliCommandAttribute.Aliases);
        public static readonly string[] Suffixes = { "RootCliCommand", "RootCommand", "SubCliCommand", "SubCommand", "CliCommand", "Command", "Cli" };
        public const string RootCommandClassName = "RootCommand";
        public const string CommandClassName = "Command";
        public const string CommandClassNamespace = "System.CommandLine";
        public const string DiagnosticName = "CLI command";
        public const string GeneratedClassSuffix = "Builder";
        public static readonly string CommandBuilderFullName = "DotMake.CommandLine.CliCommandBuilder";
        public static readonly Dictionary<string, string> PropertyMappings = new Dictionary<string, string>
        {
            { nameof(CliCommandAttribute.Hidden), "IsHidden"}
        };
        public readonly HashSet<string> UsedAliases = new HashSet<string>(StringComparer.Ordinal);

        public CliCommandInfo(ISymbol symbol, SyntaxNode syntaxNode, AttributeData attributeData, SemanticModel semanticModel, CliCommandInfo parent)
            : base(symbol, syntaxNode, semanticModel)
        {
            Symbol = (INamedTypeSymbol)symbol;
            Parent = parent;

            AttributeArguments = new Dictionary<string, TypedConstant>();
            Settings = CliCommandSettings.Parse(Symbol, attributeData, AttributeArguments);
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
            GeneratedClassNamespace = Settings.IsParentContaining
                ? Settings.GetContainingTypeFullName(GeneratedClassSuffix)
                : (symbol.ContainingNamespace == null || symbol.ContainingNamespace.IsGlobalNamespace)
                    ? string.Empty
                    : symbol.ContainingNamespace.ToReferenceString();
            GeneratedClassFullName = string.IsNullOrEmpty(GeneratedClassNamespace)
                ? GeneratedClassName
                : GeneratedClassNamespace + "." + GeneratedClassName;

            Analyze();

            if (HasProblem)
                return;

            foreach (var member in Symbol.GetMembers())
            {
                if (member is IPropertySymbol)
                {
                    foreach (var memberAttributeData in member.GetAttributes())
                    {
                        var attributeFullName = memberAttributeData.AttributeClass?.ToCompareString();

                        if (attributeFullName == CliOptionInfo.AttributeFullName)
                            childOptions.Add(new CliOptionInfo(member, null, memberAttributeData, SemanticModel, this));

                        if (attributeFullName == CliArgumentInfo.AttributeFullName)
                            childArguments.Add(new CliArgumentInfo(member, null, memberAttributeData, SemanticModel, this));
                    }
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

            if (Handler == null)
                AddDiagnostic(DiagnosticDescriptors.WarningClassHasNotHandler, false, CliCommandHandlerInfo.DiagnosticName);

            foreach (var nestedType in Symbol.GetTypeMembers())
            {
                foreach (var memberAttributeData in nestedType.GetAttributes())
                {
                    if (memberAttributeData.AttributeClass?.ToCompareString() == AttributeFullName)
                        childCommands.Add(new CliCommandInfo(nestedType, null, memberAttributeData, SemanticModel, this));
                }
            }
        }

        public CliCommandInfo(GeneratorAttributeSyntaxContext attributeSyntaxContext)
            : this(attributeSyntaxContext.TargetSymbol,
                attributeSyntaxContext.TargetNode,
                attributeSyntaxContext.Attributes[0],
                attributeSyntaxContext.SemanticModel,
                null)
        {
        }

        public new INamedTypeSymbol Symbol { get; }

        public Dictionary<string, TypedConstant> AttributeArguments { get; }

        public CliCommandSettings Settings { get; }

        public CliCommandInfo Parent { get; }

        public bool IsRoot { get; }

        public bool IsExternalChild { get; }

        public CliCommandHandlerInfo Handler { get; }

        public string GeneratedClassName { get; }

        public string GeneratedClassNamespace { get; }

        public string GeneratedClassFullName { get; }

        public IReadOnlyList<CliOptionInfo> ChildOptions => childOptions;
        private readonly List<CliOptionInfo> childOptions = new List<CliOptionInfo>();

        public IReadOnlyList<CliArgumentInfo> ChildArguments => childArguments;
        private readonly List<CliArgumentInfo> childArguments = new List<CliArgumentInfo>();

        public IReadOnlyList<CliCommandInfo> ChildCommands => childCommands;
        private readonly List<CliCommandInfo> childCommands = new List<CliCommandInfo>();

        private void Analyze()
        {
            if ((Symbol.DeclaredAccessibility != Accessibility.Public && Symbol.DeclaredAccessibility != Accessibility.Internal)
                || Symbol.IsStatic)
                AddDiagnostic(DiagnosticDescriptors.WarningClassNotPublicNonStatic, DiagnosticName);
            else
            {
                if (Symbol.IsAbstract || Symbol.IsGenericType)
                    AddDiagnostic(DiagnosticDescriptors.ErrorClassNotNonAbstractNonGeneric, DiagnosticName);

                if (!Symbol.InstanceConstructors.Any(c =>
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

            foreach (var child in ChildOptions)
                child.ReportDiagnostics(sourceProductionContext);

            foreach (var child in ChildArguments)
                child.ReportDiagnostics(sourceProductionContext);

            foreach (var child in ChildCommands)
                child.ReportDiagnostics(sourceProductionContext);
        }

        public void AppendCSharpDefineString(CodeStringBuilder sb, bool addNamespaceBlock)
        {
            var childOptionsWithoutProblem = ChildOptions.Where(c => !c.HasProblem).ToArray();
            var childArgumentsWithoutProblem = ChildArguments.Where(c => !c.HasProblem).ToArray();
            var childCommandsWithoutProblem = ChildCommands.Where(c => !c.HasProblem).ToArray();
            var handlerWithoutProblem = (Handler != null && !Handler.HasProblem) ? Handler : null;

            if (string.IsNullOrEmpty(GeneratedClassNamespace))
                addNamespaceBlock = false;

            using (addNamespaceBlock ? sb.AppendBlockStart($"namespace {GeneratedClassNamespace}") : null)
            using (sb.AppendBlockStart($"public class {GeneratedClassName} : {CommandBuilderFullName}"))
            {
                var varCommand = (IsRoot ? RootCommandClassName : CommandClassName).ToCase(CliNameCasingConvention.CamelCase);
                var commandClass = $"{CommandClassNamespace}.{(IsRoot ? RootCommandClassName : CommandClassName)}";
                var definitionClass = Symbol.ToReferenceString();
                var parentDefinitionClass = IsRoot ? null : Settings.ParentSymbol.ToReferenceString();
                var parentDefinitionType = (parentDefinitionClass != null) ? $"typeof({parentDefinitionClass})" : "null";

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

                using (sb.AppendBlockStart($"public override {commandClass} Build()"))
                {
                    AppendCSharpCreateString(sb, varCommand);

                    sb.AppendLine();
                    var varDefaultClass = "defaultClass";
                    sb.AppendLine($"var {varDefaultClass} = new {definitionClass}();");

                    for (var index = 0; index < childOptionsWithoutProblem.Length; index++)
                    {
                        sb.AppendLine();

                        var cliOptionInfo = childOptionsWithoutProblem[index];
                        var varOption = $"option{index}";
                        cliOptionInfo.AppendCSharpCreateString(sb, varOption,
                            $"{varDefaultClass}.{cliOptionInfo.Symbol.Name}");
                        sb.AppendLine(cliOptionInfo.Global
                            ? $"{varCommand}.AddGlobalOption({varOption});"
                            : $"{varCommand}.Add({varOption});");
                    }

                    for (var index = 0; index < childArgumentsWithoutProblem.Length; index++)
                    {
                        sb.AppendLine();

                        var cliArgumentInfo = childArgumentsWithoutProblem[index];
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
                    using (sb.AppendBlockStart($"BindFunc = (parseResult) =>", ";"))
                    {
                        var varTargetClass = "targetClass";

                        sb.AppendLine($"var {varTargetClass} = new {definitionClass}();");

                        sb.AppendLine();
                        sb.AppendLine("//  Set the parsed or default values for the options");
                        for (var index = 0; index < childOptionsWithoutProblem.Length; index++)
                        {
                            var cliOptionInfo = childOptionsWithoutProblem[index];
                            var varOption = $"option{index}";
                            sb.AppendLine($"{varTargetClass}.{cliOptionInfo.Symbol.Name} = GetValueForOption(parseResult, {varOption});");
                        }

                        sb.AppendLine();
                        sb.AppendLine("//  Set the parsed or default values for the arguments");
                        for (var index = 0; index < childArgumentsWithoutProblem.Length; index++)
                        {
                            var cliArgumentInfo = childArgumentsWithoutProblem[index];
                            var varArgument = $"argument{index}";
                            sb.AppendLine($"{varTargetClass}.{cliArgumentInfo.Symbol.Name} = GetValueForArgument(parseResult, {varArgument});");
                        }

                        sb.AppendLine();
                        sb.AppendLine($"return {varTargetClass};");
                    }

                    sb.AppendLine();
                    var varInvocationContext = "context";
                    var asyncKeyword = (handlerWithoutProblem != null && handlerWithoutProblem.IsAsync) ? "async " : "";
                    using (sb.AppendBlockStart($"{CommandClassNamespace}.Handler.SetHandler({varCommand}, {asyncKeyword}{varInvocationContext} =>", ");"))
                    {
                        var varTargetClass = "targetClass";

                        sb.AppendLine($"var {varTargetClass} = ({definitionClass}) BindFunc({varInvocationContext}.ParseResult);");
                        sb.AppendLine();

                        sb.AppendLine("//  Call the command handler");
                        if (handlerWithoutProblem != null)
                        {
                            sb.AppendLineStart();
                            if (handlerWithoutProblem.ReturnsValue)
                                sb.Append($"{varInvocationContext}.ExitCode = ");
                            if (handlerWithoutProblem.IsAsync)
                                sb.Append("await ");
                            sb.Append($"{varTargetClass}.");
                            handlerWithoutProblem.AppendCSharpCallString(sb, varInvocationContext);
                            sb.Append(";");
                            sb.AppendLineEnd();
                        }
                    }

                    sb.AppendLine();
                    sb.AppendLine($"return {varCommand};");
                }

                sb.AppendLine();
                sb.AppendLine("[System.Runtime.CompilerServices.ModuleInitializerAttribute]");
                using (sb.AppendBlockStart("public static void Initialize()"))
                {
                    var varCommandBuilder = "commandBuilder";
                    sb.AppendLine($"var {varCommandBuilder} = new {GeneratedClassFullName}();");

                    sb.AppendLine();
                    sb.AppendLine("// Register this command builder so that it can be found by the definition class");
                    sb.AppendLine("// and it can be found by the parent definition class if it's a nested/external child.");
                    sb.AppendLine($"{varCommandBuilder}.Register();");
                }

                foreach (var nestedCliCommandInfo in childCommandsWithoutProblem)
                {
                    sb.AppendLine();
                    nestedCliCommandInfo.AppendCSharpDefineString(sb, false);
                }
            }
        }

        public void AppendCSharpCreateString(CodeStringBuilder sb, string varName)
        {
            var commandClass = $"{CommandClassNamespace}.{(IsRoot ? RootCommandClassName : CommandClassName)}";

            var commandName = AttributeArguments.TryGetValue(AttributeNameProperty, out var nameTypedConstant)
                                       && !string.IsNullOrWhiteSpace(nameTypedConstant.Value?.ToString())
                ? nameTypedConstant.Value.ToString().Trim()
                : null;

            IDisposable block;

            sb.AppendLine($"// Command for '{Symbol.Name}' class");

            if (IsRoot)
            {
                block = sb.AppendBlockStart($"var {varName} = new {commandClass}()", ";");
                if (commandName != null)
                    sb.AppendLine($"{AttributeNameProperty} = \"{commandName}\",");
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
                    case AttributeNameProperty:
                    case AttributeAliasesProperty:
                        continue;
                    default:
                        if (!PropertyMappings.TryGetValue(kvp.Key, out var propertyName))
                            propertyName = kvp.Key;

                        sb.AppendLine($"{propertyName} = {kvp.Value.ToCSharpString()},");
                        break;
                }
            }
            block.Dispose();

            UsedAliases.Clear(); //Reset
            if (AttributeArguments.TryGetValue(AttributeAliasesProperty, out var aliasesTypedConstant)
                && !aliasesTypedConstant.IsNull)
            {
                foreach (var aliasTypedConstant in aliasesTypedConstant.Values)
                {
                    var alias = aliasTypedConstant.Value?.ToString();
                    if (!UsedAliases.Contains(alias))
                    {
                        sb.AppendLine($"{varName}.AddAlias(\"{alias}\");");
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
