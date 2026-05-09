using Beskar.Markdown.Tests;

namespace Beskar.Markdown.Tests.Inlines;

public sealed class InlineHtmlHtmlTests
{
   [Test]
   public Task SimpleInlineHtml()
   {
      const string markdown = "Use <span>HTML</span>.";
      const string expectedHtml = "<p>Use <span>HTML</span>.</p>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task InlineHtmlWithAttributes()
   {
      const string markdown = """<span class="test">text</span>""";
      const string expectedHtml = """<p><span class="test">text</span></p>""";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }
}
