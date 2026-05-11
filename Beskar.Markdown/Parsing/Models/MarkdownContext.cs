namespace Beskar.Markdown.Parsing.Models;

public sealed class MarkdownContext<TData>
{
   public Dictionary<string, int> ReferenceDefinitions { get; } = [];
   
   public TData? Data { get; set; }
}