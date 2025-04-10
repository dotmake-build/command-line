using System;
using System.Collections.Generic;
using System.Linq;
using DotMake.CommandLine.SourceGeneration.Inputs;
using DotMake.CommandLine.SourceGeneration.Util;
using Microsoft.CodeAnalysis.CSharp;

namespace DotMake.CommandLine.SourceGeneration.Outputs
{
    public class CliCommandOutput : OutputBase
    {
        public const string RootCommandClassName = "RootCommand";
        public const string CommandClassName = "Command";
        public const string CommandClassNamespace = "System.CommandLine";
        public static readonly string CommandBuilderFullName = "DotMake.CommandLine.CliCommandBuilder";
        public static readonly string[] Suffixes = { "RootCliCommand", "RootCommand", "SubCliCommand", "SubCommand", "CliCommand", "Command", "Cli" };
        public static readonly Dictionary<string, string> PropertyMappings = new()
        {
            //{ nameof(CliCommandAttribute.Hidden), "IsHidden"}
        };

        public CliCommandOutput(CliCommandInput input)
            : base(input)
        {
            Input = input;
        }

        public new CliCommandInput Input { get; set; }

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

            if (string.IsNullOrEmpty(Input.GeneratedClassNamespace))
                addNamespaceBlock = false;

            using var namespaceBlock = addNamespaceBlock ? sb.AppendBlockStart($"namespace {Input.GeneratedClassNamespace}") : null;
            sb.AppendLine("/// <inheritdoc />");
            using (sb.AppendBlockStart($"public class {Input.GeneratedClassName} : {CommandBuilderFullName}"))
            {
                var varCommand = (Input.IsRoot ? "rootCommand" : "command");
                var definitionClass = Input.Symbol.ToReferenceString();
                var parentDefinitionClass = Input.IsRoot ? null : Input.NestedOrExternalParentSymbol.ToReferenceString();
                var parentDefinitionType = (parentDefinitionClass != null) ? $"typeof({parentDefinitionClass})" : "null";

                sb.AppendLine("/// <inheritdoc />");
                using (sb.AppendBlockStart($"public {Input.GeneratedClassName}()"))
                {
                    sb.AppendLine($"DefinitionType = typeof({definitionClass});");
                    sb.AppendLine($"ParentDefinitionType = {parentDefinitionType};");

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

                using (sb.AppendBlockStart($"private {definitionClass} CreateInstance()"))
                {
                    if (Input.CliReferenceDependantInput.HasMsDependencyInjectionAbstractions || Input.CliReferenceDependantInput.HasMsDependencyInjection)
                    {
                        sb.AppendLine(Input.CliReferenceDependantInput.HasMsDependencyInjection
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

                        var cliOptionInput = optionsWithoutProblem[index];
                        var cliOptionOutput = new CliOptionOutput(cliOptionInput);
                        var varOption = $"option{index}";
                        cliOptionOutput.AppendCSharpCreateString(sb, varOption,
                            $"{varDefaultClass}.{cliOptionInput.Symbol.Name}");
                        sb.AppendLine($"{varCommand}.Add({varOption});");
                    }

                    for (var index = 0; index < argumentsWithoutProblem.Length; index++)
                    {
                        sb.AppendLine();

                        var cliArgumentInput = argumentsWithoutProblem[index];
                        var cliArgumentOutput = new CliArgumentOutput(cliArgumentInput);
                        var varArgument = $"argument{index}";
                        cliArgumentOutput.AppendCSharpCreateString(sb, varArgument,
                            $"{varDefaultClass}.{cliArgumentInput.Symbol.Name}");
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
                    sb.AppendLine($"var {varCommandBuilder} = new {Input.GeneratedClassFullName}();");

                    sb.AppendLine();
                    sb.AppendLine("// Register this command builder so that it can be found by the definition class");
                    sb.AppendLine("// and it can be found by the parent definition class if it's a nested/external child.");
                    sb.AppendLine($"{varCommandBuilder}.Register();");
                }

                foreach (var nestedCliCommandInput in subcommandsWithoutProblem)
                {
                    sb.AppendLine();

                    var nestedCliCommandOutput = new CliCommandOutput(nestedCliCommandInput);
                    nestedCliCommandOutput.AppendCSharpDefineString(sb, false);
                }
            }
        }

        public void AppendCSharpCreateString(CodeStringBuilder sb, string varName)
        {
            var commandClass = $"{CommandClassNamespace}.{(Input.IsRoot ? RootCommandClassName : CommandClassName)}";

            var commandName = Input.AttributeArguments.TryGetValue(nameof(CliCommandAttribute.Name), out var nameValue)
                              && !string.IsNullOrWhiteSpace(nameValue.ToString())
                ? $"\"{nameValue.ToString().Trim()}\""
                : null;

            IDisposable block;

            sb.AppendLine($"// Command for '{Input.Symbol.Name}' class");
            //sb.AppendLine($"// Parent tree: '{string.Join(" -> ", Input.ParentTree.Select(p=> p.Symbol))}'");

            if (Input.IsRoot)
            {
                block = sb.AppendBlockStart($"var {varName} = new {commandClass}()", ";");
                if (commandName != null)
                    sb.AppendLine($"Name = {commandName},");
            }
            else
            {
                if (commandName == null)
                    commandName = $"GetCommandName(\"{Input.Symbol.Name.StripSuffixes(Suffixes)}\")";

                block = sb.AppendBlockStart($"var {varName} = new {commandClass}({commandName})", ";");
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
                            sb.AppendLine($"{propertyName} = {resourceProperty.ToReferenceString()},");
                        else
                            sb.AppendLine($"{propertyName} = {kvp.Value.ToCSharpString()},");
                        break;
                }
            }
            block.Dispose();

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
