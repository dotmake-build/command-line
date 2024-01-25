#pragma warning disable CS1591
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.CommandLine.Invocation;
using System.IO;
using DotMake.CommandLine;

namespace TestApp.Commands
{
    [CliCommand]
    public class EnumerableCliCommand
    {
        [CliOption(Required = false)]
        public IEnumerable<int> OptEnumerable { get; set; }

        [CliOption(Required = false)]
        public List<string> OptList { get; set; }

        [CliOption(Required = false, AllowMultipleArgumentsPerToken = true)]
        public FileAccess[] OptEnumArray { get; set; }

        [CliOption(Required = false)]
        public Collection<int?> OptCollection { get; set; }

        [CliOption(Required = false)]
        public HashSet<string> OptHashSet { get; set; }

        [CliOption(Required = false)]
        public Queue<FileInfo> OptQueue { get; set; }

        [CliOption(Required = false)]
        public CustomList<string> OptCustomList { get; set; }

        [CliArgument]
        public IList ArgIList { get; set; }

        public void Run(InvocationContext context)
        {
            context.ShowValues();
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
