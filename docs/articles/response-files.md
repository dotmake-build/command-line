# Response files

A *response file* is a file that contains a set of tokens for a command-line app. Response files are a feature of `System.CommandLine` that is useful in two scenarios:

* To invoke a command-line app by specifying input that is longer than the character limit of the terminal.
* To invoke the same command repeatedly without retyping the whole line.

To use a response file, enter the file name prefixed by an `@` sign wherever in the line you want to insert commands, options, and arguments. The *.rsp* file extension is a common convention, but you can use any file extension.

The following lines are equivalent:

```console
dotnet build --no-restore --output ./build-output/
dotnet @sample1.rsp
dotnet build @sample2.rsp --output ./build-output/
```

Contents of *sample1.rsp*:

```console
build
--no-restore 
--output
./build-output/
```

Contents of *sample2.rsp*:

```console
--no-restore
```

Here are syntax rules that determine how the text in a response file is interpreted:

* Tokens are delimited by spaces. A line that contains *Good morning!* is treated as two tokens, *Good* and *morning!*.
* Multiple tokens enclosed in quotes are interpreted as a single token. A line that contains *"Good morning!"* is treated as one token, *Good morning!*.
* Any text between a `#` symbol and the end of the line is treated as a comment and ignored.
* Tokens prefixed with `@` can reference additional response files.
* The response file can have multiple lines of text. The lines are concatenated and interpreted as a sequence of tokens.
