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
            if (node.Parent != null && node.Parent.IsKind(SyntaxKind.SimpleMemberAccessExpression))
                return node;

            var symbolInfo = semanticModel.GetSymbolInfo(node);
            if (symbolInfo.Symbol is null) return node; // give up
            return SyntaxFactory.IdentifierName(symbolInfo.Symbol.ToReferenceString());
        }

        public override SyntaxNode VisitMemberAccessExpression(MemberAccessExpressionSyntax node)
        {
            if (node.Parent != null && node.Parent.IsKind(SyntaxKind.SimpleMemberAccessExpression))
                return node;

            return ResolveMemberAccessSyntaxTree(node);
        }

        private MemberAccessExpressionSyntax ResolveMemberAccessSyntaxTree(MemberAccessExpressionSyntax node)
        {
            // for example: Microsoft.Xna.Color.Transparent;  identifier name is Transparent, expression is Microsoft.Xna.Color
            // Then we have a russian nesting doll of SimpleMemberAccessExpression until the expression is just an IdentifierNameSyntax
            //https://stackoverflow.com/questions/77259408/how-can-i-qualify-symbols-within-a-roslyn-syntax-with-a-csharpsyntaxrewriter

            if (node.Expression is MemberAccessExpressionSyntax access)
                return node.WithExpression(ResolveMemberAccessSyntaxTree(access));

            if (node.Expression is IdentifierNameSyntax name)
            {
                var symbolInfo = semanticModel.GetSymbolInfo(name);
                if (symbolInfo.Symbol is null) return node; // give up
                return node.WithExpression(name.WithIdentifier(SyntaxFactory.Identifier(symbolInfo.Symbol.ToReferenceString())));
            }

            // we don't know how to nest further!
            return node;
        }
    }
}
