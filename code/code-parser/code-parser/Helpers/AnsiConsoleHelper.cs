using CodeParser.Helpers;
using Microsoft.CodeAnalysis;

#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Spectre.Console.CodeParser.Helpers;
#pragma warning restore IDE0130 // Namespace does not match folder structure

internal enum TriviaType
{
    None,
    LeadingTrivia,
    TrailingTrivia,
}

internal static class AnsiConsoleHelper
{
    static readonly Func<object, string> GetSpanEscaped = o => Markup.Escape(SyntaxHelper.GetSpan(o).ToString());

    static readonly Func<object, string> GetTextEscaped = o => Markup.Escape(SyntaxHelper.GetText(o).Replace("\n", "\\n").Replace("\r", "\\r"));

    internal static string MakeConsoleTreeNode(object o, TriviaType triviaType = TriviaType.None)
    {
        var prefix = triviaType switch
        {
            TriviaType.LeadingTrivia => "Lead: ",
            TriviaType.TrailingTrivia => "Trail: ",
            _ => string.Empty,
        };
        var kind = SyntaxHelper.GetKind(o);
        var span = GetSpanEscaped(o);
        var line = SyntaxHelper.GetLine(o);
        var text = GetTextEscaped(o);
        var color = SyntaxHelper.GetNodeColor(o);
        return $"[{color}]{prefix}{kind}[/] Span: {span} line: {line} Text: [grey]'{text}'[/]";
    }

    extension(Tree)
    {
        /* // CS9282: This member is not allowed in an extension block
        // Implicit conversion operator is not supported as extension yet
        public static implicit operator Tree(SyntaxNode n) */
        public static Tree From(SyntaxNode n) => new(MakeConsoleTreeNode(n));
    }

    extension(IHasTreeNodes node)
    {
        public IHasTreeNodes AddNode(SyntaxNode n) => node.AddNode(MakeConsoleTreeNode(n));
        public IHasTreeNodes AddNode(SyntaxToken t) => node.AddNode(MakeConsoleTreeNode(t));
        public IHasTreeNodes AddNode(SyntaxTrivia t, TriviaType triviaType) => node.AddNode(MakeConsoleTreeNode(t, triviaType));
    }

    extension(SyntaxNode n)
    {
        public string ToConsoleString() => MakeConsoleTreeNode(n);
    }
}
