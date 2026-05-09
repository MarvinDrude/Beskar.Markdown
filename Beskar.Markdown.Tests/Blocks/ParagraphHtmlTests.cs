using Beskar.Markdown.Tests;

namespace Beskar.Markdown.Tests.Blocks;

public sealed class ParagraphHtmlTests
{
   [Test]
   public Task SimpleParagraph()
   {
      const string markdown = "This is a paragraph.";
      const string expectedHtml = "<p>This is a paragraph.</p>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task ParagraphWithSoftBreaks()
   {
      const string markdown = "line 1\nline 2";
      const string expectedHtml = "<p>line 1\nline 2</p>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task MultipleParagraphs()
   {
      const string markdown = "Para 1\n\nPara 2";
      const string expectedHtml = "<p>Para 1</p><p>Para 2</p>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }
}
