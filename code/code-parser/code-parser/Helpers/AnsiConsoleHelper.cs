using Microsoft.CodeAnalysis;
using static CodeParser.Helpers.SyntaxHelper;

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
    static readonly Func<object, string> GetSpanEscaped = o => Markup.Escape(GetSpan(o).ToString());

    static readonly Func<object, string> GetTextEscaped = o => Markup.Escape(GetText(o).Replace("\n", "\\n").Replace("\r", "\\r"));

    internal static string MakeConsoleTreeNode(object o, TriviaType triviaType = TriviaType.None)
    {
        var prefix = triviaType switch
        {
            TriviaType.LeadingTrivia => "Lead: ",
            TriviaType.TrailingTrivia => "Trail: ",
            _ => string.Empty,
        };
        var kind = GetKind(o);
        var span = GetSpanEscaped(o);
        var line = GetLine(o);
        var text = GetTextEscaped(o);
        var color = GetNodeColor(o);
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

    extension(SyntaxNodeOrToken node)
    {
        public IHasTreeNodes BuildSyntaxTree(IHasTreeNodes? parent = null)
        {
            // Create the tree node with the kind of the syntax node
            var currentParent = (node, parent) switch
            {
                (_, null) => Tree.From(node.AsNode()!),
                ({ IsNode: true }, IHasTreeNodes p) => p.AddNode(node.AsNode()!),
                ({ IsNode: false }, IHasTreeNodes p) => p.AddNode(node.AsToken()),
            };

            // Recursively process children
            if (node.IsNode)
            {
                foreach (var child in node.ChildNodesAndTokens())
                {
                    child.BuildSyntaxTree(currentParent);
                }
            }
            else
            {
                var token = node.AsToken();
                foreach (var trivia in token.LeadingTrivia)
                {
                    currentParent.AddNode(trivia, TriviaType.LeadingTrivia);
                }
                foreach (var trivia in token.TrailingTrivia)
                {
                    currentParent.AddNode(trivia, TriviaType.TrailingTrivia);
                }
            }

            return currentParent;
        }
    }
}
