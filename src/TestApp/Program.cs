using System;
using System.Collections;
using System.Collections.Generic;
using System.CommandLine;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using System.ComponentModel;
using System.Threading.Tasks;
using DotMake.CommandLine;
using TestApp.Commands;
using TestApp.Commands.PrefixConvention;

try
{
    //Using Cli.Run with delegate:
    
    Cli.Run(([CliArgument]string argument1, bool option1) =>
    {
        Console.WriteLine($@"Value for {nameof(argument1)} parameter is '{argument1}'");
        Console.WriteLine($@"Value for {nameof(option1)} parameter is '{option1}'");
    });
    
    //Using Cli.Run with class:
    //Cli.Run<RootCliCommand>(args);
    //Cli.Run<RootSnakeSlashCliCommand>(args);
    //Cli.Run<ForwardSlashCliCommand>(args);
    //Cli.Run<RootWithNestedChildrenCliCommand>(args);
    //Cli.Run<RootWithExternalChildrenCliCommand>(args);
    //Cli.Run<ArgumentConverterCliCommand>(args);
    //Cli.Run<EnumerableCliCommand>(args);
    //Cli.Run<NullableReferenceCommand>(args);
    //Cli.Run<InheritanceCliCommand>(args);
    //Cli.Run<FileSystemInfoCliCommand>(args);
    //Cli.Run<LocalizedCliCommand>(args);
    //Cli.Run<HelpCliCommand>(args);

    //Using Cli.RunAsync:
    //await Cli.RunAsync<RootWithChildrenCliCommand>(args);

    //Using configureBuilder to use an exception handler:
    /*
    Cli.Run<RootWithNestedChildrenCliCommand>(args, builder =>
        builder.UseExceptionHandler((e, context) => Console.WriteLine(@"Exception in command handler: {0}", e.Message))
    );
    */

    //Using Cli.Parse:
    /*
    var rootCliCommand = Cli.Parse<RootCliCommand>(args);
    var rootCliCommand = Cli.Parse<RootCliCommand>(args, out var parseResult);
    if (parseResult.Errors.Count > 0)
    {

    }
    Console.WriteLine($@"Value for {nameof(rootCliCommand.Option1)} property is '{rootCliCommand.Option1}'");
    Console.WriteLine($@"Value for {nameof(rootCliCommand.Argument1)} property is '{rootCliCommand.Argument1}'");
    Console.WriteLine();
    */
    //Using Cli.GetBuilder:
    /*
    var parser = Cli.GetBuilder<RootWithChildrenCliCommand>()
        .UseExceptionHandler((e, context) => Console.WriteLine(@"Exception in command handler: {0}", e.Message))
        .Build();

    parser.Invoke(args);
    */
}
catch (Exception e)
{
    Console.WriteLine(@"Exception in main: {0}", e.Message);
}
