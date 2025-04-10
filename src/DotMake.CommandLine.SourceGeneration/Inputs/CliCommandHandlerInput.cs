using System;
using DotMake.CommandLine.SourceGeneration.Util;
using Microsoft.CodeAnalysis;

namespace DotMake.CommandLine.SourceGeneration.Inputs
{
    public class CliCommandHandlerInput : InputBase, IEquatable<CliCommandHandlerInput>
    {
        private const string CliContextFullName = "DotMake.CommandLine.CliContext";
        public const string DiagnosticName = "CLI command handler";

        public CliCommandHandlerInput(IMethodSymbol symbol, SyntaxNode syntaxNode, SemanticModel semanticModel, CliCommandInput parent)
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
                Analyze(symbol);
        }

        public new IMethodSymbol Symbol { get; }

        public CliCommandInput Parent { get; }

        public bool IsAsync { get; }

        public bool ReturnsVoid { get; }

        public bool ReturnsValue { get; }

        public bool HasNoParameter { get; }

        public bool HasCliContextParameter { get; }

        public bool HasCorrectSignature { get; }

        public int SignaturePriority { get; }

        public sealed override void Analyze(ISymbol symbol)
        {
            var methodSymbol = (IMethodSymbol)symbol;

            if ((methodSymbol.DeclaredAccessibility != Accessibility.Public && methodSymbol.DeclaredAccessibility != Accessibility.Internal)
                || methodSymbol.IsStatic)
                AddDiagnostic(DiagnosticDescriptors.WarningMethodNotPublicNonStatic, DiagnosticName);
            else if (methodSymbol.IsGenericMethod)
                AddDiagnostic(DiagnosticDescriptors.ErrorMethodNotNonGeneric, DiagnosticName);
        }

        public static bool HasCorrectName(IMethodSymbol symbol)
        {
            return symbol.IsAsync || symbol.ReturnType.IsTask() || symbol.ReturnType.IsTaskInt()
                ? (symbol.Name == "RunAsync")
                : (symbol.Name == "Run");
        }

        public bool Equals(CliCommandHandlerInput other)
        {
            return base.Equals(other);
        }
    }
}
