using System.Text;

namespace Cranky.Output;

public class GithubActionsOutput : IOutput
{
    private readonly StringBuilder sb = new();

    private void Write(string verb, string message, string? file = null, int? line = null, int? col = null, int? endLine = null,
        int? endColumn = null, string? title = null)
    {
        // ::{verb} file={file},line={line},col={col},endLine={endLine},endColumn={endColumn},title={title}::{message}

        sb.Clear();

        sb.Append("::");
        sb.Append(verb);

        var args = new List<string>();
        if (file is not null)
            args.Add($"file={file}");
        if (line is not null)
            args.Add($"line={line}");
        if (col is not null)
            args.Add($"col={col}");
        if (endLine is not null)
            args.Add($"endLine={endLine}");
        if (endColumn is not null)
            args.Add($"endColumn={endColumn}");
        if (title is not null)
            args.Add($"title={title}");

        if (args.Any())
        {
            sb.Append(' ');
            sb.Append(string.Join(',', args));
        }

        sb.Append("::");
        sb.Append(message);

        Console.WriteLine(sb.ToString());
    }

    public void WriteError(string message, string? file = null, int? line = null, int? col = null, int? endLine = null,
        int? endColumn = null, string? code = null)
    {
        Write("error", message, file, line, col, endLine, endColumn, code);
    }

    public void WriteWarning(string message, string? file = null, int? line = null, int? col = null, int? endLine = null,
        int? endColumn = null, string? code = null)
    {
        Write("warning", message, file, line, col, endLine, endColumn, code);
    }

    public void WriteInfo(string message)
    {
        Console.WriteLine(message);
    }

    public void WriteDebug(string message)
    {
        Write("debug", message);
    }

    public void SetResult(AnalyzeCommandResult result)
    {
        var envFile = Environment.GetEnvironmentVariable("GITHUB_OUTPUT");
        if (envFile is not null)
        {
            using var writer = new StreamWriter(envFile, append: true, Encoding.UTF8);
            writer.WriteLine($"total={result.AnalyzerResult.Total}");
            writer.WriteLine($"undocumented={result.AnalyzerResult.Undocumented}");
            writer.WriteLine($"documented={result.AnalyzerResult.Documented}");
            writer.WriteLine($"percent={result.AnalyzerResult.DocumentedPercentageDisplay}");
            writer.WriteLine($"health={result.HealthEmoji}");
            writer.WriteLine($"badge={result.Badge}");
            writer.WriteLine($"message={result.Message}");
            writer.Flush();
        }
        else
        {
            // fall back to setting output via stdout
            Console.WriteLine($"::set-output name=total::{result.AnalyzerResult.Total}");
            Console.WriteLine($"::set-output name=undocumented::{result.AnalyzerResult.Undocumented}");
            Console.WriteLine($"::set-output name=documented::{result.AnalyzerResult.Documented}");
            Console.WriteLine($"::set-output name=percent::{result.AnalyzerResult.DocumentedPercentageDisplay}");
            Console.WriteLine($"::set-output name=health::{result.HealthEmoji}");
            Console.WriteLine($"::set-output name=badge::{result.Badge}");
            Console.WriteLine($"::set-output name=message::{result.Message}");
        }

        var summary = $"""
                       ![Documentation coverage {result.AnalyzerResult.DocumentedPercentageDisplay}%]({result.Badge})
                       
                       {result.Message}
                       """;

        Environment.SetEnvironmentVariable("GITHUB_STEP_SUMMARY", summary);
    }

    public void OpenGroup(string title, string? key = null)
    {
        Write("group", title);
    }

    public void CloseGroup(string? key = null)
    {
        Write("endgroup", "");
    }

    public void SetProgress(int total, int current, string? message = null)
    {
        // GitHub Actions does not support progress reporting
        // To not spam the log, we ignore this
    }

    public void Dispose()
    {
    }
}
