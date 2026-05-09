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
}
