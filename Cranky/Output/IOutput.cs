namespace Cranky.Output;

public interface IOutput : IDisposable
{
    public void WriteError(string message, string? file = null, int? line = null, int? col = null, int? endLine = null, int? endColumn = null, string? code = null);

    public void WriteWarning(string message, string? file = null, int? line = null, int? col = null, int? endLine = null, int? endColumn = null, string? code = null);

    public void WriteInfo(string message, string? file = null, int? line = null, int? col = null, int? endLine = null, int? endColumn = null, string? code = null);

    public void WriteDebug(string message);

    public void SetResult(AnalyzeCommandResult result);

    public void SetFailed(string message) => WriteError(message);

    public void OpenGroup(string title, string? key = null);

    public void CloseGroup(string? key = null);

    public void SetProgress(int total, int current, string? message = null);
}
