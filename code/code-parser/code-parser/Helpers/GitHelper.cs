#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace CodeParser.Helpers;
#pragma warning restore IDE0130 // Namespace does not match folder structure

using InteropServices = System.Runtime.InteropServices;

public static class GitHelper
{
    public static string GetFileAtCommit(
    string commitHash = "6c38e51",
    string filePath = "code/code-parser/code-parser/Program.cs")
    {
        Console.WriteLine($"Getting file '{filePath}' at commit '{commitHash}'");
        var cmdPostfix = InteropServices.RuntimeInformation.IsOSPlatform(InteropServices.OSPlatform.Linux)
            ? " | cat"
            : "";

        var cmd = $"git show {commitHash}:{filePath}{cmdPostfix}";        

        return ShellRunner.Execute(cmd);
    }
}
