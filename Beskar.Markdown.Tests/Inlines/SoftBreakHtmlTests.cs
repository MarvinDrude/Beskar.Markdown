using Beskar.Markdown.Tests;

namespace Beskar.Markdown.Tests.Inlines;

public sealed class SoftBreakHtmlTests
{
   [Test]
   public Task SoftBreakRendersAsNewline()
   {
      const string markdown = "line 1\nline 2";
      const string expectedHtml = "<p>line 1\nline 2</p>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }
}
