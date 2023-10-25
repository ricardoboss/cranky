namespace Cranky.Output;

public interface IOutput : IDisposable
{
    public void WriteError(string message, string? file = null, int? line = null, int? col = null, int? endLine = null, int? endColumn = null, string? title = null);

    public void WriteWarning(string message, string? file = null, int? line = null, int? col = null, int? endLine = null, int? endColumn = null, string? title = null);

    public void WriteNotice(string message, string? file = null, int? line = null, int? col = null, int? endLine = null, int? endColumn = null, string? title = null);

    public void WriteInfo(string message);

    public void WriteDebug(string message);

    public void SetResult(AggregationResults result);
}
