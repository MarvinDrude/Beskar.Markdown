using Beskar.Markdown.Parsing.Interfaces;

namespace Beskar.Markdown.Addons;

public abstract class BaseBlockExtension
   : BaseMarkdownExtension
{
   public IBlockParser[] Parsers { get; set; } = [];
}