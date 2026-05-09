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

   [Test]
   public Task ThematicBreakWithUnderscores()
   {
      const string markdown = "___";
      const string expectedHtml = "<hr />";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task ThematicBreakWithInternalSpaces()
   {
      const string markdown = " - - - ";
      const string expectedHtml = "<hr />";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task ThematicBreakInterruptedByText()
   {
      const string markdown = "--- foo ---";
      const string expectedHtml = "<p>--- foo ---</p>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task ThematicBreakNotInterruptedBySpaces()
   {
      const string markdown = " _ _ _ _ a";
      const string expectedHtml = "<p>_ _ _ _ a</p>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task ThematicBreakIndented()
   {
      const string markdown = "  ***";
      const string expectedHtml = "<hr />";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task ThematicBreakTooMuchIndent()
   {
      const string markdown = "    ***";
      const string expectedHtml = "<pre><code>***\n</code></pre>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task ThematicBreakMixedMarkersNotAllowed()
   {
      const string markdown = "*-*";
      const string expectedHtml = "<p><em>-</em></p>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }
}
