using System.Diagnostics;
using Microsoft.Build.Construction;
using Microsoft.Build.Locator;

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;

using Spectre.Console;

using spectre.console.helpers;
using System.Text;

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
                    ({ IsNode: true }, IHasTreeNodes p) => p.AddNode(node.AsNode()!),
                    ({ IsNode: false }, IHasTreeNodes p) => p.AddNode(node.AsToken()),
                }

                /*
                dynamic foo = node.IsNode ? node.AsNode()! : node.AsToken();
                // Extension methods can't be dynamically invoked :(
                var baal = parent?.AddNode(foo) ?? Tree.From(node.AsNode()!);
                */

                ;

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

            var tree = BuildSyntaxTree2(root) as Tree;
            AnsiConsole.Write(tree!);
        }
    }
    Console.WriteLine(getFileAtCommit("6c38e51", "./code-parser/Program.cs"));
    Console.WriteLine();
}

static string getFileAtCommit(string commitHash = "6c38e51", string filePath = "./code-parser/Program.cs")
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
