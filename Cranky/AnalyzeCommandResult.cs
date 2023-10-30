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
                HealthIndicator.Success => $"https://img.shields.io/badge/Documentation%20Coverage-{pct}%25-green",
                _ => throw new ArgumentOutOfRangeException(),
            };
        }
    }
}
