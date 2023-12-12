using System;
using DotMake.CommandLine;

namespace TestApp.Commands.CasingConvention
{
	[DotMakeCliCommand(
		Description = "A cli command with snake_case convention",
		NameCasingConvention = DotMakeCliCasingConvention.SnakeCase
	)]
	public class SnakeCaseCliCommand
	{
		[DotMakeCliOption(Description = "Description for Option1")]
		public string Option1 { get; set; } = "DefaultForOption1";

		[DotMakeCliArgument(Description = "Description for Argument1")]
		public string Argument1 { get; set; } = "DefaultForArgument1";

		public void Run()
		{
			Console.WriteLine($@"Handler for '{GetType().FullName}' is run:");
			Console.WriteLine($@"Value for {nameof(Option1)} property is '{Option1}'");
			Console.WriteLine($@"Value for {nameof(Argument1)} property is '{Argument1}'");
			Console.WriteLine();
		}
	}
}