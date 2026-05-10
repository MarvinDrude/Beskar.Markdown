using Beskar.Markdown.Parsing.Models;
using Beskar.Markdown.Rendering;

namespace Beskar.Markdown.Builders;

public sealed class MarkdownOptions
{
   public required RenderOptions RenderOptions { get; init; }
   
   public required ParserOptions ParserOptions { get; init; }
}