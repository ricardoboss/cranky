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
        var (total, undocumented) = result;
        var percent = total == 0 ? 0 : (int) Math.Round(undocumented / (double) total * 100);

        Console.WriteLine($"::set-output name=total::{total}");
        Console.WriteLine($"::set-output name=undocumented::{undocumented}");
        Console.WriteLine($"::set-output name=percent::{percent}");
    }

    public void Dispose()
    {
    }
}