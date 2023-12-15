using System;
using DotMake.CommandLine;

namespace TestApp.Commands.External
{
	[CliCommand(
		Name = "Level2External",
		Description = "An external level 2 sub-command",
		Parent = typeof(RootWithExternalChildrenCliCommand.Level1SubCliCommand),
		NameCasingConvention = CliNameCasingConvention.SnakeCase,
		NamePrefixConvention = CliNamePrefixConvention.ForwardSlash,
		ShortFormPrefixConvention = CliNamePrefixConvention.ForwardSlash,
		Aliases = new[] { "external2Alias" }
	)]
	public class ExternalLevel2SubCliCommand
	{
		[CliOption(Description = "Description for Option1")]
		public string Option1 { get; set; } = "DefaultForOption1";

		[CliArgument(Description = "Description for Argument1")]
		public string Argument1 { get; set; } = "DefaultForArgument1";

		public void Run()
		{
			Console.WriteLine($@"Handler for '{GetType().FullName}' is run:");
			Console.WriteLine($@"Value for {nameof(Option1)} property is '{Option1}'");
			Console.WriteLine($@"Value for {nameof(Argument1)} property is '{Argument1}'");
			Console.WriteLine();
		}

		[CliCommand(Description = "A nested level 3 sub-command")]
		public class Level3SubCliCommand
		{
			[CliOption(Description = "Description for Option1")]
			public string Option1 { get; set; } = "DefaultForOption1";

			[CliArgument(Description = "Description for Argument1")]
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
}