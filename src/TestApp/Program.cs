using System;
using System.CommandLine.Builder;
using System.CommandLine.Parsing;
using DotMake.CommandLine;
using TestApp.Commands;
using TestApp.Commands.PrefixConvention;


try
{
	//Using DotMakeCli.Run:
	DotMakeCli.Run<RootWithExternalChildrenCliCommand>(args);
	//DotMakeCli.Run<RootWithNestedChildrenCliCommand>(args);
	//DotMakeCli.Run<RootCliCommand>(args);
	//DotMakeCli.Run<ForwardSlashCliCommand>(args);

	//Using DotMakeCli.RunAsync:
	//await DotMakeCli.RunAsync<RootWithChildrenCliCommand>(args);

	//Using configureBuilder to use an exception handler:
	/*
	DotMakeCli.Run<RootWithChildrenCliCommand>(args, builder => 
		builder.UseExceptionHandler((e, context) => Console.WriteLine(@"Exception in command handler: {0}", e.Message))
	);
	*/

	//Using DotMakeCli.GetBuilder:
	/*
	var parser = DotMakeCli.GetBuilder<RootWithChildrenCliCommand>()
		.UseExceptionHandler((e, context) => Console.WriteLine(@"Exception in command handler: {0}", e.Message))
		.Build();

	parser.Invoke(args);
	*/
}
catch (Exception e)
{
	Console.WriteLine(@"Exception in main: {0}", e.Message);
}
