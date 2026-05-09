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

   [Test]
   public Task InlineCodeWithLeadingTrailingSpaces()
   {
      const string markdown = "`  code   `";
      const string expectedHtml = "<p><code> code  </code></p>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task InlineCodeAcrossLines()
   {
      const string markdown = "`code\nacross lines`";
      const string expectedHtml = "<p>`code\nacross lines`</p>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task InlineCodeWithHtmlInside()
   {
      const string markdown = "`<div>`";
      const string expectedHtml = "<p><code>&lt;div&gt;</code></p>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task InlineCodeNotParsingOtherInlines()
   {
      const string markdown = "`*not emphasis*`";
      const string expectedHtml = "<p><code>*not emphasis*</code></p>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }
}
