using Beskar.Markdown.Tests;

namespace Beskar.Markdown.Tests.Blocks;

public sealed class ThematicBreakHtmlTests
{
   [Test]
   public Task SimpleThematicBreak()
   {
      const string markdown = "---";
      const string expectedHtml = "<hr />";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task ThematicBreakWithAsterisks()
   {
      const string markdown = "***";
      const string expectedHtml = "<hr />";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task ThematicBreakWithSpaces()
   {
      const string markdown = "* * *";
      const string expectedHtml = "<hr />";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }
}
