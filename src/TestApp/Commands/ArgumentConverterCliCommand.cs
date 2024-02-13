#pragma warning disable CS1591
using System;
using System.Collections.Generic;
using DotMake.CommandLine;

namespace TestApp.Commands
{
    #region ArgumentConverterCliCommand

    // Any type with a public constructor or a static `Parse` method with a string parameter (other parameters, if any,
    // should be optional) - These types can be bound/parsed automatically even if they are wrapped
    // with `Enumerable` or `Nullable` type.

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

        public void Run(CliContext context)
        {
            context.ShowValues();
        }
    }

    public class ClassWithConstructor
    {
        private readonly string value;

        public ClassWithConstructor(string value)
        {
            if (value == "exception")
                throw new Exception("Exception in ClassWithConstructor");

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
            if (value == "exception")
                throw new Exception("Exception in ClassWithParser");

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

    #endregion
}
