using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace DotMake.CommandLine.SourceGeneration
{
	//Important: IEquatable.Equals is required for Roslyn cache to work
	//WithComparer also may not work?
	//https://github.com/dotnet/roslyn/issues/66324
	//More about Equatable:
	//https://github.com/dotnet/roslyn/issues/68070
	public class CliSymbolInfo : IEquatable<CliSymbolInfo>
	{
		public CliSymbolInfo(ISymbol symbol, SyntaxNode syntaxNode, SemanticModel semanticModel)
		{
			Symbol = symbol;
			//SyntaxNode seems to include applied attribute (changes reflected)
			SyntaxNode = syntaxNode ?? symbol.DeclaringSyntaxReferences.FirstOrDefault()?.GetSyntax();
			SemanticModel = semanticModel;
		}

		public ISymbol Symbol { get; }

		public SyntaxNode SyntaxNode { get; }

		public SemanticModel SemanticModel { get; }

		public List<Diagnostic> Diagnostics { get; } = new List<Diagnostic>();

		public bool HasProblem { get; private set; }

		public void AddDiagnostic(DiagnosticDescriptor diagnosticDescriptor, params object[] messageArgs)
		{
			AddDiagnostic(diagnosticDescriptor, Symbol, true, messageArgs);
		}

		public void AddDiagnostic(DiagnosticDescriptor diagnosticDescriptor, ISymbol targetSymbol, params object[] messageArgs)
		{
			AddDiagnostic(diagnosticDescriptor, targetSymbol, true, messageArgs);
		}

		public void AddDiagnostic(DiagnosticDescriptor diagnosticDescriptor, bool markAsProblem, params object[] messageArgs)
		{
			AddDiagnostic(diagnosticDescriptor, Symbol, markAsProblem, messageArgs);
		}

		public void AddDiagnostic(DiagnosticDescriptor diagnosticDescriptor, ISymbol targetSymbol, bool markAsProblem, params object[] messageArgs)
		{
			//Note targetSymbol should be Symbol or always point to a symbol in same SyntaxTree (same file)
			//otherwise ReportDiagnostic with outdated Location (bound to a SyntaxTree) crashes other analyzers/features in VS

			messageArgs = new[]
			{
				targetSymbol.Name
			}.Concat(messageArgs).ToArray();

			if (markAsProblem
				&& (diagnosticDescriptor.DefaultSeverity == DiagnosticSeverity.Error
					|| diagnosticDescriptor.DefaultSeverity == DiagnosticSeverity.Warning))
				HasProblem = true;
			
			var diagnostic = Diagnostic.Create(
				diagnosticDescriptor,
				targetSymbol.Locations.FirstOrDefault(),
				messageArgs);
			
			Diagnostics.Add(diagnostic);
		}

		public virtual void ReportDiagnostics(SourceProductionContext sourceProductionContext)
		{
			foreach (var diagnostic in Diagnostics)
				sourceProductionContext.ReportDiagnosticSafe(diagnostic);
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
		public bool Equals(CliSymbolInfo other)
		{
			return (SyntaxNode == other?.SyntaxNode);
			//return SyntaxNode.IsEquivalentTo(other?.SyntaxNode, true);
		}

		//Override Object.GetHashCode, just in case
		//Unfortunately, Roslyn cache does not call GetHashCode but only Equals (inside NodeStateTable<T>.TryModifyEntry)
		public override int GetHashCode()
		{
			return SyntaxNode?.GetHashCode() ?? 0;
		}
	}
}