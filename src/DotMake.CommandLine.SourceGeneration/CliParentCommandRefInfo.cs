using System;
using Microsoft.CodeAnalysis;

namespace DotMake.CommandLine.SourceGeneration
{
    public class CliParentCommandRefInfo : CliSymbolInfo, IEquatable<CliParentCommandRefInfo>
    {
        public const string DiagnosticName = "CLI parent command reference";

        public CliParentCommandRefInfo(IPropertySymbol symbol, SyntaxNode syntaxNode, SemanticModel semanticModel,
            int parentTreeIndex, CliCommandSettings parentCommandSettings) : base(symbol, syntaxNode, semanticModel)
        {
            ParentTreeIndex = parentTreeIndex;
            ParentCommandSettings = parentCommandSettings;

            Analyze();

            if (HasProblem)
                return;
        }

        public int ParentTreeIndex { get; }
        public CliCommandSettings ParentCommandSettings { get; }
        public new IPropertySymbol Symbol => (IPropertySymbol)base.Symbol;

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

        public bool Equals(CliParentCommandRefInfo other)
        {
            return base.Equals(other);
        }
    }
}
