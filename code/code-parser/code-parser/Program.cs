using System.Diagnostics;
using Microsoft.Build.Construction;
using Microsoft.Build.Locator;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Spectre.Console;

using spectre.console.helpers;

MSBuildLocator.RegisterDefaults();

await PrintSolutionInfo();

static async Task PrintSolutionInfo()
{
    var solution = SolutionFile.Parse(@"/home/rubel/dotnet/pattern/Pattern.sln");
    var projects = solution.ProjectsInOrder;

    foreach (var project in projects)
    {
        /* if (project.ProjectType == SolutionProjectType.KnownToBeMSBuildFormat)
        { */
        Console.WriteLine($"Project: {project.ProjectName}");
        Console.WriteLine($"Path: {project.AbsolutePath}");
        Console.WriteLine($"Project GUID: {project.ProjectGuid}");
        /* } */

        var projectRoot = Microsoft.Build.Evaluation.ProjectCollection.GlobalProjectCollection.LoadProject(project.AbsolutePath);
        var csItem = projectRoot.Items
            .Where(item => item.ItemType == "Compile" && item.EvaluatedInclude.EndsWith(".cs", StringComparison.OrdinalIgnoreCase))
            .Select(i => i).ToArray();
        var csFiles = csItem
            .Select(item => Path.GetFullPath(Path.Combine(Path.GetDirectoryName(projectRoot.FullPath)!, item.EvaluatedInclude)));

        foreach (var csFile in csFiles)
        {
            Console.WriteLine(new string('-', 50));
            Console.WriteLine($"  C# File: {csFile}");
            Console.WriteLine(new string('-', 50));

            var syntaxTree = CSharpSyntaxTree.ParseText(await File.ReadAllTextAsync(csFile));
            var root = syntaxTree.GetRoot();
            var compilationRoot = syntaxTree.GetCompilationUnitRoot();
            Debug.Assert(root.Equals(compilationRoot), "Root nodes should be equal");

            Console.WriteLine($"{await syntaxTree.GetTextAsync()}");
            Console.WriteLine(new string('*', 50));

            var descendantNodes = root.DescendantNodes();
            var members = compilationRoot.Members;
            Debug.Assert(members.Count != descendantNodes.Count(), "Members count should not match descendant nodes count");
            Debug.Assert(!descendantNodes.Equals(members), "Descendant nodes should not match members");

            IHasTreeNodes BuildSyntaxTree2(SyntaxNodeOrToken node, IHasTreeNodes? parent = null)
            {
                // Create the tree node with the kind of the syntax node
                var currentParent = (node, parent) switch
                {
                    (_, null) => Tree.From(node.AsNode()!),
                    ({ IsNode: true}, IHasTreeNodes p) => p.AddNode(node.AsNode()!),
                    ({ IsNode: false}, IHasTreeNodes p) => p.AddNode(node.AsToken()),
                }

                /*
                dynamic foo = node.IsNode ? node.AsNode()! : node.AsToken();
                // Extension methods can't be dynamically invoked :(
                var baal = parent?.AddNode(foo) ?? Tree.From(node.AsNode()!);
                */

                ;

                // Recursively add child nodes
                if (node.IsNode)
                {
                    foreach (var child in node.ChildNodesAndTokens())
                    {
                        BuildSyntaxTree2(child, currentParent);
                    }
                }
                else
                {
                    var token = node.AsToken();
                    foreach (var trivia in token.LeadingTrivia)
                    {
                        currentParent.AddNode(trivia, TriviaType.LeadingTrivia);
                    }
                    foreach (var trivia in token.TrailingTrivia)
                    {
                        currentParent.AddNode(trivia, TriviaType.TrailingTrivia);
                    }
                }

                return currentParent;
            }

            var tree = BuildSyntaxTree2(root) as Tree;
            AnsiConsole.Write(tree!);
        }
    }

    Console.WriteLine();
}
