using Beskar.Markdown.Parsing;
using Beskar.Markdown.Parsing.Models;
using Beskar.Markdown.Rendering;

namespace Beskar.Markdown;

public static class BeMarkdown
{
   public static string ToHtml(
      string markdown, 
      ParserOptions? parserOptions = null, 
      RenderOptions? renderOptions = null)
   {
      return ToHtml(markdown.AsSpan(), parserOptions, renderOptions);
   }

   public static string ToHtml(
      ReadOnlySpan<char> markdown, 
      ParserOptions? parserOptions = null, 
      RenderOptions? renderOptions = null)
   {
      parserOptions ??= ParserOptions.Default;
      renderOptions ??= RenderOptions.HtmlDefault;
      
      using var parser = new MarkdownParser(markdown, stackalloc MarkdownNode[16]);
      parser.Parse(parserOptions);

      var renderer = new MarkdownRenderer(markdown);
      return renderer.Render(parser.WrittenNodes, renderOptions);
   }
}