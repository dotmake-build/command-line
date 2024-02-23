namespace DotMake.CommandLine.Shared.Exceptions;

public class InvalidParentClassException : System.Exception
{
    public InvalidParentClassException() : base("The 'CliCommand' passed to the 'Run<T>' method is inside of a class that is invalid. Please ensure the parent class is also a 'CliCommand'.");
    public InvalidParentClassException(string message) : base(message);
}