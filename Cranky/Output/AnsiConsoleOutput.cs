using Spectre.Console;

namespace Cranky.Output;

public class AnsiConsoleOutput : IOutput
{
    public void WriteError(string message, string? file = null, int? line = null, int? col = null, int? endLine = null,
        int? endColumn = null, string? title = null)
    {
        AnsiConsole.MarkupLine("[red]Error:[/] {0}", message);
    }

    public void WriteWarning(string message, string? file = null, int? line = null, int? col = null, int? endLine = null,
        int? endColumn = null, string? title = null)
    {
        AnsiConsole.MarkupLine("[yellow]Warning:[/] {0}", message);
    }

    public void WriteNotice(string message, string? file = null, int? line = null, int? col = null, int? endLine = null,
        int? endColumn = null, string? title = null)
    {
        AnsiConsole.MarkupLine("[bold blue]Notice:[/] {0}", message);
    }

    public void WriteInfo(string message)
    {
        AnsiConsole.MarkupLine("[blue]Info:[/] {0}", message);
    }

    public void WriteDebug(string message)
    {
        AnsiConsole.MarkupLine("[grey]Debug:[/] {0}", message);
    }

    public void SetResult(AnalyzeCommandResult result)
    {
    }

    public void Dispose()
    {
    }
}