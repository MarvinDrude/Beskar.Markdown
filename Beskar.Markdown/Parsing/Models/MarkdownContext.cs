namespace Beskar.Markdown.Parsing.Models;

public sealed class MarkdownContext
{
   public Dictionary<string, int> ReferenceDefinitions { get; } = [];
}