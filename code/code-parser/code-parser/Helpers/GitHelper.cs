using System.Diagnostics;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace CodeParser.Helpers;
#pragma warning restore IDE0130 // Namespace does not match folder structure

public static class GitHelper
{
    public static string GetFileAtCommit(
    string commitHash = "6c38e51",
    string filePath = "code/code-parser/code-parser/Program.cs")
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
}
