using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace DotMake.CommandLine.SourceGeneration.Util
{
    public class QualifiedSyntaxRewriter : CSharpSyntaxRewriter
    {
        private readonly SemanticModel semanticModel;

        public QualifiedSyntaxRewriter(SemanticModel semanticModel)
        {
            this.semanticModel = semanticModel;
        }

        public override SyntaxNode VisitIdentifierName(IdentifierNameSyntax node)
        {
            if (node.Parent is QualifiedNameSyntax)
                return base.VisitIdentifierName(node);

            var symbol = semanticModel.GetSymbolInfo(node).Symbol;
            if (symbol == null)
                return base.VisitIdentifierName(node);

            // 1) Qualify type identifiers wherever they appear
            if (symbol is ITypeSymbol typeSymbol)
            {
                var qualifiedSyntax = ParseQualifiedType(typeSymbol).WithTriviaFrom(node);
                return FixIfInsideInterpolation(node, qualifiedSyntax);
            }

            // 2) Qualify static members referenced by simple identifiers (e.g., StaticHelper())
            // Only when not already part of a member access (to avoid rewriting the .Name slot)
            if (node.Parent is MemberAccessExpressionSyntax)
                return base.VisitIdentifierName(node);

            if (symbol.IsStatic && IsMember(symbol))
            {
                var qualifiedSyntax = BuildMemberAccess(symbol).WithTriviaFrom(node);
                return FixIfInsideInterpolation(node, qualifiedSyntax);
            }

            return base.VisitIdentifierName(node);
        }

        public override SyntaxNode VisitQualifiedName(QualifiedNameSyntax node)
        {
            // DO NOT visit left side (prevents enforcing SimpleName crash)
            // DO visit right (IdentifierName OR GenericName)
            var right = (SimpleNameSyntax)Visit(node.Right);

            var rewritten = node.WithRight(right);

            var symbol = semanticModel.GetSymbolInfo(rewritten).Symbol;

            if (symbol is ITypeSymbol typeSymbol)
            {
                var qualifiedSyntax = ParseQualifiedType(typeSymbol).WithTriviaFrom(node);
                return FixIfInsideInterpolation(node, qualifiedSyntax);
            }

            return rewritten;
        }

        public override SyntaxNode VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
        {
            // Query symbol BEFORE rewrite
            var leftSymbol = semanticModel.GetSymbolInfo(node.Expression).Symbol;

            // Do not recurse first — if left is type, rewrite entire node atomically
            if (leftSymbol is ITypeSymbol leftType)
            {
                var qualifiedLeft = ParseQualifiedType(leftType).WithTriviaFrom(node.Expression);

                // DO visit right (IdentifierName OR GenericName)
                var right = (SimpleNameSyntax)Visit(node.Name);

                var rewritten = node
                    .WithExpression(qualifiedLeft)
                    .WithName(right);

                return FixIfInsideInterpolation(node, rewritten);
            }

            return base.VisitMemberAccessExpression(node);
        }

        private SyntaxNode FixIfInsideInterpolation(SyntaxNode node, ExpressionSyntax qualifiedSyntax)
        {
            //if inside interpolation string $"{...}" wrap it with () as types with global:: will cause error
            //because : character needs to be escaped in interpolation strings
            return (node.FirstAncestorOrSelf<InterpolationSyntax>() != null
                    && node.Parent is not ParenthesizedExpressionSyntax)
                ? SyntaxFactory.ParenthesizedExpression(qualifiedSyntax)
                : qualifiedSyntax;
        }

        // Utility: determine if a symbol is a member we should qualify
        private static bool IsMember(ISymbol symbol)
        {
            switch (symbol.Kind)
            {
                case SymbolKind.Method:
                case SymbolKind.Property:
                case SymbolKind.Field:
                case SymbolKind.Event:
                    return true;
                default:
                    return false;
            }
        }

        // Utility: parse the fully-qualified type (global::Namespace.Type)
        private static TypeSyntax ParseQualifiedType(ITypeSymbol typeSymbol)
        {
            //var fq = typeSymbol.ToDisplayString(SymbolDisplayFormat.FullyQualifiedFormat);
            var fq = typeSymbol.ToReferenceString();
            return SyntaxFactory.ParseTypeName(fq);
        }

        // Utility: build ContainingType.MemberName member access for static members
        private MemberAccessExpressionSyntax BuildMemberAccess(ISymbol symbol)
        {
            var containingType = (ITypeSymbol)symbol.ContainingType;
            var typeExpr = ParseQualifiedType(containingType);
            var name = SyntaxFactory.IdentifierName(symbol.Name);

            return SyntaxFactory.MemberAccessExpression(
                SyntaxKind.SimpleMemberAccessExpression,
                typeExpr,
                name
            );
        }
    }
}
