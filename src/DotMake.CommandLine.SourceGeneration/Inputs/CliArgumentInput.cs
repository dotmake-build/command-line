using System;
using System.Collections.Generic;
using System.Linq;
using DotMake.CommandLine.SourceGeneration.Util;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DotMake.CommandLine.SourceGeneration.Inputs
{
    public class CliArgumentInput : InputBase, IEquatable<CliArgumentInput>
    {
        public static readonly string AttributeFullName = typeof(CliArgumentAttribute).FullName;
        public const string DiagnosticName = "CLI argument";

        public CliArgumentInput(ISymbol symbol, SyntaxNode syntaxNode, AttributeData attributeData, SemanticModel semanticModel, CliCommandInput parent)
         : base(symbol, syntaxNode, semanticModel)
        {
            Symbol = (IPropertySymbol)symbol;
            Parent = parent;

            ArgumentParser = new CliArgumentParserInput(Symbol, syntaxNode, semanticModel, this);

            Analyze(Symbol);

            if (HasProblem)
                return;

            AttributeArguments = new AttributeArguments(attributeData, semanticModel);
            if (AttributeArguments.TryGetValue(nameof(CliArgumentAttribute.Order), out var order))
                Order = (int)order;

            if (AttributeArguments.TryGetValue(nameof(CliArgumentAttribute.Required), out var requiredValue))
                Required = (bool)requiredValue;
            else
                Required = (SyntaxNode is PropertyDeclarationSyntax propertyDeclarationSyntax && propertyDeclarationSyntax.Initializer != null)
                    ? propertyDeclarationSyntax.Initializer.Value.IsKind(SyntaxKind.NullLiteralExpression)
                      || propertyDeclarationSyntax.Initializer.Value.IsKind(SyntaxKind.SuppressNullableWarningExpression)
                    : Symbol.Type.IsReferenceType || Symbol.IsRequired;
        }

        public CliArgumentInput(GeneratorAttributeSyntaxContext attributeSyntaxContext)
            : this(attributeSyntaxContext.TargetSymbol,
                attributeSyntaxContext.TargetNode,
                attributeSyntaxContext.Attributes[0],
                attributeSyntaxContext.SemanticModel,
                null)
        {
        }

        public new IPropertySymbol Symbol { get; }

        public CliCommandInput Parent { get; }


        public AttributeArguments AttributeArguments { get; }

        public int Order { get; }

        public bool Required { get; }


        public CliArgumentParserInput ArgumentParser { get; }

        public sealed override void Analyze(ISymbol symbol)
        {
            if ((symbol.DeclaredAccessibility != Accessibility.Public && symbol.DeclaredAccessibility != Accessibility.Internal)
                || symbol.IsStatic)
                AddDiagnostic(DiagnosticDescriptors.WarningPropertyNotPublicNonStatic, DiagnosticName);
            else
            {
                var propertySymbol = (IPropertySymbol)symbol;

                if (propertySymbol.GetMethod == null
                    || (propertySymbol.GetMethod.DeclaredAccessibility != Accessibility.Public && propertySymbol.GetMethod.DeclaredAccessibility != Accessibility.Internal))
                    AddDiagnostic(DiagnosticDescriptors.ErrorPropertyHasNotPublicGetter, DiagnosticName);

                if (propertySymbol.SetMethod == null
                    || (propertySymbol.SetMethod.DeclaredAccessibility != Accessibility.Public && propertySymbol.SetMethod.DeclaredAccessibility != Accessibility.Internal))
                    AddDiagnostic(DiagnosticDescriptors.ErrorPropertyHasNotPublicSetter, DiagnosticName);
            }
        }

        public override IEnumerable<Diagnostic> GetAllDiagnostics()
        {
            return base.GetAllDiagnostics() //self
                .Concat(ArgumentParser.GetAllDiagnostics()); 
        }

        public bool Equals(CliArgumentInput other)
        {
            return base.Equals(other);
        }
    }
}
