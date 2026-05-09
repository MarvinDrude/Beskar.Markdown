using Beskar.Markdown.Tests;

namespace Beskar.Markdown.Tests.Inlines;

public sealed class AutolinkHtmlTests
{
   [Test]
   public Task UriAutolink()
   {
      const string markdown = "<https://example.com>";
      const string expectedHtml = """<p><a href="https://example.com">https://example.com</a></p>""";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task EmailAutolink()
   {
      const string markdown = "<support@example.com>";
      const string expectedHtml = """<p><a href="mailto:support@example.com">support@example.com</a></p>""";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }
}
