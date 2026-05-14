using Beskar.Markdown.Tests;

namespace Beskar.Markdown.Tests.Inlines;

public sealed class LineBreakHtmlTests
{
   [Test]
   public Task HardLineBreakWithSpaces()
   {
      const string markdown = "line 1  \nline 2";
      const string expectedHtml = "<p>line 1\nline 2</p>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task HardLineBreakWithBackslash()
   {
      const string markdown = "line 1\\\nline 2";
      const string expectedHtml = "<p>line 1<br />line 2</p>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task SoftBreak()
   {
      const string markdown = "line 1\nline 2";
      const string expectedHtml = "<p>line 1\nline 2</p>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task MultipleSpacesAreSoftBreak()
   {
      const string markdown = "line 1 \nline 2";
      const string expectedHtml = "<p>line 1\nline 2</p>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task HardLineBreakAtEndOfParagraph()
   {
      const string markdown = "line 1  ";
      const string expectedHtml = "<p>line 1</p>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task HardLineBreakWithMoreThanTwoSpaces()
   {
      const string markdown = "line 1   \nline 2";
      const string expectedHtml = "<p>line 1\nline 2</p>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task ExampleLinkSimple()
   {
      const string markdown = "[foo bar](url)";
      const string expectedHtml = """<p><a href="url">foo bar</a></p>""";
      
      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }
}
