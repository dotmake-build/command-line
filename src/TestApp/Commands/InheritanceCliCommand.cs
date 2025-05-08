#pragma warning disable CS1591
using DotMake.CommandLine;
using System;

namespace TestApp.Commands
{
    #region InheritanceCliCommand

    // When you have repeating/common options and arguments for your commands, you can define them once in a base class and then 
    // share them by inheriting that base class in other command classes.Interfaces are also supported !

    // The property attribute and the property initializer from the most derived class in the hierarchy will be used 
    // (they will override the base ones). The command handler (Run or RunAsync) will be also inherited.
    // So in the above example, `InheritanceCliCommand` inherits options `Username`, `Password` from a base class and
    // option `Department` from an interface. Note that the property initializer for `Department` is in the derived class, 
    // so that default value will be used.

    [CliCommand]
    public class InheritanceCliCommand : CredentialCommandBase, IDepartmentCommand
    {
        public string Department { get; set; } = "Accounting";
    }

    public abstract class CredentialCommandBase
    {
        [CliOption(Description = "Username of the identity performing the command")]
        public string Username { get; set; } = "admin";

        [CliOption(Description = "Password of the identity performing the command")]
        public string Password { get; set; }

        public void Run()
        {
            Console.WriteLine($@"I am {Username}");
        }
    }

    public interface IDepartmentCommand
    {
        [CliArgument(Description = "Department of the identity performing the command (interface)")]
        string Department { get; set; }
    }

    #endregion
}
