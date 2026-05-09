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

   [Test]
   public Task BlockQuoteWithLazyContinuation()
   {
      const string markdown = 
         """
         > first line
         continuation line
         """;
      const string expectedHtml = "<blockquote><p>first line</p></blockquote><p>continuation line</p>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task BlockQuoteWithMultipleParagraphs()
   {
      const string markdown = 
         """
         > para 1
         >
         > para 2
         """;
      const string expectedHtml = "<blockquote><p>para 1</p><p>para 2</p></blockquote>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task BlockQuoteIndented()
   {
      const string markdown = "  > indented quote";
      const string expectedHtml = "<blockquote><p>indented quote</p></blockquote>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task BlockQuoteConsecutiveQuotes()
   {
      const string markdown = "> foo\n> bar";
      const string expectedHtml = "<blockquote><p>foo\nbar</p></blockquote>";
      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task BlockQuoteSeparatedByBlankLine()
   {
      const string markdown = "> foo\n\n> bar";
      const string expectedHtml = "<blockquote><p>foo</p></blockquote><blockquote><p>bar</p></blockquote>";
      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task BlockQuoteInterruptedByList()
   {
      const string markdown = "> foo\n- bar";
      const string expectedHtml = "<blockquote><p>foo</p></blockquote><ul><li>bar</li></ul>";
      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task BlockQuoteLazyContinuationWithList()
   {
      const string markdown = "> - foo\nbar";
      const string expectedHtml = "<blockquote><ul><li>foo\nbar</li></ul></blockquote>";
      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task BlockQuoteIndentedFourSpaces()
   {
      const string markdown = "    > not a quote";
      const string expectedHtml = "<pre><code>&gt; not a quote\n</code></pre>";
      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }
}
