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
        public string? Code { get; set; }
        public string? Key { get; set; }
    }

    private class OutputAggregate
    {
        public List<JsonMessage> Messages { get; } = new();
        public Dictionary<string, dynamic>? Result { get; set; }
    }

    private readonly OutputAggregate aggregate = new();

    public void WriteError(string message, string? file = null, int? line = null, int? col = null, int? endLine = null,
        int? endColumn = null, string? code = null)
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
            Code = code,
        });
    }

    public void WriteWarning(string message, string? file = null, int? line = null, int? col = null, int? endLine = null,
        int? endColumn = null, string? code = null)
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
            Code = code,
        });
    }

    public void WriteInfo(string message, string? file = null, int? line = null, int? col = null, int? endLine = null,
        int? endColumn = null, string? code = null)
    {
        aggregate.Messages.Add(new()
        {
            Type = "info",
            Message = message,
            File = file,
            Line = line,
            Col = col,
            EndLine = endLine,
            EndColumn = endColumn,
            Code = code,
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
        aggregate.Result = result.ToJson();
    }

    public void OpenGroup(string title, string? key = null)
    {
        aggregate.Messages.Add(new()
        {
            Type = "group",
            Message = title,
            Key = key,
        });
    }

    public void CloseGroup(string? key = null)
    {
        aggregate.Messages.Add(new()
        {
            Type = "endgroup",
            Key = key,
        });
    }

    public void SetProgress(int total, int current, string? message = null)
    {
        // Ignore for JSON output as it isn't logged in a streaming fashion.
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
