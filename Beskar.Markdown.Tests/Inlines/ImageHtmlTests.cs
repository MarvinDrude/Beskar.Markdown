using Beskar.Markdown.Tests;

namespace Beskar.Markdown.Tests.Inlines;

public sealed class ImageHtmlTests
{
   [Test]
   public Task SimpleImage()
   {
      const string markdown = "![alt](https://example.com/image.png)";
      const string expectedHtml = """<p><img src="https://example.com/image.png" alt="alt" /></p>""";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task ImageWithTitle()
   {
      const string markdown = """![alt](https://example.com/image.png "title")""";
      const string expectedHtml = """<p><img src="https://example.com/image.png" alt="alt" title="title" /></p>""";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }
}
