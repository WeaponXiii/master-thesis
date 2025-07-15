using System.Diagnostics;
using Microsoft.Build.Construction;
using Microsoft.Build.Locator;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Spectre.Console;

using Spectre.Console.Helpers;

MSBuildLocator.RegisterDefaults();

//await PrintSolutionInfo();

CompareFileVersions();

static void CompareFileVersions(
    string oldCommit = "6c38e51",
    string newCommit = "3a35984",
    string filePath = "code/code-parser/code-parser/Program.cs"
)
{
    var oldCode = GetFileAtCommit(oldCommit, filePath);
    var newCode = GetFileAtCommit(newCommit, filePath);

    var oldSyntaxTree = CSharpSyntaxTree.ParseText(oldCode);
    var newSyntaxTree = CSharpSyntaxTree.ParseText(newCode);

    // Compare the old and new syntax trees and print the differences
    var oldRoot = oldSyntaxTree.GetRoot();
    var newRoot = newSyntaxTree.GetRoot();

    var changes = newRoot.SyntaxTree.GetChanges(oldSyntaxTree);

    if (changes.Count == 0)
    {
        AnsiConsole.MarkupLine("[green]No differences found between the syntax trees.[/]");
    }
    else
    {
        AnsiConsole.MarkupLine($"[yellow]{changes.Count} difference(s) found between the syntax trees:[/]");
        foreach (var change in changes)
        {
            var oldNode = oldRoot.FindNode(change.Span, getInnermostNodeForTie: true);
            var newNode = newRoot.FindNode(new Microsoft.CodeAnalysis.Text.TextSpan(change.Span.Start, change.NewText?.Length ?? 0), getInnermostNodeForTie: true);
            AnsiConsole.MarkupLine($"[teal]Old Node:[/] {oldNode.ToConsoleString()}");
            AnsiConsole.MarkupLine($"[lime]New Node:[/] {newNode.ToConsoleString()}");

            // TextChange does not have a Kind property, so we infer the type of change
            string changeType = change switch
            {
                { Span.Length: 0, NewText.Length: > 0 } => "Add into",
                { Span.Length: > 0, NewText.Length: 0 } => "Remove from",
                _ => "Changed"
            };

            var line = Markup.Escape(oldSyntaxTree.GetMappedLineSpan(change.Span).ToString());
            AnsiConsole.MarkupLine($"[red]{changeType} old file@{line}[/] {Markup.Escape(change.ToString())}");
            AnsiConsole.WriteLine(new string('-', 40));
        }
    }

    Console.WriteLine(oldCode[10892..12078]);
    Console.WriteLine(new string('-', 40));
    Console.WriteLine(newCode[10892..12078]);
}

static async Task PrintSolutionInfo(
    string solutionPath = @"/home/rubel/dotnet/pattern/Pattern.sln"
)
{
    var solution = SolutionFile.Parse(solutionPath);
    var projects = solution.ProjectsInOrder;

    foreach (var project in projects)
    {
        Console.WriteLine($"Project: {project.ProjectName}");
        Console.WriteLine($"Path: {project.AbsolutePath}");
        Console.WriteLine($"Project GUID: {project.ProjectGuid}");

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


            var tree = BuildSyntaxTree2(root) as Tree;
            AnsiConsole.Write(tree!);
        }
    }
    Console.WriteLine(GetFileAtCommit(
        "6c38e51", "code/code-parser/code-parser/Program.cs"
    ));
    Console.WriteLine();
}

static IHasTreeNodes BuildSyntaxTree2(SyntaxNodeOrToken node, IHasTreeNodes? parent = null)
{
    // Create the tree node with the kind of the syntax node
    var currentParent = (node, parent) switch
    {
        (_, null) => Tree.From(node.AsNode()!),
        ({ IsNode: true }, IHasTreeNodes p) => p.AddNode(node.AsNode()!),
        ({ IsNode: false }, IHasTreeNodes p) => p.AddNode(node.AsToken()),
    };

    // Recursively process children
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

static string GetFileAtCommit(
    string commitHash = "6c38e51",
    string filePath = "code/code-parser/code-parser/Program.cs"
)
{
    //const int timeout = 10000; // 10 seconds

    // This function is a placeholder for getting a file at a specific commit.
    // You can implement it using LibGit2Sharp or any other Git library.
    Console.WriteLine($"Getting file '{filePath}' at commit '{commitHash}'");

    using Process p = new();
    p.StartInfo.FileName = "/bin/bash";
    var cmd = $"git show {commitHash}:{filePath} | cat";
    var escapedArgs = cmd.Replace("\"", "\\\"");

    //p.StartInfo.Arguments = "-c \"ls\""; // $"git show {commitHash}:{filePath} | cat";

    p.StartInfo.Arguments = $"-c \"{escapedArgs}\"";
    p.StartInfo.UseShellExecute = false;
    p.StartInfo.RedirectStandardOutput = true;
    p.StartInfo.RedirectStandardError = true;
    p.StartInfo.CreateNoWindow = true;

    /* StringBuilder output = new();
    StringBuilder error = new();
    //p.OutputDataReceived += (sender, args) => output.AppendLine(args.Data);
    //p.ErrorDataReceived += (sender, args) => error.AppendLine(args.Data);

    AutoResetEvent outputWaitHandle = new(false);
    AutoResetEvent errorWaitHandle = new(false);
    p.OutputDataReceived += (sender, args) =>
    {
        if (args.Data != null)
        {
            output.AppendLine(args.Data);
        }
        else
        {
            try
            {
                outputWaitHandle.Set();
            }
            catch (Exception e)
            {
                Console.WriteLine($"Error setting output wait handle: {e.Message}");
            }
        }
    };
    p.ErrorDataReceived += (sender, args) =>
    {
        if (args.Data != null)
        {
            error.AppendLine(args.Data);
        }
        else
        {
            if (args.Data == null)
            {
                try
                {
                    errorWaitHandle.Set();
                }
                catch (Exception e)
                {
                    Console.WriteLine($"Error setting error wait handle: {e.Message}");
                }
            }
        }
    };

    p.Start();

    p.BeginOutputReadLine();
    p.BeginErrorReadLine();

    if (p.WaitForExit(timeout) && outputWaitHandle.WaitOne(timeout) && errorWaitHandle.WaitOne(timeout))
    {
        if (!String.IsNullOrEmpty(error.ToString()))
        {
            return error.ToString();
        }
        else
        {
            return output.ToString();
        }
    }
    else
    {
        throw new Exception("Program time out");
    } */

    p.Start();

    string output = p.StandardOutput.ReadToEnd();
    p.WaitForExit();
    if (p.ExitCode != 0)
    {
        string error = p.StandardError.ReadToEnd();
        throw new Exception($"Command failed with exit code {p.ExitCode}: {error}");
    }

    return output;
}
