using Beskar.Markdown.Extensions;
using Beskar.Markdown.Parsing;
using Beskar.Markdown.Parsing.Models;

namespace Beskar.Markdown.Tests;

internal static class MarkdownAssert
{
   public static async Task RendersHtml(string markdown, string expectedHtml)
   {
      var parser = new MarkdownParser(markdown, stackalloc MarkdownNode[32]);
      parser.Parse(ParserOptions.Default);
      
      var debugString = parser.WrittenNodes.ToArray().CreateDebugString(markdown);
      var html = BeMarkdown.ToHtml(markdown);
      parser.Dispose();
      
      await Assert.That(NormalizeLineEndings(html))
         .IsEqualTo(NormalizeLineEndings(expectedHtml));
   }

   public static string NormalizeLineEndings(string text)
   {
      return text.Replace("\r\n", "\n");
   }
}
