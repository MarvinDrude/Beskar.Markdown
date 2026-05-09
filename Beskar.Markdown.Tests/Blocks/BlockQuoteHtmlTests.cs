using Beskar.Markdown.Tests;

namespace Beskar.Markdown.Tests.Blocks;

public sealed class BlockQuoteHtmlTests
{
   [Test]
   public Task SimpleBlockQuote()
   {
      const string markdown = "> text";
      const string expectedHtml = "<blockquote><p>text</p></blockquote>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task NestedBlockQuote()
   {
      const string markdown = "> > text";
      const string expectedHtml = "<blockquote><blockquote><p>text</p></blockquote></blockquote>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task BlockQuoteWithHeading()
   {
      const string markdown = "> # Heading";
      const string expectedHtml = "<blockquote><h1>Heading</h1></blockquote>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task BlockQuoteWithMixedContent()
   {
      const string markdown = 
         """
         > text
         > 
         > - list item
         """;
      const string expectedHtml = "<blockquote><p>text</p><ul><li>list item</li></ul></blockquote>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }
}
