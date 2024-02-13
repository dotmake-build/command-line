#pragma warning disable CS1591
//#define ErrorsOrWarnings
using DotMake.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;

namespace TestApp.Commands
{
#if ErrorsOrWarnings

	[CliCommand(Description = "Warning: Non public/internal class will be ignored")]
	private class NonPublicCliCommand
	{

	}

	[CliCommand(Description = "Warning: Static class will be ignored")]
	public static class StaticCliCommand
	{

	}

	[CliCommand(Description = "Error: Class cannot be abstract")]
	public abstract class AbstractCliCommand
	{

	}

	[CliCommand(Description = "Error: Class cannot be generic")]
	public  class GenericCliCommand<T>
	{

	}

	[CliCommand(Description = "Error: Class must have a public constructor")]
	public class NoPublicConstructorCliCommand
	{
		private NoPublicConstructorCliCommand()
		{

		}
	}

	[CliCommand(Description = "Error: Class must have a default (parameter-less) constructor")]
	public class NoDefaultConstructorCliCommand
	{
		public NoDefaultConstructorCliCommand(string value)
		{

		}
	}

	[CliCommand(
		Description = "Error: Circular parent dependency involving the classes",
		Parent = typeof(CircularSelfCliCommand)
	)]
	public class CircularSelfCliCommand
	{

	}

	[CliCommand(
		Description = "Error: Circular parent dependency involving the classes",
		Parent = typeof(Circular2CliCommand)
	)]
	public class Circular1CliCommand
	{

	}

	[CliCommand(
		Description = "Error: Circular parent dependency involving the classes",
		Parent = typeof(Circular1CliCommand)
	)]
	public class Circular2CliCommand
	{

	}

	public class NoAttributeCliCommand
	{

	}

	[CliCommand(
		Description = "Error: Parent class does not have the attribute",
		Parent = typeof(NoAttributeCliCommand)
	)]
	public class NoAttributeChildCliCommand
	{

	}

	[CliCommand(Description = "Warning: No Run or RunAsync method, handler will be ignored")]
	public class NoHandlerCliCommand
	{

	}

	[CliCommand(Description = "Warning: Non public/internal, static and generic Run or RunAsync method, handler will be ignored")]
	public class NoPublicNonStaticHandlerCliCommand
	{
		private void Run2()
		{

		}

		public static int Run2(CliContext context)
		{
			return 0;
		}

		private async Task<int> RunAsync2()
		{
			await Task.Delay(1000);
			return 0;
		}

		public async Task RunAsync<T>()
		{
			await Task.Delay(1000);
		}
	}

	[CliCommand(Description = "Warning: Non public/internal properties will be ignored")]
	public class NonPublicPropsCliCommand
	{
		[CliOption]
		private string Option1 { get; set; }

		[CliArgument]
		protected string Argument1 { get; set; }

		public void Run()
		{

		}
	}

	[CliCommand(Description = "Error: Properties with non public/internal getter/setter will be ignored")]
	public class NonPublicGetterOrSetterCliCommand
	{
		[CliOption]
		public string Option1 { get; }

		[CliOption]
		public string Option2 { get; private set; }

		[CliArgument]
		public string Argument1 { get; }

		[CliArgument]
		public string Argument2 { get; protected set; }

		public void Run()
		{

		}
	}

#endif
}
