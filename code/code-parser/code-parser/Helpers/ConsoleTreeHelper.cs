
using Spectre.Console;

namespace CodeParser.Helpers;

internal static class ConsoleTreeHelper
{
    static Func<object, string> GetSpanEscaped = (object o) => Markup.Escape(SyntaxHelper.GetSpan(o).ToString());

    static Func<object, string> GetTextEscaped = (object o) => Markup.Escape(SyntaxHelper.GetText(o).Replace("\n", "\\n").Replace("\r", "\\r"));

    internal static Func<object, string> MakeConsoleTreeNode = (object o) =>
    {
        var kind = SyntaxHelper.GetKind(o);
        var span = GetSpanEscaped(o);
        var line = SyntaxHelper.GetLine(o);
        var text = GetTextEscaped(o);
        var color = SyntaxHelper.GetNodeColor(o);
        return $"[{color}]{kind}[/] Span: {span} line: {line} Text: [grey]'{text}'[/]";
    };
}
