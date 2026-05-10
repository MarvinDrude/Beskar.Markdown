using Beskar.Markdown.Parsing.Interfaces;

namespace Beskar.Markdown.Addons;

public abstract class BaseInlineExtension 
   : BaseMarkdownExtension
{
   public IInlineParser[] Parsers { get; set; } = [];
}