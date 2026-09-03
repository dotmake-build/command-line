# Model binding

When the command handler is run, the properties for CLI options and arguments will be already populated 
and bound from values passed in the command-line. If no matching value is passed, the property will have its default value if
it has one or an error will be displayed if it's a required option/argument and it was not specified on the command-line.

An option/argument will be considered required when
- There is no property initializer and the property type is a reference type (e.g. `public string Arg { get; set; }`). 
  `string` is a reference type which has a null as the default value but `bool` and `enum` are value
  types which already have non-null default values. `Nullable<T>` is a reference type, e.g. `bool?`.
- There is a property initializer, but it's initialized with `null` or `null!` (SuppressNullableWarningExpression)
  (e.g. `public string Arg { get; set; } = null!;`).
- If it's forced via attribute property `Required` (e.g. `[CliArgument(Required = true)]`).
- If it's forced via `required` modifier (e.g. `public required string Opt { get; set; }`).
  Note that for being able to use `required` modifier, if your target framework is below net7.0, 
  you also need `<LangVersion>11.0</LangVersion>` tag (minimum) in your .csproj file (our source generator supplies the polyfills
  automatically as long as you set C# language version to 11).

An option/argument will be considered optional when
- There is no property initializer (e.g. `public bool Opt { get; set; }`) but the property type is a value type 
  which already have non-null default value.
- There is a property initializer, and it's not initialized with `null` or `null!` (SuppressNullableWarningExpression)
  (e.g. `public string Arg { get; set; } = "Default";`).
- If it's forced via attribute property `Required` (e.g. `[CliArgument(Required = false)]`).

When the default value for a property is not known/null, we make option/argument required by default. 
For example for reference types like `string`, default value is `null` so it's marked required 
but for value types like `bool` default value is `false` - non-null so it's not marked optional.

```c#
[CliOption]
public string Opt1 { get; set; } // => Required because default value is null

[CliOption]
public bool Opt2 { get; set; } // => Optional because default value is non-null

[CliOption]
public bool? Opt3 { get; set; } // => Required because default value is null
```

You can put `Required = false` to the attribute to force it to be optional. 
But in that case, you will need to deal with `null` values in your `Run`method:
```c#
[CliOption(Required = false)]
public string Opt1 { get; set; } // => Optional because attribute has `Required = false`

[CliOption(Required = false)]
public bool? Opt3 { get; set; } // => Optional because attribute has `Required = false`
```
So this way, with `Required = false`, you can check for `null` to determine if an optional option was not provided at all.

---
When you run,
```console
TestApp.exe NewValueForArgument1
```
or (note the double hyphen/dash which allows `dotnet run` to pass arguments to our actual application):
```console
dotnet run -- NewValueForArgument1
```
You see this result:
```console
Handler for 'TestApp.Commands.RootCliCommand' is run:
Value for Option1 property is 'DefaultForOption1'
Value for Argument1 property is 'NewValueForArgument1'
```

---

## Manual binding
When using `Cli.Parse`, you can do manual binding by calling methods of the returned `CliResult` object.
These methods are also available in `CliContext.Result` which can be accessed in `Run` command handler.


```c#
var result = Cli.Parse<RootCliCommand>(args);

//Bind returns null if the command line input does not contain
//the indicated definition class (as self or as a parent)
var subCommand = result.Bind<SubCliCommand>();
//unless you set new returnEmpty parameter to true
var subCommand2 = result.Bind<SubCliCommand>(true);

//You can get an object for called command
//without specifying the definition class
var command = result.BindCalled();
if (command is SubCliCommand subCommand3)
{

}
//Or get an array of objects for all contained commands
//(self and parents) without specifying the definition class
var commands = result.BindAll();
if (commands[0] is SubCliCommand subCommand4)
{

}

//You can check if the command line input is
//for the indicated definition class
if (result.IsCalled<SubCliCommand>())
{

}
//You can check if the command line input contains
//the indicated definition class (as self or as a parent)
if (result.Contains<SubCliCommand>())
{

}

//You can create a new instance of the command definition class
//but without any binding. This is useful for example when you need to
//instantiate a definition class when using dependency injection.
var subCommand5 = result.Create<SubCliCommand>();
```

## Supported types
Note that you can have a specific type (other than `string`) for a property which a `[CliOption]` or `[CliArgument]` attribute is applied to, for example these properties will be parsed and bound/populated automatically:
```c#
[CliCommand]
public class WriteFileCliCommand
{
    [CliArgument]
    public FileInfo OutputFile { get; set; }

    [CliOption]
    public List<string> Lines { get; set; }
}
```
The following types for properties are supported:
* Booleans (flags) - If `true` or `false` is passed for an option having a `bool` argument, it is parsed and bound as expected.
  But an option whose argument type is `bool` doesn't require an argument to be specified.
  The presence of the option token on the command line, with no argument following it, results in a value of `true`.
* Enums - The values are bound by name, and the binding is case insensitive
* Common CLR types:
  
  * `FileSystemInfo`, `FileInfo`, `DirectoryInfo`
  * `int`, `long`, `short`, `uint`, `ulong`, `ushort`
  * `double`, `float`, `decimal`
  * `byte`, `sbyte`
  * `DateTime`, `DateTimeOffset`, `TimeSpan`, `DateOnly`, `TimeOnly`
  * `Guid`
  * `Uri`, `IPAddress`, `IPEndPoint`

* Any type with a public constructor or a static `Parse` method with a string parameter (other parameters, if any, should be optional) - These types can be bound/parsed 
  automatically even if they are wrapped with `Enumerable` or `Nullable` type.
    ```c#
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
    ```
  
* Arrays, lists, collections:
  
  * Any type that implements `IEnumerable<T>` and has a public constructor with a `IEnumerable<T>` or `IList<T>` parameter 
    (other parameters, if any, should be optional). CLR collection types already satisfy this condition.
  
  * If type is generic `IEnumerable<T>`, `IList<T>`, `ICollection<T>` interfaces itself, array `T[]` will be used to create an instance.
  
  * If type is non-generic `IEnumerable`, `IList`, `ICollection` interfaces itself, array `string[]` will be used to create an instance.
  
  ```c#
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
      public Collection<string> OptCollection { get; set; }

      [CliOption]
      public HashSet<string> OptHashSet { get; set; }

      [CliOption]
      public Queue<FileInfo> OptQueue { get; set; }

      [CliOption]
      public CustomList<string> OptCustomList { get; set; }

      [CliArgument]
      public IList ArgIList { get; set; }
  }

  public class CustomList<T> : List<T>
  {
      public CustomList(IEnumerable<T> items)
          : base(items)
      {

      }
  }
  ```

## Validation

In `[CliOption]` and `[CliArgument]` attributes;
`ValidationRules` property allows setting predefined validation rules such as
- `CliValidationRules.ExistingFile`
- `CliValidationRules.NonExistingFile`
- `CliValidationRules.ExistingDirectory`
- `CliValidationRules.NonExistingDirectory`
- `CliValidationRules.ExistingFileOrDirectory`
- `CliValidationRules.NonExistingFileOrDirectory`
- `CliValidationRules.LegalPath`
- `CliValidationRules.LegalFileName`
- `CliValidationRules.LegalUri` 
- `CliValidationRules.LegalUrl`

Validation rules can be combined via using bitwise 'or' operator(`|` in C#).

`ValidationPattern` property allows setting a regular expression pattern for custom validation,
and `ValidationMessage` property allows setting a custom error message to show when `ValidationPattern` does not match.

```c#
[CliCommand]
public class ValidationCliCommand
{
    [CliOption(Required = false, ValidationRules = CliValidationRules.ExistingFile)]
    public FileInfo OptFile1 { get; set; }

    [CliOption(Required = false, ValidationRules = CliValidationRules.NonExistingFile | CliValidationRules.LegalPath)]
    public string OptFile2 { get; set; }

    [CliOption(Required = false, ValidationPattern = @"(?i)^[a-z]+$")]
    public string OptPattern1 { get; set; }

    [CliOption(Required = false, ValidationPattern = @"(?i)^[a-z]+$", ValidationMessage = "Custom error message")]
    public string OptPattern2 { get; set; }

    [CliOption(Required = false, ValidationRules = CliValidationRules.LegalUrl)]
    public string OptUrl { get; set; }

    [CliOption(Required = false, ValidationRules = CliValidationRules.LegalUri)]
    public string OptUri { get; set; }

    [CliArgument(Required = false, ValidationRules = CliValidationRules.LegalFileName)]
    public string OptFileName { get; set; }

    public void Run(CliContext context)
    {
        context.ShowValues();
    }
}
```
