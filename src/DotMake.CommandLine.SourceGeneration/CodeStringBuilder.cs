using System;
using System.Text;

namespace DotMake.CommandLine.SourceGeneration
{
	public class CodeStringBuilder
	{
		private readonly StringBuilder sb = new StringBuilder();

		private int IndentLevel { get; set; }

		public void Append(string value) => sb.Append(value);

		public void AppendIndent() => sb.Append(new string('\t', IndentLevel));
		
		public void AppendLine(string line) => sb.Append(new string('\t', IndentLevel)).AppendLine(line);
		
		public void AppendLine() => sb.AppendLine();
		
		public IDisposable BeginBlock(string line = null, string afterBlock = null)
		{
			if (line != null)
				AppendLine(line);

			sb.Append(new string('\t', IndentLevel)).AppendLine("{");
			IndentLevel += 1;
			return new BlockTracker(this, afterBlock);
		}

		public void EndBlock(string afterBlock = null)
		{
			IndentLevel -= 1;
			sb.Append(new string('\t', IndentLevel)).AppendLine(string.IsNullOrEmpty(afterBlock) ? "}" : "}" + afterBlock);
		}

		public void StartLine() => sb.Append(new string('\t', IndentLevel));

		public void EndLine() => sb.AppendLine();

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
				parent.EndBlock(afterBlock);
			}
		}
	}
}
