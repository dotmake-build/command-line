using System;
using System.Text;

namespace DotMake.CommandLine.SourceGeneration
{
    public class CodeStringBuilder
    {
        private const int IndentSize = 4;
        private const char Space = ' ';

        private readonly StringBuilder sb = new StringBuilder();

        private int IndentLevel { get; set; }

        public void Append(string value) => sb.Append(value);

        private void AppendIndent()
        {
            for (var i = 0; i < (IndentLevel * IndentSize); i++)
                sb.Append(Space);
        }

        public void AppendLine(string line)
        {
            AppendIndent();
            sb.AppendLine(line);
        }

        public void AppendLine() => sb.AppendLine();

        public void AppendLineStart() => AppendIndent();

        public void AppendLineEnd() => sb.AppendLine();

        public IDisposable AppendBlockStart(string line = null, string afterBlock = null)
        {
            if (line != null)
                AppendLine(line);

            AppendLine("{");

            IndentLevel++;

            return new BlockTracker(this, afterBlock);
        }

        public void AppendBlockEnd(string afterBlock = null)
        {
            IndentLevel--;

            AppendLine(string.IsNullOrEmpty(afterBlock) ? "}" : "}" + afterBlock);
        }

        public override string ToString() => sb.ToString();

        private class BlockTracker : IDisposable
        {
            private readonly CodeStringBuilder parent;
            private readonly string afterBlock;

            public BlockTracker(CodeStringBuilder parent, string afterBlock)
            {
                this.parent = parent;
                this.afterBlock = afterBlock;
            }

            public void Dispose()
            {
                parent.AppendBlockEnd(afterBlock);
            }
        }
    }
}
