using System.Diagnostics;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace CodeParser.Helpers;
#pragma warning restore IDE0130 // Namespace does not match folder structure
public static class BashHelper
{
    public static string RunBashCommand(string command)
    {
        var escapedArgs = command.Replace("\"", "\\\"");
        var startInfo = new ProcessStartInfo
        {
            FileName = "/bin/bash",
            Arguments = $"-c \"{escapedArgs}\"",
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true,
        };

        using var p = new Process
        {
            StartInfo = startInfo
        };

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
}
