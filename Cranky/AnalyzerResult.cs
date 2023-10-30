namespace Cranky;

public record AnalyzerResult(int Total, int Undocumented)
{
    public int Documented => Total - Undocumented;

    public double DocumentedPercentage => (double)Documented / Total;

    public int DocumentedPercentageDisplay => (int)(DocumentedPercentage * 100.0);
}
