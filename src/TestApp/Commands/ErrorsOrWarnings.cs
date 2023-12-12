//#define ErrorsOrWarnings
using DotMake.CommandLine;
using System.CommandLine.Invocation;
using System.Threading.Tasks;

namespace TestApp.Commands
{
#if ErrorsOrWarnings

	[DotMakeCliCommand( Description = "Warning: Non public/internal class will be ignored")]
	private class NonPublicCliCommand
	{

	}

	[DotMakeCliCommand( Description = "Warning: Static class will be ignored")]
	public static class StaticCliCommand
	{

	}

	[DotMakeCliCommand(Description = "Error: Class cannot be abstract")]
	public abstract class AbstractCliCommand
	{

	}

	[DotMakeCliCommand(Description = "Error: Class cannot be generic")]
	public  class GenericCliCommand<T>
	{

	}

	[DotMakeCliCommand(Description = "Error: Class must have a public constructor")]
	public class NoPublicConstructorCliCommand
	{
		private NoPublicConstructorCliCommand()
		{

		}
	}

	[DotMakeCliCommand(Description = "Error: Class must have a default (parameter-less) constructor")]
	public class NoDefaultConstructorCliCommand
	{
		public NoDefaultConstructorCliCommand(string value)
		{

		}
	}

	[DotMakeCliCommand(
		Description = "Error: Circular parent dependency involving the classes",
		Parent = typeof(CircularSelfCliCommand)
	)]
	public class CircularSelfCliCommand
	{

	}

	[DotMakeCliCommand(
		Description = "Error: Circular parent dependency involving the classes",
		Parent = typeof(Circular2CliCommand)
	)]
	public class Circular1CliCommand
	{

	}

	[DotMakeCliCommand(
		Description = "Error: Circular parent dependency involving the classes",
		Parent = typeof(Circular1CliCommand)
	)]
	public class Circular2CliCommand
	{

	}

	public class NoAttributeCliCommand
	{

	}

	[DotMakeCliCommand(
		Description = "Error: Parent class does not have the attribute",
		Parent = typeof(NoAttributeCliCommand)
	)]
	public class NoAttributeChildCliCommand
	{

	}

	[DotMakeCliCommand(Description = "Warning: No Run or RunAsync method, handler will be ignored")]
	public class NoHandlerCliCommand
	{

	}

	[DotMakeCliCommand(Description = "Warning: Non public/internal, static and generic Run or RunAsync method, handler will be ignored")]
	public class NoPublicNonStaticHandlerCliCommand
	{
		private void Run2()
		{

		}

		public static int Run2(InvocationContext context)
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

	[DotMakeCliCommand(Description = "Warning: Non public/internal properties will be ignored")]
	public class NonPublicPropsCliCommand
	{
		[DotMakeCliOption]
		private string Option1 { get; set; }

		[DotMakeCliArgument]
		protected string Argument1 { get; set; }

		public void Run()
		{

		}
	}

	[DotMakeCliCommand(Description = "Error: Properties with non public/internal getter/setter will be ignored")]
	public class NonPublicGetterOrSetterCliCommand
	{
		[DotMakeCliOption]
		public string Option1 { get; }

		[DotMakeCliOption]
		public string Option2 { get; private set; }

		[DotMakeCliArgument]
		public string Argument1 { get; }

		[DotMakeCliArgument]
		public string Argument2 { get; protected set; }

		public void Run()
		{

		}
	}

#endif
}