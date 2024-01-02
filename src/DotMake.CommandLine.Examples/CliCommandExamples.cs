using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.CommandLine;
using System.IO;
using System.Linq;

namespace DotMake.CommandLine.Examples
{
    //Indenting is intentional, to fix indentation problem when merging with other code blocks
    #region RootCliCommand

            //Create a simple class like this:

            [CliCommand(Description = "A root cli command")]
            public class RootCliCommand
            {
                [CliOption(Description = "Description for Option1")]
                public string Option1 { get; set; } = "DefaultForOption1";

                [CliArgument(Description = "Description for Argument1")]
                public string Argument1 { get; set; }

                public void Run()
                {
                    Console.WriteLine($@"Handler for '{GetType().FullName}' is run:");
                    Console.WriteLine($@"Value for {nameof(Option1)} property is '{Option1}'");
                    Console.WriteLine($@"Value for {nameof(Argument1)} property is '{Argument1}'");
                    Console.WriteLine();
                }
            }

    #endregion

    #region WriteFileCommand

    //Note that you can have a specific type (other than `string`) for a property which a `CliOption` or `CliArgument`
    //attribute is applied to, for example these properties will be parsed and bound/populated automatically:

    [CliCommand]
    public class WriteFileCommand
    {
        [CliArgument]
        public FileInfo OutputFile { get; set; }

        [CliOption]
        public List<string> Lines { get; set; }

        public void Run()
        {
            if (OutputFile.Exists)
                return;

            using (var streamWriter = OutputFile.CreateText())
            {
                foreach (var line in Lines)
                {
                    streamWriter.WriteLine(line);
                }
            }
        }
    }

    #endregion

    #region EnumerableCliCommand

    //Arrays, lists, collections - any type that implements `IEnumerable<T>` and has a public constructor with a `IEnumerable<T>`
    //or `IList<T>` parameter (other parameters, if any, should be optional).
    //If type is generic `IEnumerable<T>`, `IList<T>`, `ICollection<T>` interfaces itself, array `T[]` will be used.
    //If type is non-generic `IEnumerable`, `IList`, `ICollection` interfaces itself, array `string[]` will be used.

    [CliCommand]
    public class EnumerableCliCommand
    {
        [CliOption]
        public IEnumerable<int> OptEnumerable { get; set; }

        [CliOption]
        public List<string> OptList { get; set; }

        [CliOption(AllowMultipleArgumentsPerToken = true)]
        public FileAccess[] OptEnumArray { get; set; }

        [CliOption]
        public Collection<int?> OptCollection { get; set; }

        [CliOption]
        public HashSet<string> OptHashSet { get; set; }

        [CliOption]
        public Queue<FileInfo> OptQueue { get; set; }

        [CliOption]
        public CustomList<string> OptCustomList { get; set; }

        [CliArgument]
        public IList ArgIList { get; set; }

        public void Run()
        {
            Console.WriteLine($@"Handler for '{GetType().FullName}' is run:");

            Console.WriteLine($@"Value for {nameof(OptEnumerable)} property is '{GetAllValues(OptEnumerable)}'");
            Console.WriteLine($@"Value for {nameof(OptList)} property is '{GetAllValues(OptList)}'");
            Console.WriteLine($@"Value for {nameof(OptEnumArray)} property is '{GetAllValues(OptEnumArray)}'");
            Console.WriteLine($@"Value for {nameof(OptCollection)} property is '{GetAllValues(OptCollection)}'");
            Console.WriteLine($@"Value for {nameof(OptHashSet)} property is '{GetAllValues(OptHashSet)}'");
            Console.WriteLine($@"Value for {nameof(OptQueue)} property is '{GetAllValues(OptQueue)}'");
            Console.WriteLine($@"Value for {nameof(OptCustomList)} property is '{GetAllValues(OptCustomList)}'");
            Console.WriteLine($@"Value for {nameof(ArgIList)} property is '{GetAllValues(ArgIList)}'");

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

    public class CustomList<T> : List<T>
    {
        public CustomList(IEnumerable<T> items)
            : base(items)
        {

        }
    }

    #endregion

    #region ArgumentConverterCliCommand

    //Any type with a public constructor or a static `Parse` method with a string parameter (other parameters, if any,
    //should be optional) - These types can be bound/parsed automatically even if they are wrapped
    //with `Enumerable` or `Nullable` type.

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

        [CliOption]
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

    #region RootSnakeSlashCliCommand

    //For example, change the name casing and prefix convention:

    [CliCommand(
        Description = "A cli command with snake_case name casing and forward slash prefix conventions",
        NameCasingConvention = CliNameCasingConvention.SnakeCase,
        NamePrefixConvention = CliNamePrefixConvention.ForwardSlash,
        ShortFormPrefixConvention = CliNamePrefixConvention.ForwardSlash
    )]
    public class RootSnakeSlashCliCommand
    {
        [CliOption(Description = "Description for Option1")]
        public string Option1 { get; set; } = "DefaultForOption1";

        [CliArgument(Description = "Description for Argument1")]
        public string Argument1 { get; set; }

        public void Run()
        {
            Console.WriteLine($@"Handler for '{GetType().FullName}' is run:");
            Console.WriteLine($@"Value for {nameof(Option1)} property is '{Option1}'");
            Console.WriteLine($@"Value for {nameof(Argument1)} property is '{Argument1}'");
            Console.WriteLine();
        }
    }

    #endregion

    #region RootWithNestedChildrenCliCommand

    //Defining sub-commands in DotMake.Commandline is very easy.We simply use nested classes to create a hierarchy:

    [CliCommand(Description = "A root cli command with nested children")]
    public class RootWithNestedChildrenCliCommand
    {
        [CliOption(Description = "Description for Option1")]
        public string Option1 { get; set; } = "DefaultForOption1";

        [CliArgument(Description = "Description for Argument1")]
        public string Argument1 { get; set; }

        public void Run()
        {
            Console.WriteLine($@"Handler for '{GetType().FullName}' is run:");
            Console.WriteLine($@"Value for {nameof(Option1)} property is '{Option1}'");
            Console.WriteLine($@"Value for {nameof(Argument1)} property is '{Argument1}'");
            Console.WriteLine();
        }

        [CliCommand(Description = "A nested level 1 sub-command")]
        public class Level1SubCliCommand
        {
            [CliOption(Description = "Description for Option1")]
            public string Option1 { get; set; } = "DefaultForOption1";

            [CliArgument(Description = "Description for Argument1")]
            public string Argument1 { get; set; }

            public void Run()
            {
                Console.WriteLine($@"Handler for '{GetType().FullName}' is run:");
                Console.WriteLine($@"Value for {nameof(Option1)} property is '{Option1}'");
                Console.WriteLine($@"Value for {nameof(Argument1)} property is '{Argument1}'");
                Console.WriteLine();
            }

            [CliCommand(Description = "A nested level 2 sub-command")]
            public class Level2SubCliCommand
            {
                [CliOption(Description = "Description for Option1")]
                public string Option1 { get; set; } = "DefaultForOption1";

                [CliArgument(Description = "Description for Argument1")]
                public string Argument1 { get; set; }

                public void Run()
                {
                    Console.WriteLine($@"Handler for '{GetType().FullName}' is run:");
                    Console.WriteLine($@"Value for {nameof(Option1)} property is '{Option1}'");
                    Console.WriteLine($@"Value for {nameof(Argument1)} property is '{Argument1}'");
                    Console.WriteLine();
                }
            }
        }
    }

    //Just make sure you apply `CliCommand` attribute to the nested classes as well.
    //Command hierarchy in above example is:
    //  RootWithNestedChildrenCliCommand -> Level1SubCliCommand -> Level2SubCliCommand

    #endregion

    #region RootWithExternalChildrenCliCommand

    // Another way to create hierarchy between commands, especially if you want to use standalone classes,
    // is to use `Parent` property of `CliCommand` attribute to specify `typeof` parent class:

    [CliCommand(Description = "A root cli command")]
    public class RootWithExternalChildrenCliCommand
    {
        [CliOption(Description = "Description for Option1")]
        public string Option1 { get; set; } = "DefaultForOption1";

        [CliArgument(Description = "Description for Argument1")]
        public string Argument1 { get; set; }

        public void Run()
        {
            Console.WriteLine($@"Handler for '{GetType().FullName}' is run:");
            Console.WriteLine($@"Value for {nameof(Option1)} property is '{Option1}'");
            Console.WriteLine($@"Value for {nameof(Argument1)} property is '{Argument1}'");
            Console.WriteLine();
        }
    }

    [CliCommand(
        Name = "Level1External",
        Description = "An external level 1 sub-command",
        Parent = typeof(RootWithExternalChildrenCliCommand)
    )]
    public class ExternalLevel1SubCliCommand
    {
        [CliOption(Description = "Description for Option1")]
        public string Option1 { get; set; } = "DefaultForOption1";

        [CliArgument(Description = "Description for Argument1")]
        public string Argument1 { get; set; }

        public void Run()
        {
            Console.WriteLine($@"Handler for '{GetType().FullName}' is run:");
            Console.WriteLine($@"Value for {nameof(Option1)} property is '{Option1}'");
            Console.WriteLine($@"Value for {nameof(Argument1)} property is '{Argument1}'");
            Console.WriteLine();
        }

        [CliCommand(Description = "A nested level 2 sub-command")]
        public class Level2SubCliCommand
        {
            [CliOption(Description = "Description for Option1")]
            public string Option1 { get; set; } = "DefaultForOption1";

            [CliArgument(Description = "Description for Argument1")]
            public string Argument1 { get; set; }

            public void Run()
            {
                Console.WriteLine($@"Handler for '{GetType().FullName}' is run:");
                Console.WriteLine($@"Value for {nameof(Option1)} property is '{Option1}'");
                Console.WriteLine($@"Value for {nameof(Argument1)} property is '{Argument1}'");
                Console.WriteLine();
            }
        }
    }

    //Command hierarchy in above example is:
    //  RootWithExternalChildrenCliCommand -> ExternalLevel1SubCliCommand -> Level2SubCliCommand

    #endregion
}
