
namespace Beskar.Markdown.Tests;

public sealed class ReproductionTests
{
   [Test]
   public Task MultilineParagraphNoTrailingSpace()
   {
      const string markdown = "foo\nbaz";
      const string expectedHtml = "<p>foo\nbaz</p>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task MultilineHtmlComment()
   {
      const string markdown = "foo <!-- this is a --\ncomment - with hyphens -->";
      const string expectedHtml = "<p>foo <!-- this is a --\ncomment - with hyphens --></p>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task HardLineBreakWithSpaces()
   {
      const string markdown = "foo  \nbaz";
      const string expectedHtml = "<p>foo<br />\nbaz</p>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task HardLineBreakWithTab()
   {
      const string markdown = "foo\t\nbaz";
      const string expectedHtml = "<p>foo<br />\nbaz</p>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task HardLineBreakWithThreeSpaces()
   {
      const string markdown = "foo   \nbaz";
      const string expectedHtml = "<p>foo<br />\nbaz</p>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }
   
   [Test]
   public Task SpacesBeforeCodeBlock()
   {
      const string markdown = " ```\n aaa\naaa\n```\n";
      const string expectedHtml = "<pre><code>aaa\naaa\n</code></pre>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }
}
