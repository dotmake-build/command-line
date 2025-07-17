#pragma warning disable CS1591
using System;
using System.Threading.Tasks;
using DotMake.CommandLine;
using TestApp.Commands;

namespace TestApp
{
    // Example code that is referenced in xml comments (xmldocs) and rendered in generated API Docs (html site).
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
            //the exceptions to pass through. However if you wish, you can easily use the default exception handler
            //by passing a `CliSettings` instance like below. Default exception handler prints the exception in red color to console:
            Cli.Run<RootCliCommand>(args, new CliSettings { EnableDefaultExceptionHandler = true });

            #endregion
        }


        public void CliParse(string[] args)
        {
            #region CliParse

            //If you need to simply parse the command-line arguments without invocation, use this:
            var result = Cli.Parse<RootCliCommand>(args);
            var rootCliCommand = result.Bind<RootCliCommand>();

            #endregion
        }

        public void CliParseString()
        {
            #region CliParseString

            //If you need to simply parse the command-line string without invocation, use this:
            var result = Cli.Parse<RootCliCommand>("NewValueForArgument1 --option-1 NewValueForOption1");
            var rootCliCommand = result.Bind<RootCliCommand>();

            #endregion
        }

        public void CliParseWithResult(string[] args)
        {
            #region CliParseWithResult

            //If you need to examine the parse result, such as errors:
            var result = Cli.Parse<RootCliCommand>(args);
            if (result.ParseResult.Errors.Count > 0)
            {

            }

            #endregion
        }

        public void CliParseStringWithResult()
        {
            #region CliParseStringWithResult

            //If you need to examine the parse result, such as errors:
            var result = Cli.Parse<RootCliCommand>("NewValueForArgument1 --option-1 NewValueForOption1");
            if (result.ParseResult.Errors.Count > 0)
            {

            }

            #endregion
        }


        public void CliRunDelegate()
        {
            #region CliRunDelegate

            //Delegate-based model
            //In Program.cs, add this simple code:
            Cli.Run(([CliArgument] string argument1, bool option1) =>
            {
                Console.WriteLine($@"Value for {nameof(argument1)} parameter is '{argument1}'");
                Console.WriteLine($@"Value for {nameof(option1)} parameter is '{option1}'");
            });

            //Or:
            Cli.Run(Method);

            void Method([CliArgument] string argument2, bool option2)
            {
                Console.WriteLine($@"Value for {nameof(argument2)} parameter is '{argument2}'");
                Console.WriteLine($@"Value for {nameof(option2)} parameter is '{option2}'");
            }

            #endregion
        }

        public int CliRunDelegateWithReturn()
        {
            #region CliRunDelegateWithReturn

            //In Program.cs, add this simple code for returning exit code:
            return Cli.Run(([CliArgument] string argument1, bool option1) =>
            {
                Console.WriteLine($@"Value for {nameof(argument1)} parameter is '{argument1}'");
                Console.WriteLine($@"Value for {nameof(option1)} parameter is '{option1}'");

                return 0;
            });

            #endregion
        }

        public void CliRunAsyncDelegate()
        {
            #region CliRunAsyncDelegate

            //In Program.cs, to go async, add this simple code:
            Cli.Run(async ([CliArgument] string argument1, bool option1) =>
            {
                Console.WriteLine($@"Value for {nameof(argument1)} parameter is '{argument1}'");
                Console.WriteLine($@"Value for {nameof(option1)} parameter is '{option1}'");

                await Task.Delay(1000);
            });

            //Or:
            Cli.Run(Method);

            async Task Method([CliArgument] string argument2, bool option2)
            {
                Console.WriteLine($@"Value for {nameof(argument2)} parameter is '{argument2}'");
                Console.WriteLine($@"Value for {nameof(option2)} parameter is '{option2}'");

                await Task.Delay(1000);
            }

            #endregion
        }

        public int CliRunAsyncDelegateWithReturn()
        {
            #region CliRunAsyncDelegateWithReturn

            //In Program.cs, to go async, add this simple code for returning exit code:
            return Cli.Run(async ([CliArgument] string argument1, bool option1) =>
            {
                Console.WriteLine($@"Value for {nameof(argument1)} parameter is '{argument1}'");
                Console.WriteLine($@"Value for {nameof(option1)} parameter is '{option1}'");

                await Task.Delay(1000);
                return 0;
            });

            #endregion
        }
    }
}
