namespace DotMake.CommandLine;

public class InvalidClassException : System.Exception
{
    public InvalidClassException() : base("An invalid class was passed to the 'Run<T>' method. Please ensure the class is a 'CliCommand'.")
    public InvalidClassException(string message) : base(message) { }
}