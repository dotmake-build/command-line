using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using DotMake.CommandLine;

namespace TestApp.Commands
{
    [CliCommand]
    public class EnumerableCliCommand
    {
        [CliOption]
        public IEnumerable<int> OptEnumerable { get; set; } = null;

        [CliOption]
        public List<string> OptList { get; set; } = null;

        [CliOption(AllowMultipleArgumentsPerToken = true)]
        public FileAccess[] OptEnumArray { get; set; } = null;

        [CliOption]
        public Collection<int?> OptCollection { get; set; } = null;

        [CliOption]
        public HashSet<string> OptHashSet { get; set; } = null;

        [CliOption]
        public Queue<FileInfo> OptQueue { get; set; } = null;

        [CliOption]
        public CustomList<string> OptCustomList { get; set; } = null;

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
            //throw new NullReferenceException();
        }
    }
}
