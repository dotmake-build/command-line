using System;
using Microsoft.CodeAnalysis;

namespace DotMake.CommandLine.SourceGeneration
{
    public class CliCommandHandlerInfo : CliSymbolInfo, IEquatable<CliCommandHandlerInfo>
    {
        private const string CliContextFullName = "DotMake.CommandLine.CliContext";
        public const string DiagnosticName = "CLI command handler";

        public CliCommandHandlerInfo(IMethodSymbol symbol, SyntaxNode syntaxNode, SemanticModel semanticModel, CliCommandInfo parent)
            : base(symbol, syntaxNode, semanticModel)
        {
            Symbol = symbol;
            Parent = parent;

            if (symbol.IsAsync || symbol.ReturnType.IsTask() || symbol.ReturnType.IsTaskInt())
            {
                IsAsync = true;
                ReturnsVoid = symbol.ReturnType.IsTask();
                ReturnsValue = symbol.ReturnType.IsTaskInt();
            }
            else
            {
                ReturnsVoid = symbol.ReturnsVoid;
                ReturnsValue = (symbol.ReturnType.SpecialType == SpecialType.System_Int32);
            }

            HasNoParameter = (symbol.Parameters.Length == 0);
            HasCliContextParameter = (symbol.Parameters.Length == 1)
                                            && (symbol.Parameters[0].Type.ToCompareString() == CliContextFullName);

            HasCorrectSignature = (ReturnsVoid || ReturnsValue) && (HasNoParameter || HasCliContextParameter);

            if (IsAsync)
                SignaturePriority++;
            if (ReturnsValue)
                SignaturePriority++;
            if (HasCliContextParameter)
                SignaturePriority++;

            if (HasCorrectSignature)
                Analyze();
        }

        public new IMethodSymbol Symbol { get; }

        public CliCommandInfo Parent { get; }

        public bool IsAsync { get; }

        public bool ReturnsVoid { get; }

        public bool ReturnsValue { get; }

        public bool HasNoParameter { get; }

        public bool HasCliContextParameter { get; }

        public bool HasCorrectSignature { get; }

        public int SignaturePriority { get; }

        private void Analyze()
        {
            if ((Symbol.DeclaredAccessibility != Accessibility.Public && Symbol.DeclaredAccessibility != Accessibility.Internal)
                || Symbol.IsStatic)
                AddDiagnostic(DiagnosticDescriptors.WarningMethodNotPublicNonStatic, DiagnosticName);
            else if (Symbol.IsGenericMethod)
                AddDiagnostic(DiagnosticDescriptors.ErrorMethodNotNonGeneric, DiagnosticName);
        }

        public static bool HasCorrectName(IMethodSymbol symbol)
        {
            return symbol.IsAsync || symbol.ReturnType.IsTask() || symbol.ReturnType.IsTaskInt()
                ? (symbol.Name == "RunAsync")
                : (symbol.Name == "Run");
        }

        public void AppendCSharpCallString(CodeStringBuilder sb, string varCliContext = null)
        {
            sb.Append(Symbol.Name);
            sb.Append("(");
            if (HasCliContextParameter)
                sb.Append(varCliContext);
            sb.Append(")");
        }

        public bool Equals(CliCommandHandlerInfo other)
        {
            return base.Equals(other);
        }
    }
}
