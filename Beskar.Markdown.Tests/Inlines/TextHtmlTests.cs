using Beskar.Markdown.Tests;

namespace Beskar.Markdown.Tests.Inlines;

public sealed class TextHtmlTests
{
   [Test]
   public Task PlainText()
   {
      const string markdown = "just some text";
      const string expectedHtml = "<p>just some text</p>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task TextWithEscapedCharacters()
   {
      const string markdown = @"\*not emphasis\*";
      const string expectedHtml = "<p>*not emphasis*</p>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }
}
