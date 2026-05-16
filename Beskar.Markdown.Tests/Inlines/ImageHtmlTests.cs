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
   public Task ImageWithComplexAltText()
   {
      const string markdown = "![alt with *emphasis* and `code`](https://example.com/image.png)";
      const string expectedHtml = """<p><img src="https://example.com/image.png" alt="alt with emphasis and code" /></p>""";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task ImageWithEmptyAltText()
   {
      const string markdown = "![](https://example.com/image.png)";
      const string expectedHtml = """<p><img src="https://example.com/image.png" alt="" /></p>""";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task ImageNestedInLink()
   {
      const string markdown = "[![alt](img)](url)";
      const string expectedHtml = """<p><a href="url"><img src="img" alt="alt" /></a></p>""";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }
}
