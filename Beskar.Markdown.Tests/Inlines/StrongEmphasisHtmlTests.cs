using Beskar.Markdown.Tests;

namespace Beskar.Markdown.Tests.Inlines;

public sealed class StrongEmphasisHtmlTests
{
   [Test]
   public Task SimpleStrongEmphasisAsterisks()
   {
      const string markdown = "**strong**";
      const string expectedHtml = "<p><strong>strong</strong></p>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task SimpleStrongEmphasisUnderscores()
   {
      const string markdown = "__strong__";
      const string expectedHtml = "<p><strong>strong</strong></p>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task StrongEmphasisNestedWithEmphasis()
   {
      const string markdown = "**strong *emphasis***";
      const string expectedHtml = "<p><strong>strong <em>emphasis</em></strong></p>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }
}
