using Core;
using Core.Model;
using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;

namespace Tests;

public class Tests {
    
    private string _file;
    private TestsGenerator _generator;
    private List<GeneratedTestClass> _testClasses;
    private CompilationUnitSyntax _parsedTestClass;

    [OneTimeSetUp]
    public void Initialize() {
        _generator = new TestsGenerator();
        _file = File.ReadAllText(@"../../../Resources/MyClass.cs");
        _testClasses = _generator.Generate(_file);
        _parsedTestClass = CSharpSyntaxTree.ParseText(_testClasses[0].ClassCode).GetCompilationUnitRoot();
    }
    
    [Test]
    public void Test_ClassesAmount() {
        Assert.That(_testClasses, Has.Count.EqualTo(1));
        
        var parsedClassesAmount = _parsedTestClass.DescendantNodes().OfType<ClassDeclarationSyntax>().Count();
        Assert.That(parsedClassesAmount, Is.EqualTo(_testClasses.Count));
    }

    [Test]
    public void Test_MethodsSyntaxCorrectness() {
        var parsedMethods = _parsedTestClass.DescendantNodes().OfType<MethodDeclarationSyntax>().ToList();
        
        parsedMethods.ForEach(method => Assert.Multiple(() => {
            Assert.That(method.AttributeLists, Has.Count.EqualTo(1));
            Assert.That(method.AttributeLists.ToString(), Does.Contain("[Test]"));
            Assert.That(method.Modifiers.Any(SyntaxKind.PublicKeyword), Is.True);
            Assert.That(method.ReturnType.ToString(), Is.EqualTo("void"));
            Assert.That(method.Body!.Statements.ToString(), Is.EqualTo("Assert.Fail(\"autogenerated\");"));
        }));
    }

    [Test]
    public void Test_AllPublicMethodsGenerated() {
        var parsedMethodsAmount = _parsedTestClass.DescendantNodes().OfType<MethodDeclarationSyntax>().Count();
        
        Assert.That(parsedMethodsAmount, Is.EqualTo(4));
    }

    [Test]
    public void Test_OverloadedMethodsHaveDifferentNames() {
        var parsedOverloadedMethodsAmount = _parsedTestClass.DescendantNodes().OfType<MethodDeclarationSyntax>()
            .Count(method => method.Identifier.Text.StartsWith("ThirdMethod"));
        
        Assert.That(parsedOverloadedMethodsAmount, Is.EqualTo(2));
    }

    [Test]
    public void Test_AllNeededUsingsGenerated() {
        var neededUsings = new List<string>() { "System", "System.Collections.Generic", "NUnit.Framework" };
        var parsedUsings = _parsedTestClass.DescendantNodes().OfType<UsingDirectiveSyntax>()
            .Select(u => u.Name.ToString()).ToList();
       
        neededUsings.ForEach(neededUsing => Assert.That(parsedUsings, Does.Contain(neededUsing)));
    }

    [Test]
    public void Test_AllNamespacesGenerated() {
        var neededNamespaces = new List<string>() { "Jora", "Jira.Tests" };
        var parsedNamespaces = _parsedTestClass.DescendantNodes().OfType<NamespaceDeclarationSyntax>()
            .Select(n => n.Name.ToString()).ToList();
       
        neededNamespaces.ForEach(neededNamespace => Assert.That(parsedNamespaces, Does.Contain(neededNamespace)));
    }
}