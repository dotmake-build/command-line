using System;
using DotMake.CommandLine;

namespace TestApp.Commands
{
	[DotMakeCliCommand(
		Description = "A root cli command with external children and one nested child and testing settings inheritance",
		Aliases = new[] { "rootCmdAlias" }
	)]
	public class RootWithExternalChildrenCliCommand
	{
		[DotMakeCliOption(Description = "Description for Option1", Aliases = new[] { "opt1Alias" })]
		public string Option1 { get; set; } = "DefaultForOption1";

		[DotMakeCliOption(
			Description = "Description for Option2", 
			Aliases = new[] { "globalOpt2Alias" }, 
			Global = true,
			AllowedValues = new []{ "value1", "value2", "value3" }
		)]
		public string Option2 { get; set; } = "DefaultForOption1";

		[DotMakeCliArgument(Description = "Description for Argument1")]
		public string Argument1 { get; set; } = "DefaultForArgument1";

		public int Run()
		{
			Console.WriteLine($@"Handler for '{GetType().FullName}' is run:");
			Console.WriteLine($@"Value for {nameof(Option1)} property is '{Option1}'");
			Console.WriteLine($@"Value for {nameof(Argument1)} property is '{Argument1}'");
			Console.WriteLine();

			return 0;
		}

		[DotMakeCliCommand(
			Description = "A nested level 1 sub-command with custom settings, throws test exception",
			NameCasingConvention = DotMakeCliCasingConvention.SnakeCase,
			NamePrefixConvention = DotMakeCliPrefixConvention.ForwardSlash,
			ShortFormPrefixConvention = DotMakeCliPrefixConvention.ForwardSlash
		)]
		public class Level1SubCliCommand
		{
			[DotMakeCliOption(Description = "Description for Option1")]
			public string Option1 { get; set; } = "DefaultForOption1";

			[DotMakeCliArgument(Description = "Description for Argument1")]
			public string Argument1 { get; set; } = "DefaultForArgument1";

			public void Run()
			{
				Console.WriteLine($@"Handler for '{GetType().FullName}' is run:");
				throw new Exception("This is a test exception");
			}
		}
	}
}