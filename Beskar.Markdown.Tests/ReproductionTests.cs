
using Beskar.Markdown.Rendering;

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
   
   [Test]
   public Task HeaderFrontSpaces()
   {
      const string markdown = "#                  foo                     \n";
      const string expectedHtml = "<h1>foo</h1>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }
   
   [Test]
   public Task EmphasisWithLinkNoEmphasis()
   {
      const string markdown = "*[bar*](/url)\n";
      const string expectedHtml = "<p>*<a href=\"/url\">bar*</a></p>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }
   
   [Test]
   public Task EmphasisWithLinkNoEmphasis2()
   {
      const string markdown = "*[bar](/url)*";
      const string expectedHtml = "<p><em><a href=\"/url\">bar</a></em></p>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task PartialEmphasisMatch()
   {
      const string markdown = "**foo*";
      const string expectedHtml = "<p>*<em>foo</em></p>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }
   
   // [Test]
   // public Task MultiLineListItem()
   // {
   //    const string markdown = "  - foo\n\n    bar\n";
   //    const string expectedHtml = "<ul>\n<li>\n<p>foo</p>\n<p>bar</p>\n</li>\n</ul>\n";
   //
   //    var renderOptions = RenderOptions.HtmlDefault;
   //    return MarkdownAssert.RendersHtml(markdown, expectedHtml, renderOptions: renderOptions);
   // }
}
