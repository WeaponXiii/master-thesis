#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace CodeParser.Helpers;
#pragma warning restore IDE0130 // Namespace does not match folder structure

public static class GitHelper
{
    public static string GetFileAtCommit(
    string commitHash = "6c38e51",
    string filePath = "code/code-parser/code-parser/Program.cs")
    {
        Console.WriteLine($"Getting file '{filePath}' at commit '{commitHash}'");

        var cmd = $"git show {commitHash}:{filePath} | cat";

        return BashHelper.RunBashCommand(cmd);
    }
}
