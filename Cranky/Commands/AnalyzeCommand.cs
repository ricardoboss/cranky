using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using Cranky.Output;
using Spectre.Console.Cli;

namespace Cranky.Commands;

[SuppressMessage("ReSharper", "ClassNeverInstantiated.Global")]
internal sealed class AnalyzeCommand : AsyncCommand<AnalyzeCommand.Settings>
{
    internal sealed class Settings : CommandSettings
    {
        [CommandOption("-p|--project")]
        public FileInfo? ProjectFile { get; init; }

        [CommandOption("-s|--solution")]
        public FileInfo? SolutionFile { get; init; }

        [CommandOption("--github")]
        public bool Github { get; init; }

        [CommandOption("--json")]
        public bool Json { get; init; }

        [CommandOption("--percentages")]
        public string Percentages { get; init; } = "50,90";

        [CommandOption("-e|--set-exit-code")]
        public bool SetExitCode { get; init; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        var projectFile = settings.ProjectFile;
        var solutionFile = settings.SolutionFile;

        using IOutput output = settings switch
        {
            { Github: true } => new GithubActionsOutput(),
            { Json: true } => new JsonOutput(),
            _ => new AnsiConsoleOutput(),
        };

        if (projectFile is null && solutionFile is null)
        {
            // TODO: use current directory to find project or solution file

            output.WriteError("Either a project or solution file must be specified.");

            return 1;
        }

        if (projectFile is not null && solutionFile is not null)
        {
            output.WriteError("Only one of a project or solution file can be specified.");

            return 1;
        }

        double minPct;
        double okPct;
        try
        {
            var percentages = settings.Percentages.Split(',').Select(int.Parse).ToArray();
            if (percentages.Length != 2)
            {
                output.WriteError("Percentages must be specified as two comma-separated integers.");

                return 1;
            }

            minPct = percentages[0] / 100.0;
            okPct = percentages[1] / 100.0;
        }
        catch (Exception ex)
        {
            output.WriteError(ex.Message);

            return 1;
        }

        output.WriteDebug($"Minimum documentation coverage: {minPct:P0}");
        output.WriteDebug($"Acceptable documentation coverage: {okPct:P0}");

        // TODO implement for solutions
        var projects = solutionFile is not null
            ? Array.Empty<FileSystemInfo>()
            : new[] { projectFile! };

        if (!projects.Any())
        {
            output.WriteError("No solution or project file found.");

            return 1;
        }

        var analyzer = new Analyzer(projects, output);

        output.WriteInfo("Analyzing projects...");

        var sw = Stopwatch.StartNew();
        var result = await analyzer.AnalyzeAsync();
        sw.Stop();

        output.WriteInfo($"Analyzed {result.Total} members in {sw.ElapsedMilliseconds}ms.");

        output.WriteInfo($"Documentation coverage: {result.DocumentedPercentage:P0} ({result.Documented}/{result.Total})");

        var exitCode = 0;
        HealthIndicator health;
        string message;

        if (result.DocumentedPercentage < minPct)
        {
            health = HealthIndicator.Error;
            message = "Documentation coverage is below minimum threshold ❌";

            exitCode = settings.SetExitCode ? 1 : 0;
        } else if (result.DocumentedPercentage < okPct)
        {
            health = HealthIndicator.Warning;
            message = "Documentation coverage is below acceptable threshold ⚠️";
        }
        else
        {
            health = HealthIndicator.Success;
            message = "Documentation coverage passed ✅";
        }

        output.WriteInfo(message);

        output.SetResult(new(result, health, message));

        return exitCode;
    }
}
