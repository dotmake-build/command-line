using System;
using Microsoft.CodeAnalysis;

namespace DotMake.CommandLine.SourceGeneration.Inputs
{
    public class CliParentCommandAccessorInput : InputBase, IEquatable<CliParentCommandAccessorInput>
    {
        public const string DiagnosticName = "CLI parent command accessor";

        public CliParentCommandAccessorInput(ISymbol symbol, SyntaxNode syntaxNode, SemanticModel semanticModel)
            : base(symbol, syntaxNode, semanticModel)
        {
            Symbol = (IPropertySymbol)symbol;

            Analyze(symbol);
        }

        public new IPropertySymbol Symbol { get; }

        public sealed override void Analyze(ISymbol symbol)
        {
            if ((Symbol.DeclaredAccessibility != Accessibility.Public && Symbol.DeclaredAccessibility != Accessibility.Internal)
                || Symbol.IsStatic)
                AddDiagnostic(DiagnosticDescriptors.WarningPropertyNotPublicNonStatic, DiagnosticName);
            else
            {
                if (Symbol.GetMethod == null
                    || (Symbol.GetMethod.DeclaredAccessibility != Accessibility.Public && Symbol.GetMethod.DeclaredAccessibility != Accessibility.Internal))
                    AddDiagnostic(DiagnosticDescriptors.ErrorPropertyHasNotPublicGetter, DiagnosticName);

                if (Symbol.SetMethod == null
                    || (Symbol.SetMethod.DeclaredAccessibility != Accessibility.Public && Symbol.SetMethod.DeclaredAccessibility != Accessibility.Internal))
                    AddDiagnostic(DiagnosticDescriptors.ErrorPropertyHasNotPublicSetter, DiagnosticName);
            }
        }

        public bool Equals(CliParentCommandAccessorInput other)
        {
            return base.Equals(other);
        }
    }
}
