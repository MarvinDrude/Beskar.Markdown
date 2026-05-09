using Beskar.Markdown.Tests;

namespace Beskar.Markdown.Tests.Inlines;

public sealed class LinkHtmlTests
{
   [Test]
   public Task SimpleLink()
   {
      const string markdown = "[link](https://example.com)";
      const string expectedHtml = """<p><a href="https://example.com">link</a></p>""";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task LinkWithTitle()
   {
      const string markdown = """[link](https://example.com "title")""";
      const string expectedHtml = """<p><a href="https://example.com" title="title">link</a></p>""";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task LinkWithInlines()
   {
      const string markdown = "[link with *emphasis*](https://example.com)";
      const string expectedHtml = """<p><a href="https://example.com">link with <em>emphasis</em></a></p>""";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }
}
