using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

namespace DotMake.CommandLine.SourceGeneration.Inputs
{
    //Important: IEquatable.Equals is required for Roslyn cache to work
    //WithComparer also may not work?
    //https://github.com/dotnet/roslyn/issues/66324
    //More about Equatable:
    //https://github.com/dotnet/roslyn/issues/68070
    public abstract class InputBase : IEquatable<InputBase>
    {
        protected InputBase(ISymbol symbol, SyntaxNode syntaxNode, SemanticModel semanticModel)
        {
            //Should not store symbol or semanticModel because they will root Compilation object in memory.
            //So previous compilations would be continued to be kept in memory.
            //But for now, we need them due to heavy/complex usage.
            //One way is to set them to null in OutputBase, as it means if it goes there, it's at generated state
            //But for  CliCommandInput, we need to re-set those values because sometimes that input will be re-generated
            //even self is not changed.
            //Another solution could be using symbol names like below, or create a type encapsulating important properties:
            //SymbolName = symbol.Name;
            //SymbolNamespace = symbol.GetNamespaceOrEmpty();
            //SymbolFullName = symbol.ToReferenceString();
            Symbol = symbol;
            SemanticModel = semanticModel;

            //Note that SyntaxNode seems to include applied attribute to a symbol (so changes reflected)
            SyntaxNode = syntaxNode ?? symbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax();

            //Location = syntaxNode?.GetLocation();
            Location = symbol.Locations.FirstOrDefault();

            Language = semanticModel.Compilation.Language;
            if (syntaxNode?.SyntaxTree.Options is CSharpParseOptions options)
                LanguageVersion = (int)options.LanguageVersion;
        }

        protected InputBase(Compilation compilation)
        {
            SyntaxNode = compilation.SyntaxTrees.FirstOrDefault()?.GetRoot();

            Language = compilation.Language;
            if (SyntaxNode?.SyntaxTree.Options is CSharpParseOptions options)
                LanguageVersion = (int)options.LanguageVersion;
        }

        public ISymbol Symbol { get; }
        
        public SyntaxNode SyntaxNode { get; }

        public SemanticModel SemanticModel { get; }

        public Location Location { get; }

        public string Language { get; }

        public int LanguageVersion { get; }

        public List<Diagnostic> Diagnostics { get; } = new();

        public bool HasProblem { get; private set; }

        public abstract void Analyze(ISymbol symbol);

        public void AddDiagnostic(DiagnosticDescriptor diagnosticDescriptor, params object[] messageArgs)
        {
            //Note targetSymbol should be Symbol or always point to a symbol in same SyntaxTree (same file)
            //otherwise ReportDiagnostic with outdated Location (bound to a SyntaxTree) crashes other analyzers/features in VS

            messageArgs = new[]
            {
                Symbol.Name
            }.Concat(messageArgs).ToArray();

            HasProblem = (diagnosticDescriptor.DefaultSeverity == DiagnosticSeverity.Error
                          || diagnosticDescriptor.DefaultSeverity == DiagnosticSeverity.Warning);

            var diagnostic = Diagnostic.Create(
                diagnosticDescriptor,
                Location,
                messageArgs);

            Diagnostics.Add(diagnostic);
        }

        public void RemoveDiagnostic(DiagnosticDescriptor diagnosticDescriptor)
        {
            Diagnostics.RemoveAll(d => d.Descriptor.Equals(diagnosticDescriptor));
        }

        public virtual IEnumerable<Diagnostic> GetAllDiagnostics()
        {
            return Diagnostics;
        }

        /*
        Notes:
        If we use SyntaxNode.IsEquivalentTo(topLevel: true) alone, it works good for caching (we can ignore whitespace 
        and nodes inside method bodies, in classes), but SyntaxNode (and Symbol, SemanticModel) seems to no longer refer 
        to the most recent ones (especially when a class is commented out and reverted). Unfortunately, Roslyn seems to
        create new a SyntaxNode (actually a SyntaxTree for the whole file) even if we change whitespace or nodes inside 
        method bodies, in classes. As a side effect, it can cause location error for sourceProductionContext.ReportDiagnostic 
        or crashing of other analyzers/features still in VS 17.8.2.

        So we simply use reference equality for SyntaxNode to see if we are still in same node and tree.

        One possible solution is to use safe location which does not point to any SyntaxTree, e.g.:
	        var location = symbol.Locations.FirstOrDefault();
	        var safeLocation = Location.Create(location.SyntaxTree.FilePath, location.SourceSpan, location.GetLineSpan().Span);
        This creates ExternalFileLocation from a SourceLocation (which depends to a SyntaxTree)
        Reference: https://github.com/dotnet/roslyn/issues/62269
        Problems:
        - location.SyntaxTree.FilePath does not seem to work (comes empty? in the error list .csproj is shown not the actual file)
	        but symbol.DeclaringSyntaxReferences.FirstOrDefault().SyntaxTree.FilePath works
        - If we use SyntaxNode.IsEquivalentTo(other?.SyntaxNode, true) for Equals
	        then location will be shifted because if we add/remove comments, the location will not be the same although we think it's same SyntaxNode
        - we see duplicate diagnostics in the error list?

        So it's better to stick to reference equality for SyntaxNode.
        */
        public bool Equals(InputBase other)
        {
            return (SyntaxNode == other?.SyntaxNode);
            //return SyntaxNode.IsEquivalentTo(other?.SyntaxNode, true);
        }

        public override bool Equals(object obj)
        {
            if (obj is null) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != GetType()) return false;
            return Equals((InputBase)obj);
        }

        //Override Object.GetHashCode, just in case
        //Unfortunately, Roslyn cache does not call GetHashCode but only Equals (inside NodeStateTable<T>.TryModifyEntry)
        public override int GetHashCode()
        {
            return SyntaxNode?.GetHashCode() ?? 0;
        }
    }
}
