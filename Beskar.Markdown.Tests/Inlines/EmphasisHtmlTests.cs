using Beskar.Markdown.Tests;

namespace Beskar.Markdown.Tests.Inlines;

public sealed class EmphasisHtmlTests
{
   [Test]
   public Task SimpleEmphasisAsterisk()
   {
      const string markdown = "*emphasis*";
      const string expectedHtml = "<p><em>emphasis</em></p>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task SimpleEmphasisUnderscore()
   {
      const string markdown = "_emphasis_";
      const string expectedHtml = "<p><em>emphasis</em></p>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task EmphasisWithMixedInlines()
   {
      const string markdown = "*emphasis with `code`*";
      const string expectedHtml = "<p><em>emphasis with <code>code</code></em></p>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }
}
