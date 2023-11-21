using System.Collections.Immutable;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace AwesomeGenerators;

[Generator]
public class FluentBuilderGenerator : IIncrementalGenerator
{
    public int Counter = 0;

    public void Initialize(IncrementalGeneratorInitializationContext context)
    {
        // define the execution pipeline here via a series of transformations:

        // find all additional files that end with .txt
        var textFiles = context.AdditionalTextsProvider.Where(static file => file.Path.EndsWith(".txt"));

        var classes = context.CompilationProvider
                             .Select((x, c) => x.SyntaxTrees);

        var interfaceDeclarations =
            context.SyntaxProvider
                   .CreateSyntaxProvider(
                       predicate: static (s, _) => IsSyntaxTargetForGeneration(s),
                       transform: static (ctx, _) => GetSemanticTargetForGeneration(ctx))
                   .Where(static m => m is not null);

        var compilationAndClasses
            = context.CompilationProvider.Combine(interfaceDeclarations.Collect());

        context.RegisterSourceOutput(
            compilationAndClasses,
            static (spc, source) => Execute(source.Left, source.Right, spc));

        var classDeclarations =
            context.SyntaxProvider
                   .CreateSyntaxProvider(
                       predicate: static (s, _) => IsSyntaxIsInterface(s),
                       transform: static (ctx, _) => GetClassForInterface(ctx))
                   .Where(static m => m is not null);

        static bool IsSyntaxTargetForGeneration(SyntaxNode node)
            => node is ClassDeclarationSyntax c; //&& m.AttributeLists.Count > 0;

        static bool IsSyntaxIsInterface(SyntaxNode node)
            => node is InterfaceDeclarationSyntax c; //&& m.AttributeLists.Count > 0;

        context.RegisterSourceOutput(
            classDeclarations,
            static (spc, source) => Execute2(source, spc));

        //context.RegisterSourceOutput(classes, GetSource);

        var projectDirProvider =
            context.AnalyzerConfigOptionsProvider
                   .Select(
                       static (provider, _) =>
                       {
                           provider.GlobalOptions.TryGetValue(
                               "build_property.projectdir",
                               out string? projectDirectory);

                           return projectDirectory;
                       });

        var paths 
            = context.CompilationProvider.Combine(projectDirProvider);
        
        context.RegisterSourceOutput(
            paths, 
            (ctx, source) =>
            {
                var projectDirectory = source.Right;
                using var file = File.CreateText(projectDirectory + "/Generated/stat.dat");

                foreach (var v in source.Left.SyntaxTrees)
                {
                    file.WriteLine(v.FilePath);
                }
            });
    }

    private static void Execute2(ClassDeclarationSyntax c, SourceProductionContext spc)
    {
        spc.AddSource($"{c.Identifier.ToString()}.g.cs", c.NormalizeWhitespace().ToString());

        

    }

    private static void Execute(
        Compilation sourceLeft,
        ImmutableArray<InterfaceDeclarationSyntax> interfaces,
        SourceProductionContext spc)
    {
        foreach (var i in interfaces)
        {
            spc.AddSource($"{i.Identifier.ToString()}.g.cs", i.NormalizeWhitespace().ToString());
        }
        
        spc.AddSource("GeneratedHelper2.g.cs", 
            $@"namespace ConsoleAppTest;
public static class GeneratedHelper2
{{
    public static void Test()
    {{
        Console.WriteLine(""test2"");
    }}
}}");
    }

    void GetSource(SourceProductionContext spc, IEnumerable<SyntaxTree> trees)
    {
        var classes = trees.SelectMany(x => x.GetRoot().DescendantNodes().OfType<ClassDeclarationSyntax>())
                           .ToList();

        foreach (var c in classes)
        {
            var name = c.Identifier.ToString();
            spc.AddSource(
                $"{name}.g.cs",
                $@"
public static partial class Class{name}
{{
    // count = {Counter++};
}}");
        }
    }

    static InterfaceDeclarationSyntax GetSemanticTargetForGeneration(GeneratorSyntaxContext context)
    {
        // ...
        var classDeclaration = (ClassDeclarationSyntax)context.Node;
        return InterfaceDeclaration($"InterfaceFor{classDeclaration.Identifier.ToString()}");
    }

    static ClassDeclarationSyntax GetClassForInterface(GeneratorSyntaxContext context)
    {
        // ...
        var i = (InterfaceDeclarationSyntax)context.Node;
        return ClassDeclaration($"ClassFor{i.Identifier.ToString()}");
    }


    public void Initialize2(IncrementalGeneratorInitializationContext initContext)
    {
    }
}