// See https://aka.ms/new-console-template for more information

using System.Reflection;
using ConsoleAppTest;

var assembly = Assembly.GetExecutingAssembly();

foreach (var r in assembly.GetManifestResourceNames())
{
    Console.WriteLine($"Resource {r}");
    using var stream = assembly.GetManifestResourceStream(r);
    using var reader = new StreamReader(stream!);

    var len = reader.ReadToEnd().Split("\n").Length;
    
    Console.WriteLine($"Len: {len}");

    //GeneratedHelper.Test();

    GeneratedHelper2.Test();
}


var t = new TestClass();
t.Say("Hello");

Messenger.Say("Hello, World!");