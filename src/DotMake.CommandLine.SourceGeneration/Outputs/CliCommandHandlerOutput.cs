using DotMake.CommandLine.SourceGeneration.Inputs;
using DotMake.CommandLine.SourceGeneration.Util;

namespace DotMake.CommandLine.SourceGeneration.Outputs
{
    public class CliCommandHandlerOutput : OutputBase
    {
        public CliCommandHandlerOutput(CliCommandHandlerInput input)
            : base(input)
        {
            Input = input;
        }

        public new CliCommandHandlerInput Input { get; }

        public void AppendCSharpCallString(CodeStringBuilder sb, string varCliContext = null)
        {
            sb.Append(Input.Symbol.Name);
            sb.Append("(");
            if (Input.HasCliContextParameter)
                sb.Append(varCliContext);
            sb.Append(")");
        }
    }
}
