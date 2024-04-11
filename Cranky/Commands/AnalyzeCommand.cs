using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Text.RegularExpressions;
using ByteDev.DotNet.Solution;
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

        [CommandOption("-x|--exclude")]
        public string[]? ExcludedProjectsPatterns { get; init; }

        [CommandOption("--github")]
        public bool Github { get; init; }

        [CommandOption("--json")]
        public bool Json { get; init; }

        [CommandOption("--percentages")]
        public string Percentages { get; init; } = "50,90";

        [CommandOption("-e|--set-exit-code")]
        public bool SetExitCode { get; init; }

        [CommandOption("--debug")]
        public bool Debug { get; init; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        var projectFile = settings.ProjectFile;
        var solutionFile = settings.SolutionFile;
        var excludedProjectPatterns = settings.ExcludedProjectsPatterns;
        var debug = settings.Debug;

        using IOutput output = settings switch
        {
            { Github: true } => new GithubActionsOutput(),
            { Json: true } => new JsonOutput(),
            _ => new AnsiConsoleOutput(),
        };

        if (projectFile is null && solutionFile is null)
        {
            (solutionFile, projectFile) = TryFindSolutionOrProjectInCurrentDir();

            if (solutionFile is null && projectFile is null)
            {
                output.SetFailed("Either a project or solution file must be specified.");

                return 1;
            }
        }

        if (projectFile is not null && solutionFile is not null)
        {
            output.SetFailed("Only one of a project or solution file can be specified.");

            return 1;
        }

        double minPct;
        double okPct;
        try
        {
            var percentages = settings.Percentages.Split(',').Select(int.Parse).ToArray();
            if (percentages.Length != 2)
            {
                output.SetFailed("Percentages must be specified as two comma-separated integers.");

                return 1;
            }

            minPct = percentages[0] / 100.0;
            okPct = percentages[1] / 100.0;
        }
        catch (Exception ex)
        {
            output.SetFailed(ex.Message);

            return 1;
        }

        output.WriteDebug($"Minimum documentation coverage: {minPct:P0}");
        output.WriteDebug($"Acceptable documentation coverage: {okPct:P0}");

        var projectFiles = LoadProjects(output, solutionFile, projectFile, excludedProjectPatterns).ToList();
        if (projectFiles.Count == 0)
        {
            output.SetFailed("No solution or project file found.");

            return 1;
        }

        var analyzer = new Analyzer(projectFiles, output, debug);

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

    private static (FileInfo? solutionFile, FileInfo? projectFile) TryFindSolutionOrProjectInCurrentDir()
    {
        var currentDir = new DirectoryInfo(Environment.CurrentDirectory);

        var solutionFile = currentDir.EnumerateFiles("*.sln").FirstOrDefault();
        if (solutionFile is not null)
            return (solutionFile, null);

        var projectFile = currentDir.EnumerateFiles("*.csproj").FirstOrDefault();
        if (projectFile is not null)
            return (null, projectFile);

        return (null, null);
    }

    private static IEnumerable<FileSystemInfo> LoadProjects(IOutput output, FileSystemInfo? solutionFile, FileSystemInfo? projectFile, string[]? excludedProjectsPatterns)
    {
        IEnumerable<FileSystemInfo> projects;
        if (solutionFile is not null)
            projects = LoadProjectsFromSolution(output, solutionFile);
        else
        {
            Debug.Assert(projectFile is not null);

            output.WriteInfo($"Loading project: {projectFile.FullName}");

            projects = new[] { projectFile };
        }

        if (excludedProjectsPatterns is null)
        {
            foreach (var project in projects)
                yield return project;

            yield break;
        }

        var regexes = excludedProjectsPatterns.Select(p => new Regex("^" + Regex.Escape(p).Replace("\\*", ".*").Replace("\\?", ".") + "$")).ToArray();

        output.WriteDebug($"Applying {excludedProjectsPatterns.Length} project exclude pattern{(excludedProjectsPatterns.Length == 1 ? "" : "s")}:");
        foreach (var excludePattern in regexes)
            output.WriteDebug($"  {excludePattern}");

        foreach (var project in projects)
        {
            if (regexes.Any(r => r.IsMatch(project.FullName)))
                output.WriteDebug($"Excluding project: {project.FullName}");
            else
                yield return project;
        }
    }

    private static IEnumerable<FileSystemInfo> LoadProjectsFromSolution(IOutput output, FileSystemInfo solutionFile)
    {
        Guid[] csProjectTypes = [
            new("fae04ec0-301f-11d3-bf4b-00c04f79efbc"), // C# (.NET Framework)
            new("9a19103f-16f7-4668-be54-9a1e7a4f7556"), // C# (.NET Core)
            new("2eff6e4d-ff75-4adf-a9be-74bec0b0aff8"), // Class library
        ];

        output.OpenGroup($"Loading solution: {solutionFile.FullName}", "solution");

        var solution = DotNetSolution.Load(solutionFile.FullName);

        var projects = solution.Projects
            .Where(p => !p.Type.IsSolutionFolder)
            .Where(p => csProjectTypes.Contains(p.Type.Id))
            .ToList();

        output.WriteDebug($"Found {projects.Count} projects in solution:");
        foreach (var project in projects)
            output.WriteDebug($"  {project.Path}");

        output.CloseGroup("solution");

        return projects.Select(p => new FileInfo(p.Path));
    }
}
