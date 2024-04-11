using Cranky.Output;
using Microsoft.Build.Framework;

namespace Cranky;

public class BuildLogger(IOutput output) : ILogger
{
    /// <inheritdoc />
    public LoggerVerbosity Verbosity { get; set; } = LoggerVerbosity.Normal;

    /// <inheritdoc />
    public string Parameters { get; set; } = string.Empty;

    /// <inheritdoc />
    public void Initialize(IEventSource eventSource)
    {
        eventSource.MessageRaised += OnMessageRaised;
        eventSource.ErrorRaised += OnErrorRaised;
        eventSource.WarningRaised += OnWarningRaised;
        eventSource.BuildFinished += OnBuildFinished;
        eventSource.BuildStarted += OnBuildStarted;
    }

    private void OnBuildStarted(object sender, BuildStartedEventArgs e)
    {
        output.WriteInfoEscaped("Build started.");
    }

    private void OnBuildFinished(object sender, BuildFinishedEventArgs e)
    {
        output.WriteInfoEscaped("Build finished.");
    }

    private void OnWarningRaised(object sender, BuildWarningEventArgs e)
    {
        output.WriteWarningEscaped(e.Message, e.File, e.LineNumber, e.ColumnNumber, e.EndLineNumber, e.EndColumnNumber, e.Code);
    }

    private void OnErrorRaised(object sender, BuildErrorEventArgs e)
    {
        output.WriteErrorEscaped(e.Message);
    }

    private void OnMessageRaised(object sender, BuildMessageEventArgs e)
    {
        output.WriteInfoEscaped(e.Message);
    }

    /// <inheritdoc />
    public void Shutdown()
    {
        // do nothing
    }
}
