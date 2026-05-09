using Beskar.Markdown.Tests;

namespace Beskar.Markdown.Tests.Blocks;

public sealed class HtmlBlockHtmlTests
{
   [Test]
   public Task SimpleHtmlBlock()
   {
      const string markdown = "<div>\ncontent\n</div>";
      const string expectedHtml = "<div>\ncontent\n</div>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task HtmlBlockWithMixedContent()
   {
      const string markdown = 
         """
         <section>
         # Not a heading inside HTML block
         </section>
         """;
      const string expectedHtml = 
         """
         <section>
         # Not a heading inside HTML block
         </section>
         """;

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task HtmlBlockType1Script()
   {
      const string markdown = "<script type=\"text/javascript\">\n// <![CDATA[\nfunction some_func() { return 1; }\n// ]]>\n</script>";
      const string expectedHtml = "<script type=\"text/javascript\">\n// <![CDATA[\nfunction some_func() { return 1; }\n// ]]>\n</script>";
      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task HtmlBlockType2Comment()
   {
      const string markdown = "<!-- foo\n\nbar\n -->";
      const string expectedHtml = "<!-- foo\n\nbar\n -->";
      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task HtmlBlockType4Declaration()
   {
      const string markdown = "<!DOCTYPE html>";
      const string expectedHtml = "<!DOCTYPE html>";
      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task HtmlBlockType5CData()
   {
      const string markdown = "<![CDATA[\nfunction f(x) {\n  return x < 0;\n}\n]]>";
      const string expectedHtml = "<![CDATA[\nfunction f(x) {\n  return x < 0;\n}\n]]>";
      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task HtmlBlockInterruptedByBlankLine()
   {
      const string markdown = "<table>\n  <tr>\n    <td>\n      hi\n    </td>\n  </tr>\n</table>\n\nokay.";
      const string expectedHtml = "<table>\n  <tr>\n    <td>\n      hi\n    </td>\n  </tr>\n</table><p>okay.</p>";
      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }
}
