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

   [Test]
   public Task LinkWithEmptyDestination()
   {
      const string markdown = "[]()";
      const string expectedHtml = """<p><a href=""></a></p>""";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task LinkWithSpacesInDestination()
   {
      const string markdown = "[link](<my url>)";
      const string expectedHtml = """<p><a href="my%20url">link</a></p>""";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task LinkWithEscapedCharacters()
   {
      const string markdown = @"[link](https://example.com?foo=3#frag)";
      const string expectedHtml = """<p><a href="https://example.com?foo=3#frag">link</a></p>""";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task NestedLinksNotAllowed()
   {
      const string markdown = "[outer [inner](url1)](url2)";
      const string expectedHtml = """<p>[outer <a href="url1">inner</a>](url2)</p>""";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task DeepBracketRunDoesNotExplodeNestedLinkDetection()
   {
      const string markdown = "[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]";
      const string expectedHtml = """<p>[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[[]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]]</p>""";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task LinkWithNewlinesInTitle()
   {
      const string markdown = "[link](url 'title with newline')";
      const string expectedHtml = """<p><a href="url" title="title with newline">link</a></p>""";
      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task LinkWithAngleBracketsAndEscapes()
   {
      const string markdown = "[link](<url\\>>)";
      const string expectedHtml = """<p><a href="url>">link</a></p>""";
      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }
}
