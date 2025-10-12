#pragma warning disable CS1591

using DotMake.CommandLine;

//Generated code should not fail to compile even if user uses some crazy namespaces

namespace TestApp.Commands.TestApp
{
    [CliCommand]
    public class TestAppCliCommand
    {
        [CliArgument(Arity = CliArgumentArity.ZeroOrMore)]
        public string Arg { get; set; }
    }
}

namespace TestApp.Commands.System
{
    [CliCommand]
    public class SystemCliCommand
    {

    }
}

namespace TestApp.Commands.DotMake
{
    [CliCommand]
    public class DotMakeCliCommand
    {

    }
}
