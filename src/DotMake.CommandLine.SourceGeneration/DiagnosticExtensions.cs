using Microsoft.CodeAnalysis;
using System;

namespace DotMake.CommandLine.SourceGeneration
{
	public static class DiagnosticExtensions
	{
		public static void ReportDiagnosticSafe(this SourceProductionContext sourceProductionContext, Diagnostic diagnostic)
		{
			try
			{
				sourceProductionContext.ReportDiagnostic(diagnostic);
			}
			catch (Exception exception)
			{
				var diagnosticDescriptor = DiagnosticDescriptors.Create(exception);

				sourceProductionContext.ReportDiagnostic(Diagnostic.Create(diagnosticDescriptor, Location.None));
			}
		}
	}
}
