using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.Text;

namespace CodeParser.Helpers;

internal static class SyntaxHelper
{
    internal static Func<object, string> GetNodeColor = (object o) => o switch
    {
        SyntaxNode node => "green",
        SyntaxToken token => "blue",
        SyntaxTrivia trivia => "red",
        _ => "yellow"
    };

    internal static Func<object, SyntaxKind> GetKind = (object o) => o switch
    {
        SyntaxNode node => node.Kind(),
        SyntaxToken token => token.Kind(),
        SyntaxTrivia trivia => trivia.Kind(),
        _ => throw new ArgumentException("Unsupported type")
    };

    internal static Func<object, string> GetText = (object o) => o switch
    {
        SyntaxNode node => node.ToString(),
        SyntaxToken token => token.Text,
        SyntaxTrivia trivia => trivia.ToString(),
        _ => throw new ArgumentException("Unsupported type")
    };

    internal static Func<object, TextSpan> GetSpan = (object o) => o switch
    {
        SyntaxNode node => node.Span,
        SyntaxToken token => token.Span,
        SyntaxTrivia trivia => trivia.Span,
        _ => throw new ArgumentException("Unsupported type")
    };

    internal static Func<object, FileLinePositionSpan> GetLine = (object o) => o switch
    {
        SyntaxNode node => node.GetLocation().GetLineSpan(),
        SyntaxToken token => token.GetLocation().GetLineSpan(),
        SyntaxTrivia trivia => trivia.GetLocation().GetLineSpan(),
        _ => throw new ArgumentException("Unsupported type")
    };
}
