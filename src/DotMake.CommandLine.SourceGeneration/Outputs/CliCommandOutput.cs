using System.Collections.Generic;
using System.Linq;
using DotMake.CommandLine.SourceGeneration.Inputs;
using DotMake.CommandLine.SourceGeneration.Util;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace DotMake.CommandLine.SourceGeneration.Outputs
{
    public class CliCommandOutput : OutputBase
    {
        public const string RootCommandClassName = "RootCommand";
        public const string CommandClassName = "Command";
        public const string CommandClassNamespace = "System.CommandLine";
        public const string CompletionsNamespace = "System.CommandLine.Completions";
        public const string GeneratedSubNamespace = "GeneratedCode";
        public const string GeneratedClassSuffix = "Builder";
        public static readonly string CommandBuilderFullName = "DotMake.CommandLine.CliCommandBuilder";
        public static readonly string[] Suffixes = { "RootCliCommand", "RootCommand", "SubCliCommand", "SubCommand", "CliCommand", "Command", "Cli" };
        public static readonly Dictionary<string, string> PropertyMappings = new()
        {
            //{ nameof(CliCommandAttribute.Hidden), "IsHidden"}
        };

        public CliCommandOutput(CliCommandInput input, CliReferenceDependantInput referenceDependantInput)
            : base(input)
        {
            Input = input;
            ReferenceDependantInput = referenceDependantInput;

            if (!referenceDependantInput.HasMsDependencyInjectionAbstractions
                && !Input.Symbol.InstanceConstructors.Any(c =>
                    c.Parameters.IsEmpty
                    && (c.DeclaredAccessibility == Accessibility.Public || c.DeclaredAccessibility == Accessibility.Internal)
                ))
                Input.AddDiagnostic(DiagnosticDescriptors.ErrorClassHasNotPublicDefaultConstructor, CliCommandInput.DiagnosticName);

            GeneratedClassName = Input.Symbol.Name + GeneratedClassSuffix;
            GeneratedClassNamespace = Input.Symbol.GetNamespaceOrEmpty();
            if (!GeneratedClassNamespace.EndsWith(GeneratedSubNamespace))
                GeneratedClassNamespace = SymbolExtensions.CombineNameParts(GeneratedClassNamespace, GeneratedSubNamespace);
            GeneratedClassFullName = (Input.Symbol.ContainingType != null)
                ? SymbolExtensions.CombineNameParts(
                    Input.Symbol.RenameContainingTypesFullName(GeneratedSubNamespace, GeneratedClassSuffix),
                    GeneratedClassName)
                : SymbolExtensions.CombineNameParts(GeneratedClassNamespace, GeneratedClassName);
        }

        public new CliCommandInput Input { get; }

        public CliReferenceDependantInput ReferenceDependantInput { get; }

        public string GeneratedClassName { get; }

        public string GeneratedClassNamespace { get; }

        public string GeneratedClassFullName { get; }

        public void AppendCSharpDefineString(CodeStringBuilder sb, bool addNamespaceBlock)
        {
            var optionsWithoutProblem = Input.Options.Where(c => !c.HasProblem).ToArray();
            var argumentsWithoutProblem = Input.Arguments.Where(c => !c.HasProblem).ToArray();
            var subcommandsWithoutProblem = Input.Subcommands.Where(c => !c.HasProblem).ToArray();
            var parentCommandAccessorsWithoutProblem = Input.ParentCommandAccessors.Where(c => !c.HasProblem).ToArray();
            var handlerWithoutProblem = (Input.Handler != null && !Input.Handler.HasProblem) ? Input.Handler : null;
            var memberHasRequiredModifier = optionsWithoutProblem.Any(o => o.Symbol.IsRequired)
                                            || argumentsWithoutProblem.Any(a => a.Symbol.IsRequired)
                                            || parentCommandAccessorsWithoutProblem.Any(r => r.Symbol.IsRequired);

            if (string.IsNullOrEmpty(GeneratedClassNamespace))
                addNamespaceBlock = false;

            using var namespaceBlock = addNamespaceBlock ? sb.AppendBlockStart($"namespace {GeneratedClassNamespace}") : null;
            sb.AppendLine("/// <inheritdoc />");
            using (sb.AppendBlockStart($"public class {GeneratedClassName} : {CommandBuilderFullName}"))
            {
                var varCommand = "command";
                var definitionClass = Input.Symbol.ToReferenceString();

                sb.AppendLine("/// <inheritdoc />");
                using (sb.AppendBlockStart($"public {GeneratedClassName}()"))
                {
                    sb.AppendLine($"DefinitionType = typeof({definitionClass});");

                    var parentDefinitionType = (Input.ParentSymbol != null) ? $"typeof({Input.ParentSymbol.ToReferenceString()})" : "null";
                    sb.AppendLine($"ParentDefinitionType = {parentDefinitionType};");

                    if (Input.ChildrenArgument == null)
                        sb.AppendLine("ChildDefinitionTypes = null;");
                    else
                        using (sb.AppendBlockStart("ChildDefinitionTypes = new [] ", ";"))
                        {
                            for (var i = 0; i < Input.ChildrenArgument.Length; i++)
                            {
                                var childType = Input.ChildrenArgument[i];
                                if (childType == null)
                                    continue;
                                sb.AppendLineStart();
                                sb.Append($"typeof({childType.ToReferenceString()})");
                                if (i < Input.ChildrenArgument.Length - 1)
                                    sb.Append(",");
                                sb.AppendLineEnd();
                            }
                        }

                    var nameCasingConvention = (Input.NameCasingConvention.HasValue)
                        ? EnumUtil<CliNameCasingConvention>.ToFullName(Input.NameCasingConvention.Value)
                        : "null";
                    sb.AppendLine($"NameCasingConvention = {nameCasingConvention};");

                    var namePrefixConvention = (Input.NamePrefixConvention.HasValue)
                        ? EnumUtil<CliNamePrefixConvention>.ToFullName(Input.NamePrefixConvention.Value)
                        : "null";
                    sb.AppendLine($"NamePrefixConvention = {namePrefixConvention};");

                    var shortFormPrefixConvention = (Input.ShortFormPrefixConvention.HasValue)
                        ? EnumUtil<CliNamePrefixConvention>.ToFullName(Input.ShortFormPrefixConvention.Value)
                        : "null";
                    sb.AppendLine($"ShortFormPrefixConvention = {shortFormPrefixConvention};");

                    var shortFormAutoGenerate = (Input.ShortFormAutoGenerate.HasValue)
                        ? Input.ShortFormAutoGenerate.Value.ToString().ToLowerInvariant()
                        : "null";
                    sb.AppendLine($"ShortFormAutoGenerate = {shortFormAutoGenerate};");

                }
                sb.AppendLine();

                /*
                GetUninitializedObject causes IL2072 trimming warnings, so no longer use this,
                Instead we will use Bind method to get cached definition instance and call GetCompletions method
                using (sb.AppendBlockStart($"private {definitionClass} CreateUninitializedInstance()"))
                {
                    sb.AppendLine($"return ({definitionClass})");
                    sb.AppendLine("#if NET5_0_OR_GREATER");
                    sb.AppendIndent();
                    sb.AppendLine("System.Runtime.CompilerServices.RuntimeHelpers.GetUninitializedObject(DefinitionType);");
                    sb.AppendLine("#else");
                    sb.AppendIndent();
                    sb.AppendLine("System.Runtime.Serialization.FormatterServices.GetUninitializedObject(DefinitionType);");
                    sb.AppendLine("#endif");
                }
                sb.AppendLine();
                */

                using (sb.AppendBlockStart($"private {definitionClass} CreateInstance()"))
                {
                    if (ReferenceDependantInput.HasMsDependencyInjectionAbstractions || ReferenceDependantInput.HasMsDependencyInjection)
                    {
                        sb.AppendLine(ReferenceDependantInput.HasMsDependencyInjection
                            ? "ServiceProvider = DotMake.CommandLine.CliServiceCollectionExtensions.GetServiceProviderOrDefault(null);"
                            : "ServiceProvider = DotMake.CommandLine.CliServiceProviderExtensions.GetServiceProvider(null);");
                        sb.AppendLine("if (ServiceProvider != null)");
                        sb.AppendIndent();
                        sb.AppendLine("return Microsoft.Extensions.DependencyInjection.ActivatorUtilities");
                        sb.AppendIndent();
                        sb.AppendIndent();
                        sb.AppendLine($".CreateInstance<{definitionClass}>(ServiceProvider);");
                        //in case ServiceProvider is null (i.e. not set with SetServiceProvider)
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

                if (Input.HasAddCompletionsInterface)
                {
                    sb.AppendLine();
                    sb.AppendLine($"private System.Collections.Generic.IEnumerable<{CompletionsNamespace}.CompletionItem> GetCompletions(");
                    sb.AppendIndent();
                    sb.AppendLine($"string propertyName, {CompletionsNamespace}.CompletionContext completionContext)");
                    using (sb.AppendBlockStart())
                    {
                        var varTargetClass = "targetClass";

                        sb.AppendLine($"var {varTargetClass} = ({definitionClass}) Bind(completionContext.ParseResult);");
                        sb.AppendLine();

                        sb.AppendLine("//  Call the interface method with property name of option or argument");
                        sb.AppendLine($"return {varTargetClass}.GetCompletions(propertyName, completionContext);");
                    }
                    sb.AppendLine();
                }

                sb.AppendLine("/// <inheritdoc />");
                using (sb.AppendBlockStart($"public override {CommandClassNamespace}.{CommandClassName} Build()"))
                {
                    AppendCSharpCreateString(sb, varCommand);

                    var varDefaultClass = "defaultClass";
                    /*
                    sb.AppendLine();
                    //No more using a default instance here to avoid IServiceProvider integration causing unnecessary instantiations.
                    //Instead, we read the property initializer SyntaxNode, qualify symbols and then use that SyntaxNode for DefaultValueFactory.
                    //However, we still need an uninitialized instance for being able to call AddCompletions method, for now.
                    sb.AppendLine($"var {varDefaultClass} = CreateUninitializedInstance();");
                    */

                    for (var index = 0; index < optionsWithoutProblem.Length; index++)
                    {
                        sb.AppendLine();

                        var cliOptionInput = optionsWithoutProblem[index];
                        var cliOptionOutput = new CliOptionOutput(cliOptionInput);
                        var varOption = $"option{index}";
                        cliOptionOutput.AppendCSharpCreateString(sb, varOption, varDefaultClass);
                        sb.AppendLine($"{varCommand}.Add({varOption});");
                    }

                    for (var index = 0; index < argumentsWithoutProblem.Length; index++)
                    {
                        sb.AppendLine();

                        var cliArgumentInput = argumentsWithoutProblem[index];
                        var cliArgumentOutput = new CliArgumentOutput(cliArgumentInput);
                        var varArgument = $"argument{index}";
                        cliArgumentOutput.AppendCSharpCreateString(sb, varArgument, varDefaultClass);
                        sb.AppendLine($"{varCommand}.Add({varArgument});");
                    }

                    /*
                    From now on, we will handle this in Cli.GetConfiguration where Build() is called and command is created.
                    We don't want it to be recursive here, because we will also create the parents.

                    sb.AppendLine();
                    sb.AppendLine("// Add nested or external registered children");
                    using (sb.AppendBlockStart("foreach (var child in Children)"))
                    {
                        sb.AppendLine($"{varCommand}.Add(child.Build());");
                    }
                    */

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
                            var handlerOutput = new CliCommandHandlerOutput(handlerWithoutProblem);
                            handlerOutput.AppendCSharpCallString(sb, varCliContext);
                            sb.Append(";");
                            sb.AppendLineEnd();
                        }
                        else
                        {
                            sb.AppendLine($"{varCliContext}.ShowHelp();");
                        }

                        sb.AppendLine();
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

                foreach (var nestedCliCommandInput in subcommandsWithoutProblem)
                {
                    sb.AppendLine();

                    var nestedCliCommandOutput = new CliCommandOutput(nestedCliCommandInput, ReferenceDependantInput);
                    nestedCliCommandOutput.AppendCSharpDefineString(sb, false);
                }
            }
        }

        public void AppendCSharpCreateString(CodeStringBuilder sb, string varName)
        {
            var commandName = Input.AttributeArguments.TryGetValue(nameof(CliCommandAttribute.Name), out var nameValue)
                              && !string.IsNullOrWhiteSpace(nameValue.ToString())
                ? $"\"{nameValue.ToString().Trim()}\""
                : $"GetCommandName(\"{Input.Symbol.Name.StripSuffixes(Suffixes)}\")";

            sb.AppendLine($"// Command for '{Input.Symbol.Name}' class");
            //sb.AppendLine($"// Parent tree: '{string.Join(" -> ", Input.ParentTree.Select(p=> p.Symbol))}'");

            using (sb.AppendBlockStart($"var {varName} = IsRoot", null, null, null))
            {
                //Cannot set name for a RootCommand, it's the executable name by default
                sb.AppendLine($"? new {CommandClassNamespace}.{RootCommandClassName}()");
                sb.AppendLine($": new {CommandClassNamespace}.{CommandClassName}({commandName});");                
            }
            
            foreach (var kvp in Input.AttributeArguments)
            {
                switch (kvp.Key)
                {
                    case nameof(CliCommandAttribute.Description):
                    case nameof(CliCommandAttribute.Hidden):
                    case nameof(CliCommandAttribute.TreatUnmatchedTokensAsErrors):
                        if (!PropertyMappings.TryGetValue(kvp.Key, out var propertyName))
                            propertyName = kvp.Key;

                        if (Input.AttributeArguments.TryGetResourceProperty(kvp.Key, out var resourceProperty))
                            sb.AppendLine($"{varName}.{propertyName} = {resourceProperty.ToReferenceString()};");
                        else
                            sb.AppendLine($"{varName}.{propertyName} = {kvp.Value.ToCSharpString()};");
                        break;
                }
            }

            if (Input.AttributeArguments.TryGetValues(nameof(CliCommandAttribute.Aliases), out var aliasesValues))
            {
                foreach (var aliasValue in aliasesValues)
                {
                    var alias = aliasValue?.ToString().Trim();

                    sb.AppendLine($"AddAlias({varName}, \"{alias}\");");
                }
            }
        }
    }
}
