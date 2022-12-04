using Core.Model;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Core;

public class TestsGenerator {

    private CompilationUnitSyntax? _unitRoot;
    private List<UsingDirectiveSyntax>? _usings;
    private IEnumerable<ClassDeclarationSyntax>? _classes;

    public Dictionary<string, string> Generate(string source) {
        InitializeMetaData(source);
        return _classes!.ToDictionary(
            classSyntax => classSyntax.Identifier.Text + "Tests.cs", GetTestClassCode);
    }

    private void InitializeMetaData(string source) {
        _unitRoot = CSharpSyntaxTree.ParseText(source).GetCompilationUnitRoot();
        _usings = _unitRoot.DescendantNodes().OfType<UsingDirectiveSyntax>().ToList();
        _classes = _unitRoot.DescendantNodes().OfType<ClassDeclarationSyntax>()
            .Where(node => node.Modifiers.Any(n => n.IsKind(SyntaxKind.PublicKeyword)));
    }

    private string GetTestClassCode(ClassDeclarationSyntax classSyntax) {
        var classDeclaration = new ClassDeclaration(classSyntax);
        return CompilationUnit()
            .WithUsings(GenerateUsing())
            .AddMembers(classDeclaration.ClassDeclarationSyntax)
            .NormalizeWhitespace().ToFullString();
    }

    private SyntaxList<UsingDirectiveSyntax> GenerateUsing() {
        _usings!.Add(UsingDirective(
            QualifiedName(
                IdentifierName("NUnit"), 
                IdentifierName("Framework"))));
        return new SyntaxList<UsingDirectiveSyntax>(_usings);
    }
}