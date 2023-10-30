using Buildalyzer;
using Cranky.Output;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cranky;

internal class Analyzer
{
    private readonly IEnumerable<FileSystemInfo> projects;
    private readonly IOutput output;

    public Analyzer(IEnumerable<FileSystemInfo> projects, IOutput output)
    {
        this.projects = projects;
        this.output = output;
    }

    public async Task<AnalyzerResult> AnalyzeAsync(CancellationToken cancellationToken = default)
    {
        var total = 0;
        var undocumented = 0;

        foreach (var projectFile in projects)
        {
            output.WriteDebug("Analyzing project: " + projectFile.FullName);

            foreach (var file in GetSourceFiles(projectFile, cancellationToken))
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

    private async Task<MemberAnalysisResult> AnalyzeFileAsync(FileSystemInfo file, CancellationToken cancellationToken = default)
    {
        output.WriteDebug("Analyzing file: " + file.FullName);

        // 1. parse source file
        var text = await File.ReadAllTextAsync(file.FullName, cancellationToken);
        var tree = CSharpSyntaxTree.ParseText(text, cancellationToken: cancellationToken);
        var root = tree.GetCompilationUnitRoot(cancellationToken);

        // 2. get public api
        var publicMembers = root.DescendantNodes()
            .OfType<MemberDeclarationSyntax>()
            .Where(m => m.Modifiers.Any(SyntaxKind.PublicKeyword) || m.Modifiers.Any(SyntaxKind.ProtectedKeyword))
            .ToList();

        // 3. get api documentation
        var publicMembersWithoutDocumentation = publicMembers
            .Where(m => !m.HasLeadingTrivia|| !m.GetLeadingTrivia().Any(t => t.IsKind(SyntaxKind.SingleLineDocumentationCommentTrivia)))
            .ToList();

        return new(publicMembers, publicMembersWithoutDocumentation);
    }
}