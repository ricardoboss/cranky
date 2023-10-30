using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Cranky;

internal record MemberAnalysisResult(IReadOnlyList<MemberDeclarationSyntax> PublicMembers, IReadOnlyList<MemberDeclarationSyntax> UndocumentedMembers);
