using DotMake.CommandLine.SourceGeneration.Inputs;
using DotMake.CommandLine.SourceGeneration.Util;
using Microsoft.CodeAnalysis;

namespace DotMake.CommandLine.SourceGeneration.Outputs
{
    public class OutputBase
    {
        public OutputBase(InputBase input)
        {
            Input = input;
        }

        public InputBase Input { get; set; }

        public void ReportDiagnostics(SourceProductionContext sourceProductionContext)
        {
            foreach (var diagnostic in Input.GetAllDiagnostics())
                sourceProductionContext.ReportDiagnosticSafe(diagnostic);
        }
    }
}
