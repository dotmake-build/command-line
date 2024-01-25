#pragma warning disable CS1591
using System.Collections.Generic;
using System.CommandLine.Invocation;
using DotMake.CommandLine;

namespace TestApp.Commands
{
    [CliCommand]
    public class ArgumentConverterCliCommand
    {
        [CliOption(Required = false)]
        public ClassWithConstructor Opt { get; set; }

        [CliOption(Required = false, AllowMultipleArgumentsPerToken = true)]
        public ClassWithConstructor[] OptArray { get; set; }

        [CliOption(Required = false)]
        public CustomStruct? OptNullable { get; set; }

        [CliOption(Required = false)]
        public IEnumerable<ClassWithConstructor> OptEnumerable { get; set; }

        [CliOption(Required = false)]
        public List<ClassWithConstructor> OptList { get; set; }

        [CliOption(Required = false)]
        public CustomList<ClassWithConstructor> OptCustomList { get; set; }

        [CliArgument]
        public IEnumerable<ClassWithParser> Arg { get; set; }

        public void Run(InvocationContext context)
        {
            context.ShowValues();
        }
    }

    public class ClassWithConstructor
    {
        private readonly string value;

        public ClassWithConstructor(string value)
        {
            this.value = value;
        }

        public override string ToString()
        {
            return value;
        }
    }

    public class ClassWithParser
    {
        private string value;

        public override string ToString()
        {
            return value;
        }

        public static ClassWithParser Parse(string value)
        {
            //throw new NullReferenceException();
            var instance = new ClassWithParser();
            instance.value = value;
            return instance;
        }
    }
    
    public struct CustomStruct
    {
        private readonly string value;

        public CustomStruct(string value)
        {
            this.value = value;
        }

        public override string ToString()
        {
            return value;
        }
    }
}
