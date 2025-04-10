using Microsoft.CodeAnalysis;
using DotMake.CommandLine.SourceGeneration.Inputs;
using DotMake.CommandLine.SourceGeneration.Util;

namespace DotMake.CommandLine.SourceGeneration.Outputs
{
    public class CliArgumentParserOutput : OutputBase
    {
        public CliArgumentParserOutput(CliArgumentParserInput input)
            : base(input)
        {
            Input = input;
        }

        public new CliArgumentParserInput Input { get; set; }

        public void AppendCSharpCallString(CodeStringBuilder sb, string varCustomParser, string afterBlock)
        {
            if (Input.ItemType != null)
            {
                using (sb.AppendParamsBlockStart($"{varCustomParser} = GetArgumentParser<{Input.Type.ToReferenceString()}, {Input.ItemType.ToReferenceString()}>", afterBlock))
                {
                    if (Input.Converter == null)
                        sb.AppendLine("null,");
                    else if (Input.Converter.ContainingType.SpecialType == SpecialType.System_Array)
                        sb.AppendLine($"array => ({Input.ItemType.ToReferenceString()}[])array,");
                    else if (Input.Converter.Name == ".ctor")
                    {
                        if (Input.Converter.Parameters[0].Type.OriginalDefinition.SpecialType == SpecialType.System_Collections_Generic_IList_T)
                            sb.AppendLine($"array => new {Input.Converter.ContainingType.ToReferenceString()}(System.Linq.Enumerable.ToList(({Input.ItemType.ToReferenceString()}[])array)),");
                        else
                            sb.AppendLine($"array => new {Input.Converter.ContainingType.ToReferenceString()}(({Input.ItemType.ToReferenceString()}[])array),");
                    }
                    else
                        sb.AppendLine($"array => {Input.Converter.ToReferenceString()}(({Input.ItemType.ToReferenceString()}[])array),");

                    if (Input.ItemConverter == null)
                        sb.AppendLine("null");
                    else if (Input.ItemConverter.Name == ".ctor")
                        sb.AppendLine($"item => new {Input.ItemConverter.ContainingType.ToReferenceString()}(item)");
                    else
                        sb.AppendLine($"item => {Input.ItemConverter.ToReferenceString()}(item)");
                }
            }
            else
            {
                //Even if argument type does not need a converter, use a ParseArgument method,
                //so that our custom converter is used for supporting all collection compatible types.
                using (sb.AppendParamsBlockStart($"{varCustomParser} = GetArgumentParser<{Input.Type.ToReferenceString()}>", afterBlock))
                {
                    if (Input.Converter == null)
                        sb.AppendLine("null");
                    else if (Input.Converter.Name == ".ctor")
                        sb.AppendLine($"input => new {Input.Converter.ContainingType.ToReferenceString()}(input)");
                    else
                        sb.AppendLine($"input => {Input.Converter.ToReferenceString()}(input)");
                }
            }
        }
    }
}
