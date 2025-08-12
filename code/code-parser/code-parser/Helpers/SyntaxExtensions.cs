#pragma warning disable IDE0130 // Namespace does not match folder structure
namespace Microsoft.CodeAnalysis.CSharp.Helpers;
#pragma warning restore IDE0130 // Namespace does not match folder structure

// See https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-14#extension-members
// and https://learn.microsoft.com/en-us/dotnet/csharp/programming-guide/classes-and-structs/extension-methods
static class SyntaxNodeOrTokenExtensions
{
    extension(SyntaxNodeOrToken node)
    {
        public SyntaxKind Kind => node.Kind();
    }
}
