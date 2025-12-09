using System.CommandLine;
using System.Threading;
using System.Threading.Tasks;

namespace DotMake.CommandLine
{
    /// <summary>
    /// Describes the results of parsing a command line input based on a specific parser configuration
    /// and provides methods for binding the result to definition classes,
    /// and also for running the handler for the called command.
    /// </summary>
    public class CliRunnableResult : CliResult
    {
        private readonly CliParser parser;

        internal CliRunnableResult(CliParser parser, ParseResult parseResult)
            : base(parser.BindingContext, parseResult)
        {
            this.parser = parser;
        }

        /// <summary>
        /// Runs the handler for the called command.
        /// </summary>
        /// <returns><inheritdoc cref="CliParser.Run(string[])" path="/returns/node()" /></returns>
        public int Run()
        {
            using (new CliSession(parser.Settings))
                return ParseResult.Invoke(parser.InvocationConfiguration);
        }

        /// <summary>
        /// Runs the handler asynchronously for the called command.
        /// </summary>
        /// <param name="cancellationToken"><inheritdoc cref="CliParser.RunAsync(string[], CancellationToken)" path="/param[@name='cancellationToken']/node()" /></param>
        /// <returns><inheritdoc cref="CliParser.Run(string[])" path="/returns/node()" /></returns>
        public async Task<int> RunAsync(CancellationToken cancellationToken = default)
        {
            using (new CliSession(parser.Settings))
                return await ParseResult.InvokeAsync(parser.InvocationConfiguration, cancellationToken);
        }
    }
}
