
//This project has <PackageReference> to DotMake.CommandLine
//but DotMake.CommandLine.dll is not actually referenced/consumed due to <ExcludeAssets>compile</ExcludeAssets>
//So we should not be generating any source code for such projects
Console.WriteLine("Hello, World!");
