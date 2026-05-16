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

   [Test]
   public Task AutolinkWithQueryParameters()
   {
      const string markdown = "<https://example.com/search?q=markdown&lang=en>";
      const string expectedHtml = """<p><a href="https://example.com/search?q=markdown&lang=en">https://example.com/search?q=markdown&amp;lang=en</a></p>""";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task AutolinkWithEncodedCharacters()
   {
      const string markdown = "<https://example.com/foo%20bar>";
      const string expectedHtml = """<p><a href="https://example.com/foo%20bar">https://example.com/foo%20bar</a></p>""";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task EmailAutolinkWithPlus()
   {
      const string markdown = "<user+mailbox@example.com>";
      const string expectedHtml = """<p><a href="mailto:user+mailbox@example.com">user+mailbox@example.com</a></p>""";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task InvalidAutolinkNotRecognized()
   {
      const string markdown = "<not-a-url>";
      const string expectedHtml = """<not-a-url>""";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }
}
