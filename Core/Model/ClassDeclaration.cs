using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using static Microsoft.CodeAnalysis.CSharp.SyntaxFactory;

namespace Core.Model;

public class ClassDeclaration {
    public MemberDeclarationSyntax ClassDeclarationSyntax => _classDeclarationSyntax;
    
    private IReadOnlyList<MethodDeclaration> _methodDeclarations;
    private MemberDeclarationSyntax _classDeclarationSyntax;

    public ClassDeclaration(ClassDeclarationSyntax classDeclaration) {
        var methods = GetPublicMethods(classDeclaration);
        _methodDeclarations = GetMethodDeclarations(methods);
        _classDeclarationSyntax = GenerateClassDeclaration(classDeclaration);
    }

    private List<MethodDeclarationSyntax> GetPublicMethods(ClassDeclarationSyntax classDeclaration) {
        return classDeclaration.DescendantNodes().OfType<MethodDeclarationSyntax>()
            .Where(node => node.Modifiers.Any(n => n.IsKind(SyntaxKind.PublicKeyword))).ToList();
    }

    private List<MethodDeclaration> GetMethodDeclarations(List<MethodDeclarationSyntax> methods) {
        var groupedMethodNames = GroupMethodNames(methods);
        var uniqueNames = GenerateUniqueMethodNames(groupedMethodNames);
        return uniqueNames.Select(name => new MethodDeclaration(name)).ToList();
    }

    private Dictionary<string, int> GroupMethodNames(List<MethodDeclarationSyntax> methods) {
        var methodsNames = new Dictionary<string, int>();
        methods.ForEach(method => {
            var name = method.Identifier.Text;
            var newAmount = methodsNames.TryGetValue(name, out var amount) ? amount + 1 : 1;
            methodsNames[name] = newAmount;
        });
        return methodsNames;
    }

    private IEnumerable<string> GenerateUniqueMethodNames(Dictionary<string, int> methodNames) {
        return methodNames.SelectMany(entry => Enumerable.Range(0, entry.Value)
            .Select(i => entry.Value == 1 ? entry.Key + "Test" : entry.Key + (i + 1) + "Test")
            .ToArray()
        );
    }

    private MemberDeclarationSyntax GenerateClassDeclaration(ClassDeclarationSyntax classDeclaration) {
        var classDeclarationSyntax = ClassDeclaration(classDeclaration.Identifier.Text + "Test")
            .AddModifiers(Token(SyntaxKind.PublicKeyword))
            .WithMembers(GetMethodsDeclarationSyntax())
            .WithAttributeLists(SingletonList(GenerateTestFixtureAttributeSyntax()));
        return GenerateNamespaceHierarchy(classDeclaration, classDeclarationSyntax);
    }

    private MemberDeclarationSyntax GenerateNamespaceHierarchy(ClassDeclarationSyntax classDeclaration, ClassDeclarationSyntax generatedClassDeclaration) {
        var first = true;
        SyntaxNode current = classDeclaration;
        var currentSyntaxNode = current;
        while (current.Parent is NamespaceDeclarationSyntax outerNamespace) {
            var namespaceDeclaration = NamespaceDeclaration(first ? IdentifierName(outerNamespace.Name + ".Tests") : outerNamespace.Name)
                .WithMembers(first ? new SyntaxList<MemberDeclarationSyntax>(generatedClassDeclaration) : 
                    new SyntaxList<MemberDeclarationSyntax>((NamespaceDeclarationSyntax)currentSyntaxNode));
            first = false;
            currentSyntaxNode = namespaceDeclaration;
            current = current.Parent;
        }
        return (MemberDeclarationSyntax)currentSyntaxNode;
    }

    private SyntaxList<MemberDeclarationSyntax> GetMethodsDeclarationSyntax() {
        return new SyntaxList<MemberDeclarationSyntax>(
            _methodDeclarations.Select(m => m.MethodDeclarationSyntax).ToList());
    }

    private AttributeListSyntax GenerateTestFixtureAttributeSyntax() {
        return AttributeList(
            SingletonSeparatedList(
                Attribute(
                    IdentifierName("TestFixture"))));
    }
}