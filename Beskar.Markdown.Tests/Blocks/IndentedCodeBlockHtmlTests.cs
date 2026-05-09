using Beskar.Markdown.Tests;

namespace Beskar.Markdown.Tests.Blocks;

public sealed class IndentedCodeBlockHtmlTests
{
   [Test]
   public Task IndentedCodeBlock()
   {
      const string markdown = "    code";
      const string expectedHtml = "<pre><code>code\n</code></pre>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task MultipleIndentedLines()
   {
      const string markdown = 
         """
             line 1
             line 2
         """;
      const string expectedHtml = "<pre><code>line 1\nline 2\n</code></pre>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }
}
