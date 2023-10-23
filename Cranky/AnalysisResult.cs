using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cranky;

internal record AnalysisResult(IReadOnlyList<MemberDeclarationSyntax> PublicMembers, IReadOnlyList<MemberDeclarationSyntax> UndocumentedMembers);
