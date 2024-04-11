using System.Globalization;
using System.Text;

namespace Cranky.Output;

public class AzurePipelinesOutput : IOutput
{
    private readonly StringBuilder sb = new();

    private void WriteMessage(string? level, string message)
    {
        sb.Clear();

        if (level is not null)
        {
            sb.Append("##[");
            sb.Append(level);
            sb.Append(']');
        }

        sb.Append(message);

        Console.WriteLine(sb.ToString());
    }

    private void WriteIssue(string type, string message, string? sourcePath, int? lineNumber, int? columnNumber,
        string? code)
    {
        sb.Clear();

        sb.Append("##vso[task.logissue type=");
        sb.Append(type);
        sb.Append(';');

        if (sourcePath is not null)
        {
            sb.Append("sourcepath=");
            sb.Append(sourcePath);
            sb.Append(';');
        }

        if (lineNumber is not null)
        {
            sb.Append("linenumber=");
            sb.Append(lineNumber);
            sb.Append(';');
        }

        if (columnNumber is not null)
        {
            sb.Append("columnnumber=");
            sb.Append(columnNumber);
            sb.Append(';');
        }

        if (code is not null)
        {
            sb.Append("code=");
            sb.Append(code);
            sb.Append(';');
        }

        sb.Append(']');
        sb.Append(message);

        Console.WriteLine(sb.ToString());
    }

    public void WriteError(string message, string? file = null, int? line = null, int? col = null, int? endLine = null,
        int? endColumn = null, string? code = null)
    {
        WriteIssue("error", message, file, line, col, code);
    }

    public void WriteWarning(string message, string? file = null, int? line = null, int? col = null, int? endLine = null,
        int? endColumn = null, string? code = null)
    {
        WriteIssue("warning", message, file, line, col, code);
    }

    public void WriteInfo(string message, string? file = null, int? line = null, int? col = null, int? endLine = null,
        int? endColumn = null, string? code = null)
    {
        WriteMessage(null, message);
    }

    public void WriteDebug(string message)
    {
        WriteMessage("debug", message);
    }

    public void SetResult(AnalyzeCommandResult result)
    {
        SetVariable("total", result.AnalyzerResult.Total.ToString(CultureInfo.InvariantCulture));
        SetVariable("undocumented", result.AnalyzerResult.Undocumented.ToString(CultureInfo.InvariantCulture));
        SetVariable("documented", result.AnalyzerResult.Documented.ToString(CultureInfo.InvariantCulture));
        SetVariable("percent", result.AnalyzerResult.DocumentedPercentageDisplay.ToString(CultureInfo.InvariantCulture));
        SetVariable("health", result.HealthEmoji);
        SetVariable("badge", result.Badge);
        SetVariable("message", result.Message);

        var taskResult = result.Health switch
        {
            HealthIndicator.Success => "Succeeded",
            HealthIndicator.Warning => "SucceededWithIssues",
            _ => "Failed",
        };

        Console.WriteLine($"##vso[task.complete result={taskResult};]{result.Message}");

        return;

        void SetVariable(string name, string value)
        {
            Console.WriteLine($"##vso[task.setvariable variable={name};isoutput=true;isreadonly=true]{value}");
        }
    }

    public void SetFailed(string? message = null)
    {
        Console.WriteLine($"##vso[task.complete result=Failed;]{message}");
    }

    public void OpenGroup(string title, string? key = null)
    {
        WriteMessage("group", title);
    }

    public void CloseGroup(string? key = null)
    {
        WriteMessage("endgroup", "");
    }

    public void SetProgress(int total, int current, string? message = null)
    {
        sb.Clear();

        int percent;
        if (total == 0)
            percent = 100;
        else
            percent = (int)((double)current / total * 100);

        sb.Append("##vso[task.setprogress value=");
        sb.Append(percent);
        sb.Append(";]");

        if (message is not null)
        {
            sb.Append(message);
        }

        Console.WriteLine(sb.ToString());
    }

    public void Dispose()
    {
    }
}
