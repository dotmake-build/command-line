using System;
using System.Collections;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Parsing;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using DotMake.CommandLine;
using TestApp.Commands;
using TestApp.Commands.CasingConvention;
using TestApp.Commands.External;
using TestApp.Commands.PrefixConvention;


//Using Cli.Run with class:
Cli.Run<RootHelpOnEmptyCliCommand>(args);
//Cli.Run<RootCliCommand>(args);
//Cli.Run<WriteFileCliCommand>(args);
//Cli.Run<ArgumentConverterCliCommand>(args);
//Cli.Run<EnumerableCliCommand>(args);
//Cli.Run<RootSnakeSlashCliCommand>(args);
//Cli.Run<RootWithNestedChildrenCliCommand>(args);
//Cli.Run<RootWithExternalChildrenCliCommand>(args);
//Cli.Run<RootAsExternalParentCliCommand>(args);
//Cli.Run<RootWithMixedChildrenCliCommand>(args);
//Cli.Run<InheritanceCliCommand>(args);
//Cli.Run<LocalizedCliCommand>(args);
//Cli.Run<HelpCliCommand>(args);
//Cli.Run<ValidationCliCommand>(args);
//Cli.Run<InvalidCliCommand>(args);
//Cli.Run<InvalidCliCommand.SubCliCommand>(args);
//Cli.Run<RecursiveOptionCliCommand>(args);
//Cli.Run<ParentCommandAccessorCliCommand>(args);
//Cli.Run<OptionBundlingCliCommand>(args);
//Cli.Run<GetCompletionsCliCommand>(args);
//Cli.Run<DefaultValuesCliCommand>(args);
//Cli.Run<ShortFormCliCommand>(args);
//Cli.Run<DirectiveCliCommand>(args);
//Cli.Run<OrderedCliCommand>(args);

//Using Cli.RunAsync:
//await Cli.RunAsync<RootWithNestedChildrenCliCommand>(args);
//await Cli.RunAsync<RunAsyncCliCommand>(args);
//await Cli.RunAsync<RunAsyncWithReturnCliCommand>(args);

//Using themes:
//Cli.Run<RootCliCommand>(args, new CliSettings { Theme = CliTheme.Red });
//Cli.Run<RootCliCommand>(args, new CliSettings { Theme = CliTheme.DarkRed });
//Cli.Run<RootCliCommand>(args, new CliSettings { Theme = CliTheme.Green });
//Cli.Run<RootCliCommand>(args, new CliSettings { Theme = CliTheme.DarkGreen });
//Cli.Run<RootCliCommand>(args, new CliSettings { Theme = CliTheme.Blue });
//Cli.Run<RootCliCommand>(args, new CliSettings { Theme = CliTheme.DarkBlue });
/*
Cli.Run<RootCliCommand>(args, new CliSettings
{
    Theme = new CliTheme(CliTheme.Default)
    {
        HeadingCasing = CliNameCasingConvention.UpperCase,
        HeadingNoColon = true
    }
});
*/

//Using Cli.Run with delegate:
/*
Cli.Run(([CliArgument] string arg1, bool opt1) =>
{
    Console.WriteLine($@"Value for {nameof(arg1)} parameter is '{arg1}'");
    Console.WriteLine($@"Value for {nameof(opt1)} parameter is '{opt1}'");
});
*/
/*
Cli.Run(([CliArgument] string arg1, bool opt1) =>
{
    Console.WriteLine($@"Value for {nameof(arg1)} parameter is '{arg1}'");
    Console.WriteLine($@"Value for {nameof(opt1)} parameter is '{opt1}'");
    return Task.CompletedTask;
});
*/

//Misc:
//Cli.Run<GlobalNamespaceCliCommand>(args);
//Cli.Run<NullableReferenceCommand>(args);
//Cli.Run<PartialCliCommand>(args);
//Cli.Run<CamelCaseCliCommand>(args);
//Cli.Run<NoCaseCliCommand>(args);
//Cli.Run<SnakeCaseCliCommand>(args);
//Cli.Run<UpperCaseCliCommand>(args);
//Cli.Run<SingleHyphenCliCommand>(args);
//Cli.Run<ForwardSlashCliCommand>(args);

//Using the default exception handler which prints the exception in red color to console:
//Cli.Run<RootCliCommand>(args, new CliSettings { EnableDefaultExceptionHandler = true });

//Using custom exception handling::
/*
try
{
    Cli.Run<RootCliCommand>(args);
}
catch (Exception e)
{
    Console.WriteLine(@"Exception in main: {0}", e.Message);
}
*/

//Using Cli.Parse:
/*
var result = Cli.Parse<RootCliCommand>(args);
if (result.ParseResult.Errors.Count > 0)
{
    foreach (var error in result.ParseResult.Errors)
        Console.WriteLine(error);
}
else
{
    var rootCliCommand = result.Bind<RootCliCommand>();
    Console.WriteLine($@"Value for {nameof(rootCliCommand.Option1)} property is '{rootCliCommand.Option1}'");
    Console.WriteLine($@"Value for {nameof(rootCliCommand.Argument1)} property is '{rootCliCommand.Argument1}'");
}
*/

//Using CliParser:
/*
var parser = Cli.GetParser<RootCliCommand>(new CliSettings());
var result = parser.Parse(args);
parser.Run(args);
*/

if (!Debugger.IsAttached)
{
    Console.WriteLine(@"Press any key to exit...");
    Console.ReadKey(true);
}
