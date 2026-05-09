using Beskar.Markdown.Tests;

namespace Beskar.Markdown.Tests.Inlines;

public sealed class StrongEmphasisHtmlTests
{
   [Test]
   public Task SimpleStrongEmphasisAsterisks()
   {
      const string markdown = "**strong**";
      const string expectedHtml = "<p><strong>strong</strong></p>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task SimpleStrongEmphasisUnderscores()
   {
      const string markdown = "__strong__";
      const string expectedHtml = "<p><strong>strong</strong></p>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task StrongEmphasisNestedWithEmphasis()
   {
      const string markdown = "**strong *emphasis***";
      const string expectedHtml = "<p><strong>strong <em>emphasis</em></strong></p>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task EmphasisNestedInStrong()
   {
      const string markdown = "**outer _inner_ outer**";
      const string expectedHtml = "<p><strong>outer <em>inner</em> outer</strong></p>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task StrongNestedInStrong()
   {
      const string markdown = "****strong****";
      const string expectedHtml = "<p><strong><strong>strong</strong></strong></p>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task StrongWithInlines()
   {
      const string markdown = "**strong `code` [link](url)**";
      const string expectedHtml = """<p><strong>strong <code>code</code> <a href="url">link</a></strong></p>""";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }
}
