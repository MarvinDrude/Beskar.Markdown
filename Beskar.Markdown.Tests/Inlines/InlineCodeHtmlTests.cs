using Beskar.Markdown.Tests;

namespace Beskar.Markdown.Tests.Inlines;

public sealed class InlineCodeHtmlTests
{
   [Test]
   public Task SimpleInlineCode()
   {
      const string markdown = "`code`";
      const string expectedHtml = "<p><code>code</code></p>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task InlineCodeWithBackticks()
   {
      const string markdown = "`` `code` ``";
      const string expectedHtml = "<p><code>`code`</code></p>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }
}
