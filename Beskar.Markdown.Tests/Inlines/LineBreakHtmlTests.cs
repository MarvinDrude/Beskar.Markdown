using Beskar.Markdown.Tests;

namespace Beskar.Markdown.Tests.Inlines;

public sealed class LineBreakHtmlTests
{
   [Test]
   public Task HardLineBreakWithSpaces()
   {
      const string markdown = "line 1  \nline 2";
      const string expectedHtml = "<p>line 1<br />line 2</p>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task HardLineBreakWithBackslash()
   {
      const string markdown = "line 1\\\nline 2";
      const string expectedHtml = "<p>line 1<br />line 2</p>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }
}
