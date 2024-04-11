using Spectre.Console;

namespace Cranky.Output;

public class AnsiConsoleOutput : IOutput
{
    private int indent;

    private void WriteIndent()
    {
        if (indent > 0)
            AnsiConsole.Write(new string(' ', indent * 2));
    }

    public void WriteError(string message, string? file = null, int? line = null, int? col = null, int? endLine = null,
        int? endColumn = null, string? code = null)
    {
        WriteIndent();
        AnsiConsole.MarkupLine("[red]Error:[/] {0}", message);
    }

    public void WriteWarning(string message, string? file = null, int? line = null, int? col = null, int? endLine = null,
        int? endColumn = null, string? code = null)
    {
        WriteIndent();
        AnsiConsole.MarkupLine("[yellow]Warning:[/] {0}", message);
    }

    public void WriteInfo(string message)
    {
        WriteIndent();
        AnsiConsole.MarkupLine("[blue]Info:[/] {0}", message);
    }

    public void WriteDebug(string message)
    {
        WriteIndent();
        AnsiConsole.MarkupLine("[grey]Debug:[/] {0}", message);
    }

    public void SetResult(AnalyzeCommandResult result)
    {
    }

    public void OpenGroup(string title, string? key = null)
    {
        AnsiConsole.MarkupLine("[bold]{0}[/]", title);
        indent++;
    }

    public void CloseGroup(string? key = null)
    {
        AnsiConsole.WriteLine();
        indent--;
    }

    public void SetProgress(int total, int current, string? message = null)
    {
        // TODO: Implement progress reporting via Spectre.Console
    }

    public void Dispose()
    {
    }
}
