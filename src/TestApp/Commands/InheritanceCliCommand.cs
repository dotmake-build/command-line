#pragma warning disable CS1591
using DotMake.CommandLine;
using System;

namespace TestApp.Commands
{
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
        [CliOption(Description = "Department of the identity performing the command (interface)")]
        string Department { get; set; }
    }
}
