using System.Diagnostics.CodeAnalysis;
using Beskar.Markdown.Extensions;
using Beskar.Markdown.Parsing;
using Beskar.Markdown.Parsing.Models;
using Beskar.Markdown.Rendering;

namespace Beskar.Markdown;

public static class BeMarkdown
{
   private static readonly ParserOptions _defaultParserOptions = ParserOptions.Default;
   private static readonly RenderOptions _defaultRenderOptions = RenderOptions.HtmlDefault;
   
   public static string ToHtml(
      [StringSyntax("Markdown")] string markdown, 
      ParserOptions? parserOptions = null, 
      RenderOptions? renderOptions = null)
   {
      return ToHtml(markdown.AsSpan(), parserOptions, renderOptions);
   }

   public static string ToHtml(
      [StringSyntax("Markdown")] ReadOnlySpan<char> markdown, 
      ParserOptions? parserOptions = null, 
      RenderOptions? renderOptions = null)
   {
      parserOptions ??= _defaultParserOptions;
      renderOptions ??= _defaultRenderOptions;
      
      using var parser = new MarkdownParser(markdown, stackalloc MarkdownNode[16]);
      parser.Parse(parserOptions);

      var debug = parser.WrittenNodes.ToArray().CreateDebugString(markdown);

      var renderer = new MarkdownRenderer(markdown);
      return renderer.Render(parser.WrittenNodes, renderOptions);
   }
}