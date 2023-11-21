using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace AwesomeGenerators;

[Generator]
public class FluentBuilderGenerator2 : ISourceGenerator
{
    public void Initialize(GeneratorInitializationContext context)
    {
    }

    public void Execute(GeneratorExecutionContext context)
    {
        context.AddSource("TestClass.g.cs", GetFile());
        context.AddSource("GeneratedHelper.g.cs", GetHelperFile());
        context.AddSource("Test3.cs", $"//this is a test file {DateTime.Now}");

        var interfaces =
            context.Compilation
                .SyntaxTrees
                .SelectMany(static x => x.GetRoot()
                                         .DescendantNodesAndSelf()
                                         .OfType<InterfaceDeclarationSyntax>())
                .ToList();

        foreach(var interf in interfaces)
        {
            var name = interf.Identifier.ToString();
            context.AddSource($"BuilderFor{name}.g.cs", 
            $@"namespace ConsoleAppTest;
public class BuilderFor{name}
{{
    public {name} Build()
    {{
        return null!;
    }}
}}" );

        }
    }

    private string GetFile()
    {
        return 
            $@"namespace ConsoleAppTest;
partial class TestClass
{{
    partial void SayIt(string from, string msg)
    {{
        System.Console.WriteLine($""{{from}} says: {{msg}} at {DateTime.Now}!"");
    }}
}}";
    }

    private string GetHelperFile()
    {
        return 
            $@"namespace ConsoleAppTest;
public static class GeneratedHelper
{{
    public static void Test()
    {{
        Console.WriteLine(""test"");
    }}
}}";
    }
    
    private DiagnosticDescriptor CreateError(Exception ex)
    {
        var descriptor = new DiagnosticDescriptor(
            "SG0001",
            "Interface generation error",
            " Exception:" + ex.Message + " ",
            "",
            DiagnosticSeverity.Error,
            true);

        return descriptor;
    }
}
