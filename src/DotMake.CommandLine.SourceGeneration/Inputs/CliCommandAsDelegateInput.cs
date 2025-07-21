using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DotMake.CommandLine.SourceGeneration.Util;
using DotMake.CommandLine.Util;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.CodeAnalysis.Operations;

namespace DotMake.CommandLine.SourceGeneration.Inputs
{
    public class CliCommandAsDelegateInput : InputBase, IEquatable<CliCommandAsDelegateInput>
    {
        public CliCommandAsDelegateInput(ISymbol symbol, SyntaxNode syntaxNode, SemanticModel semanticModel)
            : base(symbol, syntaxNode, semanticModel)
        {
            Symbol = (IMethodSymbol)symbol;

            if (Symbol.IsAsync || Symbol.ReturnType.IsTask() || Symbol.ReturnType.IsTaskInt())
            {
                IsAsync = true;
                ReturnsVoid = Symbol.ReturnType.IsTask();
                ReturnsValue = Symbol.ReturnType.IsTaskInt();
            }
            else
            {
                ReturnsVoid = Symbol.ReturnsVoid;
                ReturnsValue = (Symbol.ReturnType.SpecialType == SpecialType.System_Int32);
            }

            Analyze(symbol);
            if (HasProblem)
                return;

            AttributeData = Symbol.GetAttributes()
                .FirstOrDefault(a => a.AttributeClass.ToCompareString() == CliCommandInput.AttributeFullName);

            foreach (var parameter in Symbol.Parameters)
            {
                ParameterInfos.Add(new ParameterInfo
                {
                    Symbol = parameter,
                    AttributeData = parameter.GetAttributes()
                        .FirstOrDefault(a => a.AttributeClass.ToCompareString() == CliOptionInput.AttributeFullName
                                             || a.AttributeClass.ToCompareString() == CliArgumentInput.AttributeFullName),
                    DefaultSyntax = (parameter.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax() as ParameterSyntax)
                        ?.Default?.Value
                });
            }

            Hash = GenerateHash();
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

        public static CliCommandAsDelegateInput From(GeneratorSyntaxContext generatorSyntaxContext)
        {
            var invocationExpressionSyntax = (InvocationExpressionSyntax)generatorSyntaxContext.Node;
            var argumentExpressionSyntax = invocationExpressionSyntax.ArgumentList.Arguments[0].Expression;

            var operation = generatorSyntaxContext.SemanticModel.GetOperation(argumentExpressionSyntax);

            if (operation is IAnonymousFunctionOperation anonymousFunctionOperation)
                return new CliCommandAsDelegateInput(
                    anonymousFunctionOperation.Symbol,
                    argumentExpressionSyntax,
                    generatorSyntaxContext.SemanticModel);


            if (operation is IMethodReferenceOperation methodReferenceOperation)
                return new CliCommandAsDelegateInput(
                    methodReferenceOperation.Method,
                    methodReferenceOperation.Method.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax(),
                    generatorSyntaxContext.SemanticModel);

            return null;
        }

        public sealed override void Analyze(ISymbol symbol)
        {
            if (!ReturnsVoid && !ReturnsValue)
                AddDiagnostic(DiagnosticDescriptors.ErrorDelegateNotCorrect);
        }

        public string GenerateHash()
        {
            return HashUtil.GetStableStringHashCode32(GenerateString());
        }

        //The generated string should match the one generated in DotMake.CommandLine.CliCommandAsDelegate
        //So that the generated "hash" matches.
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

        public List<ParameterInfo> ParameterInfos { get; } = new();

        public bool IsAsync { get; }

        public bool ReturnsVoid { get; }

        public bool ReturnsValue { get; }

        public string Hash { get; }

        public bool Equals(CliCommandAsDelegateInput other)
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
