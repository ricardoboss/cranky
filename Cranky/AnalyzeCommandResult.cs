namespace Cranky;

public record AnalyzeCommandResult(AnalyzerResult AnalyzerResult, HealthIndicator Health, string Message)
{
    public string Badge
    {
        get
        {
            var pct = AnalyzerResult.DocumentedPercentageDisplay;
            return Health switch
            {
                HealthIndicator.Error => $"https://img.shields.io/badge/Documentation%20Coverage-{pct}%25-red",
                HealthIndicator.Warning => $"https://img.shields.io/badge/Documentation%20Coverage-{pct}%25-yellow",
                HealthIndicator.Success => $"https://img.shields.io/badge/Documentation%20Coverage-{pct}%25-brightgreen",
                _ => throw new ArgumentOutOfRangeException(),
            };
        }
    }

    public string HealthEmoji => Health switch
    {
        HealthIndicator.Error => "❌",
        HealthIndicator.Warning => "⚠️",
        HealthIndicator.Success => "✅",
        _ => throw new ArgumentOutOfRangeException(),
    };

    public Dictionary<string, dynamic> ToJson()
    {
        return new()
        {
            { "total", AnalyzerResult.Total },
            { "documented", AnalyzerResult.Documented },
            { "undocumented", AnalyzerResult.Undocumented },
            { "percent", AnalyzerResult.DocumentedPercentageDisplay },
            { "health", HealthEmoji },
            { "message", Message },
            { "badge", Badge },
        };
    }
}
