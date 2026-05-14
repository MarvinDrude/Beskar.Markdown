
namespace Beskar.Markdown.Tests;

public sealed class ReproductionTests
{
   [Test]
   public Task MultilineParagraphNoTrailingSpace()
   {
      const string markdown = "foo\nbaz";
      const string expectedHtml = "<p>foo\nbaz</p>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task MultilineParagraphWithTrailingSpace()
   {
      const string markdown = "foo \nbaz";
      const string expectedHtml = "<p>foo\nbaz</p>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }
}
