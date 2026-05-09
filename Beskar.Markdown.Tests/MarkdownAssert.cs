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
      var parser = new MarkdownParser(markdown, stackalloc MarkdownNode[32]);
      parser.Parse(parserOptions ?? ParserOptions.Default);
      
      var debugString = parser.WrittenNodes.ToArray().CreateDebugString(markdown);
      var html = BeMarkdown.ToHtml(markdown, parserOptions, renderOptions);
      parser.Dispose();
      
      await Assert.That(NormalizeLineEndings(html))
         .IsEqualTo(NormalizeLineEndings(expectedHtml));
   }

   private static string NormalizeLineEndings(string text)
   {
      return text.Replace("\r\n", "\n");
   }
}
