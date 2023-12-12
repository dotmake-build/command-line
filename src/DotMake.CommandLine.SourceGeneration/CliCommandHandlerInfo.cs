using System;
using System.Linq;
using Microsoft.CodeAnalysis;

namespace DotMake.CommandLine.SourceGeneration
{
	public class CliCommandHandlerInfo : CliSymbolInfo, IEquatable<CliCommandHandlerInfo>
	{
		private const string TaskFullName = "System.Threading.Tasks.Task";
		private const string InvocationContextFullName = "System.CommandLine.Invocation.InvocationContext";
		public const string DiagnosticName = "CLI command handler";

		public CliCommandHandlerInfo(IMethodSymbol symbol, SyntaxNode syntaxNode, SemanticModel semanticModel, CliCommandInfo parent)
			: base(symbol, syntaxNode, semanticModel)
		{
			Symbol = symbol;
			Parent = parent;

			if (symbol.IsAsync)
			{
				IsAsync = true;
				ReturnsVoid = (symbol.ReturnType.ToDisplayString() == TaskFullName);
				ReturnsValue = (symbol.ReturnType is INamedTypeSymbol namedTypeSymbol) 
				               && namedTypeSymbol.IsGenericType
				               && namedTypeSymbol.BaseType?.ToDisplayString() == TaskFullName
				               && (namedTypeSymbol.TypeArguments.FirstOrDefault().SpecialType == SpecialType.System_Int32);
			}
			else
			{
				ReturnsVoid = symbol.ReturnsVoid;
				ReturnsValue = (symbol.ReturnType.SpecialType == SpecialType.System_Int32);
			}

			HasNoParameter = (symbol.Parameters.Length == 0);
			HasInvocationContextParameter = (symbol.Parameters.Length == 1)
			                                && (symbol.Parameters[0].Type.ToDisplayString() == InvocationContextFullName);

			HasCorrectSignature = (ReturnsVoid || ReturnsValue) && (HasNoParameter || HasInvocationContextParameter);

			if (IsAsync)
				SignaturePriority++;
			if (ReturnsValue)
				SignaturePriority++;
			if (HasInvocationContextParameter)
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

		public bool HasInvocationContextParameter { get; }

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
			return symbol.IsAsync
				? (symbol.Name == "RunAsync")
				: (symbol.Name == "Run");
		}

		public void AppendCSharpCallString(CodeStringBuilder sb, string varInvocationContext = null)
		{
			sb.Append(Symbol.Name);
			sb.Append(HasInvocationContextParameter ? $"({varInvocationContext})" : "()");
		}

		public bool Equals(CliCommandHandlerInfo other)
		{
			return base.Equals(other);
		}
	}
}