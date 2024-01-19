using DotMake.CommandLine.Examples;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

// ReSharper disable once CheckNamespace
namespace DotMake.CommandLine.CliCommandExamples
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

    //Another way to create hierarchy between commands, especially if you want to use standalone classes,
    //is to use `Parent` property of `CliCommand` attribute to specify `typeof` parent class:

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

    #region InheritanceCliCommand

    //When you have repeating/common options and arguments for your commands, you can define them once in a base class and then 
    //share them by inheriting that base class in other command classes.Interfaces are also supported !

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

    //The property attribute and the property initializer from the most derived class in the hierarchy will be used 
    //(they will override the base ones). The command handler (Run or RunAsync) will be also inherited.
    //So in the above example, `InheritanceCliCommand` inherits options `Username`, `Password` from a base class and
    //option `Department` from an interface. Note that the property initializer for `Department` is in the derived class, 
    //so that default value will be used.

    #endregion

    #region FileSystemInfoCliCommand

    // AllowExisting property in [CliOption] and [CliArgument] controls whether
    // an argument should accept only values corresponding to an existing file or directory.

    [CliCommand(Description = "A cli command with input (must exists) files and or directories")]
    public class FileSystemInfoCliCommand
    {
        [CliOption(Description = "Optional input file (must exists)", AllowExisting = true, Required = false)]
        public FileInfo? FileOpt { get; set; }

        [CliOption(Description = "Optional input directory (must exists)", AllowExisting = true, Required = false)]
        public DirectoryInfo? DirOpt { get; set; }

        [CliArgument(Description = "Input file or directory (must exists)", AllowExisting = true)]
        public required FileSystemInfo FileOrDirArg { get; set; }

        public void Run()
        {
            Console.WriteLine($@"Handler for '{GetType().FullName}' is run:");
            Console.WriteLine($@"Value for {nameof(FileOpt)} property is '{FileOpt}'");
            Console.WriteLine($@"Value for {nameof(DirOpt)} property is '{DirOpt}'");
            Console.WriteLine($@"Value for {nameof(FileOrDirArg)} property is '{FileOrDirArg}'");
            Console.WriteLine();
        }
    }

    #endregion

    #region LocalizedCliCommand

    //Localizing commands, options and arguments is supported.
    //You can specify a `nameof` operator expression with a resource property (generated by resx) in the attribute's argument (for `string` types only)
    //and the source generator will smartly use the resource property accessor as the value of the argument so that it can localize at runtime.
    //If the property in the `nameof` operator expression does not point to a resource property, then the name of that property will be used as usual.

    [CliCommand(Description = nameof(TestResources.CommandDescription))]
    internal class LocalizedCliCommand
    {
        [CliOption(Description = nameof(TestResources.OptionDescription))]
        public string Option1 { get; set; } = "DefaultForOption1";

        [CliArgument(Description = nameof(TestResources.ArgumentDescription))]
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
}
