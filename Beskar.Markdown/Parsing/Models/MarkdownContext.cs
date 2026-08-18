namespace Beskar.Markdown.Parsing.Models;

public sealed class MarkdownContext<TData>
{
   public Dictionary<string, int> ReferenceDefinitions => field ??= [];

   public Dictionary<string, string> FrontMatter => field ??= [];

   public Dictionary<string, string> SlugToPlainText => field ??= [];

   public List<HeaderInfo> Headers => field ??= [];
   
   public Dictionary<string, string> Variables => field ??= [];

   public Func<string, string?>? VariableResolver { get; set; }

   public TData? Data { get; set; }
}

public record HeaderInfo(string Slug, string PlainText, int Level);
