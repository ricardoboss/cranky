using Spectre.Console;

namespace Cranky.Output;

public static class OutputExtensions
{
    public static void WriteErrorEscaped(this IOutput output, string message, string? file = null, int? line = null, int? col = null, int? endLine = null,
        int? endColumn = null, string? code = null)
    {
        if (output is AnsiConsoleOutput)
            message = message.EscapeMarkup();

        output.WriteError(message, file, line, col, endLine, endColumn, code);
    }

    public static void WriteDebugEscaped(this IOutput output, string message)
    {
        if (output is AnsiConsoleOutput)
            message = message.EscapeMarkup();

        output.WriteDebug(message);
    }

    public static void WriteInfoEscaped(this IOutput output, string message)
    {
        if (output is AnsiConsoleOutput)
            message = message.EscapeMarkup();

        output.WriteInfo(message);
    }

    public static void WriteWarningEscaped(this IOutput output, string message, string? file = null, int? line = null, int? col = null, int? endLine = null,
        int? endColumn = null, string? code = null)
    {
        if (output is AnsiConsoleOutput)
            message = message.EscapeMarkup();

        output.WriteWarning(message, file, line, col, endLine, endColumn, code);
    }
}
