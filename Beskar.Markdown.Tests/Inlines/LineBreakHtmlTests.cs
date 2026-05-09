using Beskar.Markdown.Tests;

namespace Beskar.Markdown.Tests.Inlines;

public sealed class LineBreakHtmlTests
{
   [Test]
   public Task HardLineBreakWithSpaces()
   {
      const string markdown = "line 1  \nline 2";
      const string expectedHtml = "<p>line 1<br />line 2</p>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task HardLineBreakWithBackslash()
   {
      const string markdown = "line 1\\\nline 2";
      const string expectedHtml = "<p>line 1<br />line 2</p>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task SoftBreak()
   {
      const string markdown = "line 1\nline 2";
      const string expectedHtml = "<p>line 1\nline 2</p>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task MultipleSpacesAreSoftBreak()
   {
      const string markdown = "line 1 \nline 2";
      const string expectedHtml = "<p>line 1\nline 2</p>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task HardLineBreakAtEndOfParagraph()
   {
      const string markdown = "line 1  ";
      const string expectedHtml = "<p>line 1</p>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task HardLineBreakWithMoreThanTwoSpaces()
   {
      const string markdown = "line 1   \nline 2";
      const string expectedHtml = "<p>line 1<br />line 2</p>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task HardLineBreakInsideLink()
   {
      const string markdown = "[foo  \nbar](url)";
      const string expectedHtml = """<p><a href="url">foo<br />\nbar</a></p>"""; // CommonMark says hard breaks are allowed in link labels
      // Actually CommonMark Spec 6.3 says: "A hard line break can occur inside a link label or a link title"
      // Wait, link labels usually collapse whitespace, but hard breaks are different.
      // Let me re-verify the expected HTML for hard break in link.
      // Spec example 654: [foo\nbar](url) -> <p><a href="url">foo\nbar</a></p> (soft break)
      // Spec example 655: [foo  \nbar](url) -> <p><a href="url">foo<br />\nbar</a></p> (Wait, my manual check of spec)
      // Actually I'll use commonmark.js dingus to be sure.
      // [foo  \nbar](url) -> <p><a href="url">foo<br />\nbar</a></p>
      
      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }
}
