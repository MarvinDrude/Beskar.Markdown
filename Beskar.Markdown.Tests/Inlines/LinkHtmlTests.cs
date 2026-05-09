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
      // Assuming title is rendered if supported, but let's check standard behavior
      const string expectedHtml = """<p><a href="https://example.com" title="title">link</a></p>""";

      // If title is not supported yet, this might fail, but let's see.
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
