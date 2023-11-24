using Buildalyzer;
using Cranky.Output;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using NuGet.Frameworks;

namespace Cranky;

internal class Analyzer(IEnumerable<FileSystemInfo> files, IOutput output, bool buildLogging)
{
    public async Task<AnalyzerResult> AnalyzeAsync(CancellationToken cancellationToken = default)
    {
        var total = 0;
        var undocumented = 0;

        foreach (var projectFile in files)
        {
            var projectFilePath = projectFile.FullName.Replace("\\", "/").Replace("/", Path.DirectorySeparatorChar.ToString());

            output.WriteDebug("Analyzing project: " + projectFilePath);

            foreach (var file in GetSourceFiles(projectFilePath, cancellationToken))
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

    private IEnumerable<FileSystemInfo> GetSourceFiles(string projectFilePath, CancellationToken cancellationToken = default)
    {;
        // cause load of NuGet.Framework DLL
        _ = NuGetFramework.AnyFramework;

        var manager = new AnalyzerManager();
        var analyzer = manager.GetProject(projectFilePath);

        if (buildLogging)
            analyzer.AddBuildLogger(new BuildLogger(output));

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