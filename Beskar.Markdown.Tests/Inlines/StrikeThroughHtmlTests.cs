using Beskar.Markdown.Tests;

namespace Beskar.Markdown.Tests.Inlines;

public sealed class StrikeThroughHtmlTests
{
   [Test]
   public Task SimpleStrikeThrough()
   {
      const string markdown = "~~strike~~";
      const string expectedHtml = "<p><del>strike</del></p>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task StrikeThroughWithMixedInlines()
   {
      const string markdown = "~~strike with *emphasis*~~";
      const string expectedHtml = "<p><del>strike with <em>emphasis</em></del></p>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }
}
