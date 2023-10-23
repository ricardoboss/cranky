using Buildalyzer;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Spectre.Console;
using Spectre.Console.Cli;

namespace Cranky.Commands;

internal sealed class AnalyzeCommand : AsyncCommand<AnalyzeCommand.Settings>
{
    internal sealed class Settings : CommandSettings
    {
        [CommandOption("-p|--project")]
        public FileInfo? ProjectFile { get; init; }

        [CommandOption("-s|--solution")]
        public FileInfo? SolutionFile { get; init; }
    }

    public override async Task<int> ExecuteAsync(CommandContext context, Settings settings)
    {
        var projectFile = settings.ProjectFile;
        var solutionFile = settings.SolutionFile;

        if (projectFile is null && solutionFile is null)
        {
            // TODO: use current directory to find project or solution file

            AnsiConsole.MarkupLine("[red]Error:[/] Either a project or solution file must be specified.");
            return 1;
        }

        if (projectFile is not null && solutionFile is not null)
        {
            AnsiConsole.MarkupLine("[red]Error:[/] Only one of a project or solution file can be specified.");
            return 1;
        }

        // TODO implement for solutions
        var projects = solutionFile is not null
            ? Array.Empty<FileSystemInfo>()
            : new[] { projectFile! };

        if (!projects.Any())
        {
            AnsiConsole.MarkupLine("[red]Error:[/] No solution or project file found.");
            return 1;
        }

        var analyzer = new Analyzer(projects);
        var result = await analyzer.AnalyzeAsync();

        var documented = result.Total - result.Undocumented;
        var pct = (double)documented / result.Total;
        var color = pct switch
        {
            >= 0.9 => "green",
            >= 0.5 => "yellow",
            _ => "red",
        };

        AnsiConsole.MarkupLine($"[bold]Documented API:[/] [bold {color}]{pct:P2}[/] ({documented}/{result.Total})");

        return 0;
    }
}

internal class Analyzer
{
    private readonly IEnumerable<FileSystemInfo> projects;

    public Analyzer(IEnumerable<FileSystemInfo> projects)
    {
        this.projects = projects;
    }

    public async Task<AggregationResults> AnalyzeAsync(CancellationToken cancellationToken = default)
    {
        var total = 0;
        var undocumented = 0;

        foreach (var project in projects)
        {
            foreach (var file in GetSourceFiles(project, cancellationToken))
            {
                if (!file.Exists)
                    continue;

                var result = await AnalyzeFileAsync(file, cancellationToken);

                total += result.PublicMembers.Count;
                undocumented += result.UndocumentedMembers.Count;
            }
        }

        return new(total, undocumented);
    }

    private static IEnumerable<FileSystemInfo> GetSourceFiles(FileSystemInfo projectFile, CancellationToken cancellationToken = default)
    {
        var manager = new AnalyzerManager();
        var analyzer = manager.GetProject(projectFile.FullName);
        var results = analyzer.Build();
        foreach (var result in results)
        {
            foreach (var sourceFile in result.SourceFiles)
            {
                cancellationToken.ThrowIfCancellationRequested();

                yield return new FileInfo(sourceFile);
            }
        }
    }

    private static async Task<AnalysisResult> AnalyzeFileAsync(FileSystemInfo file, CancellationToken cancellationToken = default)
    {
        // 1. parse source file
        var text = await File.ReadAllTextAsync(file.FullName, cancellationToken);
        var tree = CSharpSyntaxTree.ParseText(text, cancellationToken: cancellationToken);
        var root = tree.GetCompilationUnitRoot(cancellationToken);

        // 2. get public api
        var publicMembers = root.DescendantNodes()
            .OfType<MemberDeclarationSyntax>()
            .Where(m => m.Modifiers.Any(SyntaxKind.PublicKeyword))
            .ToList();

        // 3. get api documentation
        var publicMembersWithoutDocumentation = publicMembers
            .Where(m => !m.HasLeadingTrivia|| !m.GetLeadingTrivia().Any(t => t.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia)))
            .ToList();

        return new(publicMembers, publicMembersWithoutDocumentation);
    }
}

internal record AnalysisResult(IReadOnlyList<MemberDeclarationSyntax> PublicMembers, IReadOnlyList<MemberDeclarationSyntax> UndocumentedMembers);

internal record AggregationResults(int Total, int Undocumented);
