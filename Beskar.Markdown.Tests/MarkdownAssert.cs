using Beskar.Markdown.Extensions;
using Beskar.Markdown.Parsing;
using Beskar.Markdown.Parsing.Models;
using Beskar.Markdown.Rendering;

namespace Beskar.Markdown.Tests;

internal static class MarkdownAssert
{
   public static async Task RendersHtml(string markdown, string expectedHtml, 
      ParserOptions? parserOptions = null, RenderOptions? renderOptions = null)
   {
      if (renderOptions is null)
      {
         renderOptions = RenderOptions.HtmlDefault;
         renderOptions.AddBlockNewLines = false;
      }
      
      var parser = new MarkdownParser<object>(markdown, stackalloc MarkdownNode[32]);
      parser.Parse(parserOptions ?? ParserOptions.Default);
      
      var debugString = parser.WrittenNodes.ToArray().CreateDebugString(markdown);
      var html = BeMarkdown.ToHtml(markdown, parserOptions, renderOptions);
      parser.Dispose();
      
      await Assert.That(NormalizeLineEndings(html))
         .IsEqualTo(NormalizeLineEndings(expectedHtml));
   }

   public static async Task RendersPlainText(string markdown, string expectedText, 
      ParserOptions? parserOptions = null, RenderOptions? renderOptions = null)
   {
      if (renderOptions is null)
      {
         renderOptions = RenderOptions.PlainDefault;
         renderOptions.AddBlockNewLines = false;
      }
      
      var text = BeMarkdown.ToPlainText(markdown, parserOptions, renderOptions);
      
      await Assert.That(NormalizeLineEndings(text))
         .IsEqualTo(NormalizeLineEndings(expectedText));
   }

   private static string NormalizeLineEndings(string text)
   {
      return text.Replace("\r\n", "\n");
   }
}
