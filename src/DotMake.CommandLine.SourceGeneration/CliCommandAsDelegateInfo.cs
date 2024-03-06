using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace DotMake.CommandLine.SourceGeneration
{
    public class CliCommandAsDelegateInfo : CliSymbolInfo, IEquatable<CliCommandAsDelegateInfo>
    {
        private const string TaskFullName = "System.Threading.Tasks.Task";
        private const string TaskIntFullName = "System.Threading.Tasks.Task<System.Int32>";
        public static readonly string CliCommandAsDelegateFullName = "DotMake.CommandLine.CliCommandAsDelegate";

        public CliCommandAsDelegateInfo(ISymbol symbol, SyntaxNode syntaxNode, SemanticModel semanticModel)
            : base(symbol, syntaxNode, semanticModel)
        {
            Symbol = (IMethodSymbol)symbol;

            if (Symbol.IsAsync || Symbol.ReturnType.ToCompareString() is TaskFullName or TaskIntFullName)
            {
                IsAsync = true;
                ReturnsVoid = (Symbol.ReturnType.ToCompareString() == TaskFullName);
                ReturnsValue = (Symbol.ReturnType is INamedTypeSymbol namedTypeSymbol)
                               && namedTypeSymbol.IsGenericType
                               && namedTypeSymbol.BaseType?.ToCompareString() == TaskFullName
                               && (namedTypeSymbol.TypeArguments.FirstOrDefault().SpecialType == SpecialType.System_Int32);
            }
            else
            {
                ReturnsVoid = Symbol.ReturnsVoid;
                ReturnsValue = (Symbol.ReturnType.SpecialType == SpecialType.System_Int32);
            }

            Analyze();
            if (HasProblem)
                return;

            AttributeData = Symbol.GetAttributes()
                .FirstOrDefault(a => a.AttributeClass.ToCompareString() == CliCommandInfo.AttributeFullName);

            foreach (var parameter in Symbol.Parameters)
            {
                ParameterInfos.Add(new ParameterInfo
                {
                    Symbol = parameter,
                    AttributeData = parameter.GetAttributes()
                        .FirstOrDefault(a => a.AttributeClass.ToCompareString() == CliOptionInfo.AttributeFullName
                                             || a.AttributeClass.ToCompareString() == CliArgumentInfo.AttributeFullName),
                    DefaultSyntax = (parameter.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() as ParameterSyntax)
                        ?.Default?.Value
                });
            }

            Hash = GenerateHash();
            GeneratedClassName = "CliCommandAsDelegate_" + Hash;
            GeneratedClassNamespace = CliCommandInfo.GeneratedSubNamespace;
            GeneratedClassFullName = string.IsNullOrEmpty(GeneratedClassNamespace)
                ? GeneratedClassName
                : GeneratedClassNamespace + "." + GeneratedClassName;
        }

        public static bool IsMatch(SyntaxNode syntaxNode)
        {
            return syntaxNode is InvocationExpressionSyntax invocationExpressionSyntax
                   && invocationExpressionSyntax.Expression is MemberAccessExpressionSyntax memberAccessExpressionSyntax
                   && ((memberAccessExpressionSyntax.Expression is IdentifierNameSyntax identifierNameSyntax
                        && identifierNameSyntax.Identifier.ValueText == "Cli")
                       || memberAccessExpressionSyntax.Expression.NormalizeWhitespace().ToString() == "DotMake.CommandLine.Cli")
                   && memberAccessExpressionSyntax.Name.Identifier.ValueText == "Run"
                   && invocationExpressionSyntax.ArgumentList.Arguments.Count > 0 //maybe have optional CliSettings parameter
                   && (invocationExpressionSyntax.ArgumentList.Arguments[0].Expression is ParenthesizedLambdaExpressionSyntax
                       || invocationExpressionSyntax.ArgumentList.Arguments[0].Expression is AnonymousMethodExpressionSyntax
                       || invocationExpressionSyntax.ArgumentList.Arguments[0].Expression is IdentifierNameSyntax);
        }

        public static CliCommandAsDelegateInfo From(GeneratorSyntaxContext generatorSyntaxContext)
        {
            var invocationExpressionSyntax = (InvocationExpressionSyntax)generatorSyntaxContext.Node;
            var argumentExpressionSyntax = invocationExpressionSyntax.ArgumentList.Arguments[0].Expression;

            var operation = generatorSyntaxContext.SemanticModel.GetOperation(argumentExpressionSyntax);

            if (operation is IAnonymousFunctionOperation anonymousFunctionOperation)
                return new CliCommandAsDelegateInfo(
                    anonymousFunctionOperation.Symbol,
                    argumentExpressionSyntax,
                    generatorSyntaxContext.SemanticModel);


            if (operation is IMethodReferenceOperation methodReferenceOperation)
                return new CliCommandAsDelegateInfo(
                    methodReferenceOperation.Method,
                    methodReferenceOperation.Method.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax(),
                    generatorSyntaxContext.SemanticModel);

            return null;
        }

        private void Analyze()
        {
            if (!ReturnsVoid && !ReturnsValue)
                AddDiagnostic(DiagnosticDescriptors.ErrorDelegateNotCorrect);
        }

        public string GenerateHash()
        {
            return GenerateString().GetStableStringHashCode32();
        }

        //The generated string should match the one generated in DotMake.CommandLine.CliCommandAsDelegate
        //So that the generated hash matches.
        public string GenerateString()
        {
            var sb = new StringBuilder();

            AppendMethodData(sb);

            return sb.ToString();
        }

        private void AppendMethodData(StringBuilder sb)
        {
            AppendAttributeData(sb, AttributeData);

            sb.AppendLine(Symbol.ReturnType.ToCompareString());

            foreach (var parameterInfo in ParameterInfos)
            {
                AppendAttributeData(sb, parameterInfo.AttributeData);

                sb.Append(parameterInfo.Symbol.Type.ToCompareString());
                sb.Append(",");
                sb.Append(parameterInfo.Symbol.Name);
                sb.Append(",");
                if (parameterInfo.Symbol.HasExplicitDefaultValue)
                    sb.Append(parameterInfo.Symbol.ExplicitDefaultValue);
                sb.AppendLine();
            }
        }

        private static void AppendAttributeData(StringBuilder sb, AttributeData attributeData)
        {
            sb.Append("[");

            if (attributeData != null)
            {
                sb.Append($"{attributeData.AttributeClass.ToCompareString()}(");

                foreach (var kvp in attributeData.NamedArguments)
                {
                    var typedConstant = kvp.Value;
                    var value = typedConstant.IsNull
                        ? "null"
                        : typedConstant.Kind == TypedConstantKind.Array
                            ? string.Join(",", typedConstant.Values)
                            : typedConstant.Value?.ToString();

                    sb.Append($"{kvp.Key}=\"{value}\",");
                }

                sb.Append(")");
            }

            sb.AppendLine("]");
        }

        public new IMethodSymbol Symbol { get; }

        public AttributeData AttributeData { get; }

        public AttributeSyntax AttributeSyntax => AttributeData?.ApplicationSyntaxReference?.GetSyntax() as AttributeSyntax;

        public List<ParameterInfo> ParameterInfos { get; } = new List<ParameterInfo>();

        public bool IsAsync { get; }

        public bool ReturnsVoid { get; }

        public bool ReturnsValue { get; }

        public string Hash { get; }

        public string GeneratedClassName { get; }

        public string GeneratedClassNamespace { get; }

        public string GeneratedClassFullName { get; }
        
        public void AppendCSharpDefineString(CodeStringBuilder sb)
        {
            using var namespaceBlock = sb.AppendBlockStart($"namespace {GeneratedClassNamespace}");

            sb.AppendLine("/// <inheritdoc />");

            if (AttributeSyntax != null)
                sb.AppendLine($"[{AttributeData.AttributeClass.ToReferenceString()}{AttributeSyntax.ArgumentList}]");
            else
                sb.AppendLine($"[{CliCommandInfo.AttributeFullName}]");

            using (sb.AppendBlockStart($"public class {GeneratedClassName} : {CliCommandAsDelegateFullName}"))
            {
                for (var index = 0; index < ParameterInfos.Count; index++)
                {
                    var parameterInfo = ParameterInfos[index];

                    sb.AppendLine("/// <inheritdoc />");

                    if (parameterInfo.AttributeSyntax != null)
                        sb.AppendLine(
                            $"[{parameterInfo.AttributeData.AttributeClass.ToReferenceString()}{parameterInfo.AttributeSyntax.ArgumentList}]");
                    else
                        sb.AppendLine($"[{CliOptionInfo.AttributeFullName}]");

                    if (parameterInfo.DefaultSyntax != null)
                        sb.AppendLine(
                            $"public {parameterInfo.Symbol.Type.ToReferenceString()} {parameterInfo.Symbol.Name} {{ get; set; }} = {parameterInfo.DefaultSyntax};");
                    else
                        sb.AppendLine(
                            $"public {parameterInfo.Symbol.Type.ToReferenceString()} {parameterInfo.Symbol.Name} {{ get; set; }}");

                    if (index < ParameterInfos.Count - 1)
                        sb.AppendLine();
                }

                sb.AppendLine();
                sb.AppendLine("/// <inheritdoc />");
                var asyncKeyword = IsAsync ? "async " : "";
                var methodName = IsAsync ? "RunAsync" : "Run";
                using (sb.AppendBlockStart($"public {asyncKeyword}{Symbol.ReturnType.ToReferenceString()} {methodName}()"))
                {
                    var returnKeyword = ReturnsValue ? "return " : "";
                    var awaitKeyword = IsAsync ? "await " : "";
                    var cast = ReturnsValue || IsAsync ? $"({Symbol.ReturnType.ToReferenceString()})" : "";

                    using (sb.AppendParamsBlockStart($"{returnKeyword}{awaitKeyword}{cast}InvokeDelegate", ";"))
                    {
                        sb.AppendLine($"\"{Hash}\",");
                        using (sb.AppendBlockStart($"new object[]"))
                        {
                            foreach (var parameterInfo in ParameterInfos)
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
                    sb.AppendLine($"Register<{GeneratedClassFullName}>(\"{Hash}\");");
                }
            }
        }

        public bool Equals(CliCommandAsDelegateInfo other)
        {
            return base.Equals(other);
        }

        public class ParameterInfo
        {
            public IParameterSymbol Symbol { get; internal set; }

            public AttributeData AttributeData { get; internal set; }

            public AttributeSyntax AttributeSyntax => AttributeData?.ApplicationSyntaxReference?.GetSyntax() as AttributeSyntax;
                
            public ExpressionSyntax DefaultSyntax { get; internal set; }
        }
    }
}
