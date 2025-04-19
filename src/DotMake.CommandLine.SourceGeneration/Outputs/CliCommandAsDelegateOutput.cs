using DotMake.CommandLine.SourceGeneration.Inputs;
using DotMake.CommandLine.SourceGeneration.Util;

namespace DotMake.CommandLine.SourceGeneration.Outputs
{
    public class CliCommandAsDelegateOutput : OutputBase
    {
        public static readonly string CliCommandAsDelegateFullName = "DotMake.CommandLine.CliCommandAsDelegate";

        public CliCommandAsDelegateOutput(CliCommandAsDelegateInput input)
            : base(input)
        {
            Input = input;

            GeneratedClassName = "CliCommandAsDelegate_" + Input.Hash;
            GeneratedClassNamespace = CliCommandOutput.GeneratedSubNamespace;
            GeneratedClassFullName = SymbolExtensions.CombineNameParts(GeneratedClassNamespace, GeneratedClassName);
        }

        public new CliCommandAsDelegateInput Input { get; set; }

        public string GeneratedClassName { get; }

        public string GeneratedClassNamespace { get; }

        public string GeneratedClassFullName { get; }

        public void AppendCSharpDefineString(CodeStringBuilder sb)
        {
            using var namespaceBlock = sb.AppendBlockStart($"namespace {GeneratedClassNamespace}");

            sb.AppendLine("/// <inheritdoc />");

            if (Input.AttributeSyntax != null)
                sb.AppendLine($"[{Input.AttributeData.AttributeClass.ToReferenceString()}{Input.AttributeSyntax.ArgumentList}]");
            else
                sb.AppendLine($"[{CliCommandInput.AttributeFullName}]");

            using (sb.AppendBlockStart($"public class {GeneratedClassName} : {CliCommandAsDelegateFullName}"))
            {
                for (var index = 0; index < Input.ParameterInfos.Count; index++)
                {
                    var parameterInfo = Input.ParameterInfos[index];

                    sb.AppendLine("/// <inheritdoc />");

                    if (parameterInfo.AttributeSyntax != null)
                        sb.AppendLine(
                            $"[{parameterInfo.AttributeData.AttributeClass.ToReferenceString()}{parameterInfo.AttributeSyntax.ArgumentList}]");
                    else
                        sb.AppendLine($"[{CliOptionInput.AttributeFullName}]");

                    if (parameterInfo.DefaultSyntax != null)
                        sb.AppendLine(
                            $"public {parameterInfo.Symbol.Type.ToReferenceString()} {parameterInfo.Symbol.Name} {{ get; set; }} = {parameterInfo.DefaultSyntax};");
                    else
                        sb.AppendLine(
                            $"public {parameterInfo.Symbol.Type.ToReferenceString()} {parameterInfo.Symbol.Name} {{ get; set; }}");

                    if (index < Input.ParameterInfos.Count - 1)
                        sb.AppendLine();
                }

                sb.AppendLine();
                sb.AppendLine("/// <inheritdoc />");
                var asyncKeyword = Input.IsAsync ? "async " : "";
                var methodName = Input.IsAsync ? "RunAsync" : "Run";
                using (sb.AppendBlockStart($"public {asyncKeyword}{Input.Symbol.ReturnType.ToReferenceString()} {methodName}()"))
                {
                    var returnKeyword = Input.ReturnsValue ? "return " : "";
                    var awaitKeyword = Input.IsAsync ? "await " : "";
                    var cast = Input.ReturnsValue || Input.IsAsync ? $"({Input.Symbol.ReturnType.ToReferenceString()})" : "";

                    using (sb.AppendParamsBlockStart($"{returnKeyword}{awaitKeyword}{cast}InvokeDelegate", ";"))
                    {
                        sb.AppendLine($"\"{Input.Hash}\",");
                        using (sb.AppendBlockStart($"new object[]"))
                        {
                            foreach (var parameterInfo in Input.ParameterInfos)
                            {
                                sb.AppendLine($"{parameterInfo.Symbol.Name}, ");
                            }
                        }
                    }
                }

                sb.AppendLine();
                sb.AppendLine("[System.Runtime.CompilerServices.ModuleInitializerAttribute]");
                using (sb.AppendBlockStart("internal static void Initialize()"))
                {
                    sb.AppendLine("// Register this definition class so that it can be found by the command as delegate hash.");
                    sb.AppendLine($"Register<{GeneratedClassFullName}>(\"{Input.Hash}\");");
                }
            }
        }
    }
}
