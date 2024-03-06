using System;
using Microsoft.CodeAnalysis;

namespace DotMake.CommandLine.SourceGeneration
{
    public class CliParentCommandAccessorInfo : CliSymbolInfo, IEquatable<CliParentCommandAccessorInfo>
    {
        public const string DiagnosticName = "CLI parent command accessor";

        public CliParentCommandAccessorInfo(ISymbol symbol, SyntaxNode syntaxNode, SemanticModel semanticModel)
            : base(symbol, syntaxNode, semanticModel)
        {
            Symbol = (IPropertySymbol)symbol;

            Analyze();
        }

        public new IPropertySymbol Symbol;

        private void Analyze()
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

        public bool Equals(CliParentCommandAccessorInfo other)
        {
            return base.Equals(other);
        }
    }
}
