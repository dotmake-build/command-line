using System;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using DotMake.CommandLine;
using TestApp.Commands;
using TestApp.Commands.PrefixConvention;


try
{
	//Using Cli.Run:
	Cli.Run<RootWithExternalChildrenCliCommand>(args);
	//Cli.Run<RootWithNestedChildrenCliCommand>(args);
	//Cli.Run<RootCliCommand>(args);
	//Cli.Run<ForwardSlashCliCommand>(args);

	//Using Cli.RunAsync:
	//await Cli.RunAsync<RootWithChildrenCliCommand>(args);

	//Using configureBuilder to use an exception handler:
	/*
	Cli.Run<RootWithChildrenCliCommand>(args, builder => 
		builder.UseExceptionHandler((e, context) => Console.WriteLine(@"Exception in command handler: {0}", e.Message))
	);
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
