using System;
using System.CommandLine.Builder;
using System.Threading.Tasks;

namespace DotMake.CommandLine.Examples
{
    public class CliExamples
    {
        public void CliRun(string[] args)
        {
            #region CliRun

            //In Program.cs, add this single line:
            Cli.Run<RootCliCommand>(args);

            #endregion
        }

        public void CliRunString()
        {
            #region CliRunString

            //In Program.cs, add this single line:
            Cli.Run<RootCliCommand>("NewValueForArgument1 --option-1 NewValueForOption1");

            #endregion
        }

        public int CliRunWithReturn(string[] args)
        {
            #region CliRunWithReturn

            //In Program.cs, add this single line for returning exit code:
            return Cli.Run<RootCliCommand>(args);

            #endregion
        }

        public int CliRunStringWithReturn()
        {
            #region CliRunStringWithReturn

            //In Program.cs, add this single line for returning exit code:
            return Cli.Run<RootCliCommand>("NewValueForArgument1 --option-1 NewValueForOption1");

            #endregion
        }


        public async void CliRunAsync(string[] args)
        {
            #region CliRunAsync

            //In Program.cs, to go async, add this single line:
            await Cli.RunAsync<RootCliCommand>(args);

            #endregion
        }

        public async void CliRunAsyncString()
        {
            #region CliRunAsyncString

            //In Program.cs, to go async, add this single line:
            await Cli.RunAsync<RootCliCommand>("NewValueForArgument1 --option-1 NewValueForOption1");

            #endregion
        }

        public async Task<int> CliRunAsyncWithReturn(string[] args)
        {
            #region CliRunAsyncWithReturn

            //In Program.cs, to go async, add this single line for returning exit code:
            return await Cli.RunAsync<RootCliCommand>(args);

            #endregion
        }

        public async Task<int> CliRunAsyncStringWithReturn()
        {
            #region CliRunAsyncStringWithReturn

            //In Program.cs, to go async, add this single line for returning exit code:
            return await Cli.RunAsync<RootCliCommand>("NewValueForArgument1 --option-1 NewValueForOption1");

            #endregion
        }

        public void CliRunExceptions(string[] args)
        {
            #region CliRunExceptions

            //To handle exceptions, you just use a try-catch block:
            try
            {
                Cli.Run<RootCliCommand>(args);
            }
            catch (Exception e)
            {
                Console.WriteLine(@"Exception in main: {0}", e.Message);
            }

            //System.CommandLine, by default overtakes your exceptions that are thrown in command handlers
            //(even if you don't set an exception handler explicitly) but DotMake.CommandLine, by default allows
            //the exceptions to pass through. However if you wish, you can easily use an exception handler by
            //using `configureBuilder` delegate parameter like this:
            Cli.Run<RootCliCommand>(args, builder =>
                builder.UseExceptionHandler((e, context) => Console.WriteLine(@"Exception in command handler: {0}", e.Message))
            );

            #endregion
        }
        public void CliParse(string[] args)
        {
            #region CliParse

            //If you need to simply parse the command-line arguments without invocation, use this:
            var rootCliCommand = Cli.Parse<RootCliCommand>(args);

            #endregion
        }

        public void CliParseString()
        {
            #region CliParseString

            //If you need to simply parse the command-line arguments without invocation, use this:
            var rootCliCommand = Cli.Parse<RootCliCommand>("NewValueForArgument1 --option-1 NewValueForOption1");

            #endregion
        }

        public void CliParseWithResult(string[] args)
        {
            #region CliParseWithResult

            //If you need to examine the parse result, such as errors:
            var rootCliCommand = Cli.Parse<RootCliCommand>(args, out var parseResult);
            if (parseResult.Errors.Count > 0)
            {

            }

            #endregion
        }
        public void CliParseStringWithResult()
        {
            #region CliParseStringWithResult

            //If you need to examine the parse result, such as errors:
            var rootCliCommand = Cli.Parse<RootCliCommand>("NewValueForArgument1 --option-1 NewValueForOption1", out var parseResult);
            if (parseResult.Errors.Count > 0)
            {

            }

            #endregion
        }
    }
}
