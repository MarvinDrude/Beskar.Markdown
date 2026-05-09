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

   [Test]
   public Task StrikeThroughNested()
   {
      const string markdown = "~~outer ~~inner~~ outer~~";
      const string expectedHtml = "<p><del>outer </del>inner<del> outer</del></p>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task StrikeThroughWithSingleTilde()
   {
      const string markdown = "~not strike~";
      const string expectedHtml = "<p>~not strike~</p>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task StrikeThroughAcrossLines()
   {
      const string markdown = "~~first line\nsecond line~~";
      const string expectedHtml = "<p>~~first line\nsecond line~~</p>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }
}
