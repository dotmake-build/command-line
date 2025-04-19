using System;
using System.Text;

namespace DotMake.CommandLine.SourceGeneration.Util
{
    public class CodeStringBuilder
    {
        private const int IndentSize = 4;
        private const char Space = ' ';

        private readonly StringBuilder sb = new();

        private int IndentLevel { get; set; }

        public void Append(string value) => sb.Append(value);

        public void AppendIndent()
        {
            for (var i = 0; i < IndentSize; i++)
                sb.Append(Space);
        }

        private void AppendIndentForLevel()
        {
            for (var i = 0; i < IndentLevel * IndentSize; i++)
                sb.Append(Space);
        }

        public void AppendLine(string line)
        {
            AppendIndentForLevel();
            sb.AppendLine(line);
        }

        public void AppendLine() => sb.AppendLine();

        public void AppendLineStart() => AppendIndentForLevel();

        public void AppendLineEnd() => sb.AppendLine();

        public IDisposable AppendBlockStart(string line, string startBlock, string endBlock, string afterBlock)
        {
            if (line != null)
                AppendLine(line);

            if (startBlock != null)
                AppendLine(startBlock);

            IndentLevel++;

            return new BlockTracker(this, endBlock, afterBlock);
        }

        public IDisposable AppendBlockStart(string line = null, string afterBlock = null)
        {
            return AppendBlockStart(line, "{", "}", afterBlock);
        }

        public IDisposable AppendParamsBlockStart(string line = null, string afterBlock = null)
        {
            return AppendBlockStart(line, "(", ")", afterBlock);
        }

        public void AppendBlockEnd(string endBlock, string afterBlock = null)
        {
            IndentLevel--;

            if (endBlock != null)
                AppendLine((afterBlock == null) ? endBlock : endBlock + afterBlock);
        }

        public override string ToString() => sb.ToString();

        private class BlockTracker : IDisposable
        {
            private readonly CodeStringBuilder parent;
            private readonly string endBlock;
            private readonly string afterBlock;

            public BlockTracker(CodeStringBuilder parent, string endBlock, string afterBlock)
            {
                this.parent = parent;
                this.endBlock = endBlock;
                this.afterBlock = afterBlock;
            }

            public void Dispose()
            {
                parent.AppendBlockEnd(endBlock, afterBlock);
            }
        }
    }
}
