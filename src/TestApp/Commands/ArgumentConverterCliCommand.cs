using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DotMake.CommandLine;

namespace TestApp.Commands
{
    [CliCommand]
    public class ArgumentConverterCliCommand
    {
        [CliOption]
        public ClassWithConstructor Opt { get; set; }

        [CliOption]
        public ClassWithConstructor[] OptArray { get; set; }

        [CliOption]
        public CustomStruct? OptNullable { get; set; }

        [CliOption]
        public IEnumerable<ClassWithConstructor> OptEnumerable { get; set; }

        [CliOption]
        public List<ClassWithConstructor> OptList { get; set; }

        /*
        [CliOption]
        public CustomList<ClassWithConstructor> OptCustomList { get; set; }
        */

        [CliArgument]
        public IEnumerable<Sub.ClassWithParser> Arg { get; set; }

        public void Run()
        {
            Console.WriteLine($@"Handler for '{GetType().FullName}' is run:");
            
            foreach (var property in GetType().GetProperties())
            {
                var value = property.GetValue(this);
                if (value is IEnumerable enumerable)
                    value = string.Join(", ",
                        enumerable
                        .Cast<object>()
                        .Select(s => s.ToString())
                    );

                Console.WriteLine($@"Value for {property.Name} property is '{value}'");

            }
            
            Console.WriteLine();
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
