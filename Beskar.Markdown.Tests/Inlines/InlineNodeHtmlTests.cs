using Beskar.Markdown.Rendering;
using Beskar.Markdown.Tests;

namespace Beskar.Markdown.Tests.Inlines;

public sealed class InlineNodeHtmlTests
{
   [Test]
   public Task PlainTextRendersInsideParagraph()
   {
      const string markdown = "plain text";
      const string expectedHtml = "<p>plain text</p>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task BackslashEscapesMarkdownPunctuation()
   {
      const string markdown = "\\*not emphasized\\*";
      const string expectedHtml = "<p>*not emphasized*</p>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task EmphasisRendersWithEmTag()
   {
      const string markdown = "Use *emphasis* and _stress_.";
      const string expectedHtml = "<p>Use <em>emphasis</em> and <em>stress</em>.</p>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task StrongEmphasisRendersWithStrongTag()
   {
      const string markdown = "Use **strong** and __bold__.";
      const string expectedHtml = "<p>Use <strong>strong</strong> and <strong>bold</strong>.</p>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task NestedEmphasisRendersNestedTags()
   {
      const string markdown = "***very important***";
      const string expectedHtml = "<p><em><strong>very important</strong></em></p>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task InlineCodeRendersCodeSpan()
   {
      const string markdown = "Call `ToHtml(markdown)`.";
      const string expectedHtml = "<p>Call <code>ToHtml(markdown)</code>.</p>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task InlineCodeTrimsOnePaddingSpaceAroundNonSpaceContent()
   {
      const string markdown = "Use `` `code` ``.";
      const string expectedHtml = "<p>Use <code>`code`</code>.</p>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task LinkRendersAnchor()
   {
      const string markdown = "Read [the docs](https://example.com/docs).";
      const string expectedHtml = """<p>Read <a href="https://example.com/docs">the docs</a>.</p>""";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task LinkWithAngleBracketDestinationRendersAnchor()
   {
      const string markdown = "Read [the docs](<https://example.com/docs?q=1>).";
      const string expectedHtml = """<p>Read <a href="https://example.com/docs?q=1">the docs</a>.</p>""";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task ImageRendersImageElementWithAltText()
   {
      const string markdown = "Logo: ![Beskar logo](/assets/beskar.png).";
      const string expectedHtml = """<p>Logo: <img src="/assets/beskar.png" alt="Beskar logo" />.</p>""";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task InlineHtmlPassesThroughRawHtml()
   {
      const string markdown = """Use <span class="token">raw</span> HTML.""";
      const string expectedHtml = """<p>Use <span class="token">raw</span> HTML.</p>""";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task SoftBreakPreservesSourceNewlineByDefault()
   {
      const string markdown =
         """
         alpha
         beta
         """;

      const string expectedHtml =
         """
         <p>alpha
         beta</p>
         """;

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public async Task SoftBreakCanRenderAsSpace()
   {
      const string markdown =
         """
         alpha
         beta
         """;

      var options = RenderOptions.HtmlDefault;
      options.PreserveSoftBreaks = false;

      var html = BeMarkdown.ToHtml(markdown, renderOptions: options);

      await Assert.That(html).IsEqualTo("<p>alpha beta</p>\n");
   }

   [Test]
   public Task BackslashLineBreakRendersBreakElement()
   {
      const string markdown =
         """
         alpha\
         beta
         """;

      const string expectedHtml = "<p>alpha<br />beta</p>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task TrailingSpacesLineBreakRendersBreakElement()
   {
      const string markdown = "alpha  \nbeta";
      const string expectedHtml = "<p>alpha<br />\nbeta</p>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task StrikeThroughRendersDeletedText()
   {
      const string markdown = "Remove ~~obsolete~~ text.";
      const string expectedHtml = "<p>Remove <del>obsolete</del> text.</p>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task UriAutolinkRendersAnchor()
   {
      const string markdown = "Visit <https://example.com/docs>.";
      const string expectedHtml = """<p>Visit <a href="https://example.com/docs">https://example.com/docs</a>.</p>""";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task EmailAutolinkRendersMailtoAnchor()
   {
      const string markdown = "Email <support@example.com>.";
      const string expectedHtml = """<p>Email <a href="mailto:support@example.com">support@example.com</a>.</p>""";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }
}
