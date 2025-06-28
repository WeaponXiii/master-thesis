using System.Diagnostics;
using Microsoft.Build.Construction;
using Microsoft.Build.Locator;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Spectre.Console;

using CodeParser.Helpers;

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

            // Let's proceed with DescendantNodes
            var descendants = root.DescendantNodes();

            /* var classes = descendants.OfType<ClassDeclarationSyntax>();
            foreach (var classDeclaration in classes)
            {
                Console.WriteLine($"    Class: {classDeclaration.Identifier.Text}");
            } */

            /* foreach (SyntaxNode item in descendants)
            {
                var identifier = item switch
                {
                    UsingDirectiveSyntax usingDirective => usingDirective.Name!.ToString(),
                    NamespaceDeclarationSyntax namespaceDecl => namespaceDecl.Name.ToString(),
                    IdentifierNameSyntax idName => idName.Identifier.Text,
                    ClassDeclarationSyntax classDecl => classDecl.Identifier.Text,
                    MethodDeclarationSyntax methodDecl => methodDecl.Identifier.Text,
                    PropertyDeclarationSyntax propertyDecl => propertyDecl.Identifier.Text,
                    FieldDeclarationSyntax fieldDecl => fieldDecl.Declaration.Variables.FirstOrDefault()?.Identifier.Text ?? "",
                    LocalFunctionStatementSyntax localFunc => localFunc.Identifier.Text,
                    _ => ""
                };

                // Console.WriteLine($"  Descendant Node: {identifier}({item.Kind()}) Line: {item.GetLocation().GetLineSpan()} {item.GetLocation().GetMappedLineSpan()} span: {item.Span} FullSpan: {item.FullSpan}");

                Console.WriteLine($"  Descendant Node: {identifier}({item.Kind()}) Line: {item.GetLocation().GetLineSpan()} span: {item.Span}");
            } */

            // Print all descendant nodes, tokens, and trivia's in hierarchical order
            /* void PrintSyntax(SyntaxNodeOrToken nodeOrToken, int indent = 0)
            {
                string indentStr = new string(' ', indent * 2);
                if (nodeOrToken.IsNode)
                {
                    var node = nodeOrToken.AsNode()!;
                    Console.WriteLine($"{indentStr}Node: {node.Kind()} Span: {node.Span} line: {node.GetLocation().GetLineSpan()} Text: '{node.ToString().Replace("\n", "\\n").Replace("\r", "\\r")}'");
                    foreach (var child in node.ChildNodesAndTokens())
                    {
                        PrintSyntax(child, indent + 1);
                    }
                }
                else
                {
                    var token = nodeOrToken.AsToken();
                    Console.WriteLine($"{indentStr}Token: {token.Kind()} Span: {token.Span} line: {token.GetLocation().GetLineSpan()} Text: '{token.Text}'");
                    foreach (var trivia in token.LeadingTrivia)
                    {
                        Console.WriteLine($"{indentStr}  LeadingTrivia: {trivia.Kind()} Span: {trivia.Span} line: {trivia.GetLocation().GetLineSpan()} Text: '{trivia.ToString().Replace("\n", "\\n").Replace("\r", "\\r")}'");
                    }
                    foreach (var trivia in token.TrailingTrivia)
                    {
                        Console.WriteLine($"{indentStr}  TrailingTrivia: {trivia.Kind()} Span: {trivia.Span} line: {trivia.GetLocation().GetLineSpan()} Text: '{trivia.ToString().Replace("\n", "\\n").Replace("\r", "\\r")}'");
                    }
                }
            }

            PrintSyntax(root); */

            /*             // ...existing code...

            Tree BuildSyntaxTree(SyntaxNode node)
            {
                // Create the root tree node with the kind of the syntax node
                var tree = new Tree($"[yellow]{node.Kind()}[/]");

                // Recursively add child nodes
                void AddChildren(TreeNode parent, SyntaxNodeOrToken nodeOrToken)
                {
                    if (nodeOrToken.IsNode)
                    {
                        var childNode = nodeOrToken.AsNode()!;
                        var childTreeNode = parent.AddNode($"[green]{childNode.Kind()}[/]");
                        foreach (var child in childNode.ChildNodesAndTokens())
                        {
                            AddChildren(childTreeNode, child);
                        }
                    }
                    else
                    {
                        var token = nodeOrToken.AsToken();
                        parent.AddNode($"[blue]{token.Kind()}[/] '[grey]{token.Text}[/]'");
                    }
                }

                // Add children to the root node of the tree
                foreach (var child in node.ChildNodesAndTokens())
                {
                    // AddNode returns a TreeNode, which we use for recursion
                    if (child.IsNode)
                    {
                        var childNode = child.AsNode()!;
                        var childTreeNode = tree.AddNode($"[green]{childNode.Kind()}[/]");
                        foreach (var grandChild in childNode.ChildNodesAndTokens())
                        {
                            AddChildren(childTreeNode, grandChild);
                        }
                    }
                    else
                    {
                        var token = child.AsToken();
                        tree.AddNode($"[blue]{token.Kind()}[/] '[grey]{token.Text}[/]'");
                    }
                }

                return tree;
            } */

            Tree BuildSyntaxTree(SyntaxNode node)
            {
                // Create the root tree node with the kind of the syntax node
                var tree = new Tree($"[yellow]{node.Kind()}[/]");

                // Recursively add child nodes
                void AddChildren(TreeNode parent, SyntaxNodeOrToken nodeOrToken)
                {
                    if (nodeOrToken.IsNode)
                    {
                        var childNode = nodeOrToken.AsNode()!;
                        var childTreeNode = parent.AddNode(ConsoleTreeHelper.MakeConsoleTreeNode(childNode));
                        foreach (var child in childNode.ChildNodesAndTokens())
                        {
                            AddChildren(childTreeNode, child);
                        }
                    }
                    else
                    {
                        var token = nodeOrToken.AsToken();
                        var tokenTreeNode = parent.AddNode(ConsoleTreeHelper.MakeConsoleTreeNode(token));
                        foreach (var trivia in token.LeadingTrivia)
                        {
                            tokenTreeNode.AddNode(ConsoleTreeHelper.MakeConsoleTreeNode(trivia));
                        }
                        foreach (var trivia in token.TrailingTrivia)
                        {
                            tokenTreeNode.AddNode(ConsoleTreeHelper.MakeConsoleTreeNode(trivia));
                        }
                    }
                }

                // Add children to the root node of the tree
                foreach (var child in node.ChildNodesAndTokens())
                {
                    // AddNode returns a TreeNode, which we use for recursion
                    if (child.IsNode)
                    {
                        var childNode = child.AsNode()!;
                        var childTreeNode = tree.AddNode(ConsoleTreeHelper.MakeConsoleTreeNode(childNode));
                        foreach (var grandChild in childNode.ChildNodesAndTokens())
                        {
                            AddChildren(childTreeNode, grandChild);
                        }
                    }
                    else
                    {
                        var token = child.AsToken();
                        var tokenTreeNode = tree.AddNode(ConsoleTreeHelper.MakeConsoleTreeNode(token));
                        foreach (var trivia in token.LeadingTrivia)
                        {
                            tokenTreeNode.AddNode(ConsoleTreeHelper.MakeConsoleTreeNode(trivia));
                        }
                        foreach (var trivia in token.TrailingTrivia)
                        {
                            tokenTreeNode.AddNode(ConsoleTreeHelper.MakeConsoleTreeNode(trivia));
                        }
                    }
                }

                return tree;
            }

            IHasTreeNodes BuildSyntaxTree2(SyntaxNodeOrToken node, IHasTreeNodes? parent = null)
            {
                // Create the tree node with the kind of the syntax node
                var currentParent = parent ?? new Tree($"[yellow]{node.Kind()}[/]");

                // Recursively add child nodes
                foreach (var child in node.ChildNodesAndTokens())
                {
                    if (child.IsNode)
                    {
                        var childNode = child.AsNode()!;
                        var childTreeNode = currentParent.AddNode(ConsoleTreeHelper.MakeConsoleTreeNode(childNode));
                        foreach (var grandChild in childNode.ChildNodesAndTokens())
                        {
                            BuildSyntaxTree2(grandChild, childTreeNode);
                        }
                    }
                    else
                    {
                            var token = child.AsToken();
                        var tokenTreeNode = currentParent.AddNode(ConsoleTreeHelper.MakeConsoleTreeNode(token));
                        foreach (var trivia in token.LeadingTrivia)
                        {
                            tokenTreeNode.AddNode(ConsoleTreeHelper.MakeConsoleTreeNode(trivia));
                        }
                        foreach (var trivia in token.TrailingTrivia)
                        {
                            tokenTreeNode.AddNode(ConsoleTreeHelper.MakeConsoleTreeNode(trivia));
                        }
                    }
                }

                return currentParent;
            }

            // Encountered unescaped ']' token at position 37
            var syntaxTreeDisplay = BuildSyntaxTree(root);
            AnsiConsole.Write(syntaxTreeDisplay);

            /* var tree = BuildSyntaxTree2(root) as Tree;
            AnsiConsole.Write(tree!); */
        }
    }

    Console.WriteLine();
}
