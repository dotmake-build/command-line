#pragma warning disable CS1591
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

        public void Run()
        {
            Console.WriteLine($@"Handler for '{GetType().FullName}' is run:");
            
            Console.WriteLine($@"Value for {nameof(Opt)} property is '{GetAllValues(Opt)}'");
            Console.WriteLine($@"Value for {nameof(OptArray)} property is '{GetAllValues(OptArray)}'");
            Console.WriteLine($@"Value for {nameof(OptNullable)} property is '{GetAllValues(OptNullable)}'");
            Console.WriteLine($@"Value for {nameof(OptEnumerable)} property is '{GetAllValues(OptEnumerable)}'");
            Console.WriteLine($@"Value for {nameof(OptList)} property is '{GetAllValues(OptList)}'");
            Console.WriteLine($@"Value for {nameof(OptCustomList)} property is '{GetAllValues(OptCustomList)}'");
            Console.WriteLine($@"Value for {nameof(Arg)} property is '{GetAllValues(Arg)}'");
            
            Console.WriteLine();
        }

        private static string GetAllValues(object value)
        {
            if (value is IEnumerable enumerable)
            {
                var items = enumerable.Cast<object>().ToArray();
                if (items.Length == 0)
                    return "<empty>";
                return string.Join("|", items.Select(GetValue));
            }

            return GetValue(value);
        }

        private static string GetValue(object value)
        {
            if (value == null)
                return "<null>";

            return value.ToString();
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
