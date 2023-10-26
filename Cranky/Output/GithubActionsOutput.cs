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
        int? endColumn = null, string? title = null)
    {
        Write("error", message, file, line, col, endLine, endColumn, title);
    }

    public void WriteWarning(string message, string? file = null, int? line = null, int? col = null, int? endLine = null,
        int? endColumn = null, string? title = null)
    {
        Write("warning", message, file, line, col, endLine, endColumn, title);
    }

    public void WriteNotice(string message, string? file = null, int? line = null, int? col = null, int? endLine = null,
        int? endColumn = null, string? title = null)
    {
        Write("notice", message, file, line, col, endLine, endColumn, title);
    }

    public void WriteInfo(string message)
    {
        Console.WriteLine(message);
    }

    public void WriteDebug(string message)
    {
        Write("debug", message);
    }

    public void SetResult(AggregationResults result)
    {
        var envFile = Environment.GetEnvironmentVariable("GITHUB_OUTPUT");
        if (envFile is not null)
        {
            using var writer = new StreamWriter(envFile, append: true, Encoding.UTF8);
            writer.WriteLine($"total={result.Total}");
            writer.WriteLine($"undocumented={result.Undocumented}");
            writer.WriteLine($"documented={result.Documented}");
            writer.WriteLine($"percent={result.DocumentedPercentageDisplay}");
            writer.Flush();
        }
        else
        {
            // fall back to setting output via stdout
            Console.WriteLine($"::set-output name=total::{result.Total}");
            Console.WriteLine($"::set-output name=undocumented::{result.Undocumented}");
            Console.WriteLine($"::set-output name=documented::{result.Documented}");
            Console.WriteLine($"::set-output name=percent::{result.DocumentedPercentageDisplay}");
        }
    }

    public void Dispose()
    {
    }
}