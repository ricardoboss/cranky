using System.Text.Json;
using System.Text.Json.Serialization;

namespace Cranky.Output;

public class JsonOutput : IOutput
{
    private class JsonMessage
    {
        public string Type { get; set; } = "";
        public string Message { get; set; } = "";
        public string? File { get; set; }
        public int? Line { get; set; }
        public int? Col { get; set; }
        public int? EndLine { get; set; }
        public int? EndColumn { get; set; }
        public string? Title { get; set; }
    }

    private class OutputAggregate
    {
        public List<JsonMessage> Messages { get; } = new();
        public AnalyzeCommandResult? Result { get; set; }
    }

    private readonly OutputAggregate aggregate = new();

    public void WriteError(string message, string? file = null, int? line = null, int? col = null, int? endLine = null,
        int? endColumn = null, string? title = null)
    {
        aggregate.Messages.Add(new()
        {
            Type = "error",
            Message = message,
            File = file,
            Line = line,
            Col = col,
            EndLine = endLine,
            EndColumn = endColumn,
            Title = title,
        });
    }

    public void WriteWarning(string message, string? file = null, int? line = null, int? col = null, int? endLine = null,
        int? endColumn = null, string? title = null)
    {
        aggregate.Messages.Add(new()
        {
            Type = "warning",
            Message = message,
            File = file,
            Line = line,
            Col = col,
            EndLine = endLine,
            EndColumn = endColumn,
            Title = title,
        });
    }

    public void WriteNotice(string message, string? file = null, int? line = null, int? col = null, int? endLine = null,
        int? endColumn = null, string? title = null)
    {
        aggregate.Messages.Add(new()
        {
            Type = "notice",
            Message = message,
            File = file,
            Line = line,
            Col = col,
            EndLine = endLine,
            EndColumn = endColumn,
            Title = title,
        });
    }

    public void WriteInfo(string message)
    {
        aggregate.Messages.Add(new()
        {
            Type = "info",
            Message = message,
        });
    }

    public void WriteDebug(string message)
    {
        aggregate.Messages.Add(new()
        {
            Type = "debug",
            Message = message,
        });
    }

    public void SetResult(AnalyzeCommandResult result)
    {
        aggregate.Result = result;
    }

    public void Dispose()
    {
        var json = JsonSerializer.Serialize(aggregate, new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingDefault,
        });

        Console.WriteLine(json);
    }
}