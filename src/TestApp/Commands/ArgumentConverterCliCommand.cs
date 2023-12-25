using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DotMake.CommandLine;

namespace TestApp.Commands
{
    [CliCommand]
    public class ArgumentConverterCliCommand
    {
        [CliOption]
        public ClassWithConstructor Opt { get; set; }

        [CliOption(AllowMultipleArgumentsPerToken = true)]
        public ClassWithConstructor[] OptArray { get; set; }

        [CliOption]
        public CustomStruct? OptNullable { get; set; }

        [CliOption]
        public IEnumerable<ClassWithConstructor> OptEnumerable { get; set; }

        [CliOption]
        public List<ClassWithConstructor> OptList { get; set; }

        [CliOption(AllowMultipleArgumentsPerToken = true)]
        public FileAccess[] OptEnumArray { get; set; }

        /*
        [CliOption]
        public CustomList<ClassWithConstructor> OptCustomList { get; set; }
        */

        [CliArgument]
        public IEnumerable<Sub.ClassWithParser> Arg { get; set; }

        public void Run()
        {
            Console.WriteLine($@"Handler for '{GetType().FullName}' is run:");
            
            Console.WriteLine($@"Value for {nameof(Opt)} property is '{GetAllValues(Opt)}'");
            Console.WriteLine($@"Value for {nameof(OptArray)} property is '{GetAllValues(OptArray)}'");
            Console.WriteLine($@"Value for {nameof(OptNullable)} property is '{GetAllValues(OptNullable)}'");
            Console.WriteLine($@"Value for {nameof(OptEnumerable)} property is '{GetAllValues(OptEnumerable)}'");
            Console.WriteLine($@"Value for {nameof(OptList)} property is '{GetAllValues(OptList)}'");
            Console.WriteLine($@"Value for {nameof(OptEnumArray)} property is '{GetAllValues(OptEnumArray)}'");
            Console.WriteLine($@"Value for {nameof(Arg)} property is '{GetAllValues(Arg)}'");
            
            Console.WriteLine();
        }

        private string GetAllValues(object value)
        {
            if (value is IEnumerable enumerable)
                return string.Join("|",
                    enumerable
                        .Cast<object>()
                        .Select(s => s?.ToString())
                );

            return value?.ToString();
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

    /*
    public class CustomList<T> : List<T>
    {

    }
    */

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

    namespace Sub
    {
        public class ClassWithParser
        {
            private readonly string value;

            private ClassWithParser(string value)
            {
                this.value = value;
            }

            public override string ToString()
            {
                return value;
            }

            public static ClassWithParser Parse(string value)
            {
                return new ClassWithParser(value);
            }
        }
    }
}
