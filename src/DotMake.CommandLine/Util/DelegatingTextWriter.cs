using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
#if NETCOREAPP2_1_OR_GREATER
using System.Threading;
#endif
#pragma warning disable CS8632 // The annotation for nullable reference types should only be used in code within a '#nullable' annotations context.
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member

namespace DotMake.CommandLine.Util
{
    /// <summary>Delegates all write operations to an underlying <see cref="TextWriter"/>.</summary>
    /// <remarks>This class does not own the underlying writer unless explicitly configured.</remarks>
    /// <inheritdoc/>
    public class DelegatingTextWriter : TextWriter
    {
        protected readonly TextWriter Inner;
        private readonly bool leaveOpen;

        /// <summary>Initializes a new instance of the <see cref="DelegatingTextWriter" /> class.</summary>
        /// <param name="inner">
        /// The underlying <see cref="TextWriter"/> to which output is delegated.
        /// </param>
        /// <param name="leaveOpen">
        /// Whether to leave the underlying <see cref="TextWriter"/> open when this instance is disposed.
        /// </param>
        public DelegatingTextWriter(TextWriter inner, bool leaveOpen = true)
        {
            Inner = inner ?? throw new ArgumentNullException(nameof(inner));
            this.leaveOpen = leaveOpen;
        }

        public override Encoding Encoding => Inner.Encoding;

        public override IFormatProvider FormatProvider => Inner.FormatProvider;

        public override string NewLine
        {
            get => Inner.NewLine;
            set => Inner.NewLine = value;
        }

        /*
            The real “hub” methods are these:
            In TextWriter, the actual core pipeline is:

                1. String
                    Write(string value)
                2. Char
                    Write(char value)
                3. Char buffer
                    Write(char[] buffer, int index, int count)
                4. String-based formatting
                    Write(string format, object arg0, ...)

            So where does object overloads fit?

                This method:

                    Write(object value)

                is special because:

                    object → polymorphic formatting entry point

                It handles:

                    IFormattable
                    custom ToString(IFormatProvider)
                    culture-aware formatting

                So it is not just another primitive (int, float) overload.

                You override Write(object) but not int/float etc because:

                object is a polymorphic formatting gateway
                primitives are already safely reduced to string internally
                overriding primitives is redundant and adds no benefit in a delegating writer
                TextWriter is designed so that string/char are the real core abstraction

            Why ReadOnlyMemory<char> overloads matters for async methods?

                This is the key:

                    WriteAsync(ReadOnlyMemory<char>, CancellationToken)

                enables:

                    zero-copy writes
                    true async streaming
                    high-performance pipelines (Kestrel, logging, etc.)

                Without it:

                    memory must be copied into char[] or string
                    extra allocations occur per write

            Final takeaway

                object overload → semantic correctness (formatting behavior)
                ReadOnlyMemory<char> → performance correctness (zero allocations)
        */

        // -------------------------------------------------
        // Sync forwarding (pure delegation)
        // -------------------------------------------------

        public override void Flush() => Inner.Flush();

        public override void Write(char value) => Inner.Write(value);

        public override void Write(string? value) => Inner.Write(value);

        public override void Write(char[] buffer, int index, int count)
            => Inner.Write(buffer, index, count);

        public override void WriteLine() => Inner.WriteLine();

        public override void WriteLine(string? value) => Inner.WriteLine(value);

        public override void WriteLine(char value) => Inner.WriteLine(value);

        public override void WriteLine(char[] buffer, int index, int count)
            => Inner.WriteLine(buffer, index, count);

        public override void Write(object? value) => Inner.Write(value);

        public override void WriteLine(object? value) => Inner.WriteLine(value);

        // -------------------------------------------------
        // Async (REAL async forwarding)
        // -------------------------------------------------

        public override Task WriteAsync(char value)
            => Inner.WriteAsync(value);

        public override Task WriteAsync(string? value)
            => Inner.WriteAsync(value);

        public override Task WriteAsync(char[] buffer, int index, int count)
            => Inner.WriteAsync(buffer, index, count);

        public override Task WriteLineAsync()
            => Inner.WriteLineAsync();

        public override Task WriteLineAsync(char value)
            => Inner.WriteLineAsync(value);

        public override Task WriteLineAsync(string? value)
            => Inner.WriteLineAsync(value);

        public override Task WriteLineAsync(char[] buffer, int index, int count)
            => Inner.WriteLineAsync(buffer, index, count);

        public override Task FlushAsync()
            => Inner.FlushAsync();

#if NETCOREAPP2_1_OR_GREATER

        public override Task WriteAsync(ReadOnlyMemory<char> buffer, CancellationToken cancellationToken = default)
            => Inner.WriteAsync(buffer, cancellationToken);

        public override Task WriteLineAsync(ReadOnlyMemory<char> buffer, CancellationToken cancellationToken = default)
            => Inner.WriteLineAsync(buffer, cancellationToken);

#endif

        // -------------------------------------------------
        // Dispose
        // -------------------------------------------------

        protected override void Dispose(bool disposing)
        {
            if (disposing && !leaveOpen)
                Inner.Dispose();

            base.Dispose(disposing);
        }

#if NETCOREAPP3_0_OR_GREATER
        public override ValueTask DisposeAsync()
        {
            if (!leaveOpen)
                return Inner.DisposeAsync();

            return base.DisposeAsync();
        }
#endif
    }
}
