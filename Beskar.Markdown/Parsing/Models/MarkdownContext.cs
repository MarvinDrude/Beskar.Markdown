namespace Beskar.Markdown.Parsing.Models;

public sealed class MarkdownContext<TData>
{
   public Dictionary<string, int> ReferenceDefinitions { get; } = [];

   public Dictionary<string, string> FrontMatter => field ??= [];

   public Dictionary<string, string> SlugToPlainText => field ??= [];

   public TData? Data { get; set; }
}