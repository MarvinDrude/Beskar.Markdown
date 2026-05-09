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

   [Test]
   public Task EmphasisNested()
   {
      const string markdown = "*outer _inner_ outer*";
      const string expectedHtml = "<p><em>outer <em>inner</em> outer</em></p>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task EmphasisAcrossLines()
   {
      const string markdown = "*first line\nsecond line*";
      const string expectedHtml = "<p><em>first line\nsecond line</em></p>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task EmphasisWithEscapedDelimiters()
   {
      const string markdown = @"\*not emphasis*";
      const string expectedHtml = "<p>*not emphasis*</p>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task EmphasisInMiddleOfWordAsterisk()
   {
      const string markdown = "foo*bar*baz";
      const string expectedHtml = "<p>foo<em>bar</em>baz</p>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task EmphasisInMiddleOfWordUnderscore()
   {
      const string markdown = "foo_bar_baz";
      const string expectedHtml = "<p>foo_bar_baz</p>"; // CommonMark doesn't allow intraword emphasis with underscores

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task EmphasisMultipleAsterisks()
   {
      const string markdown = "5*6*78";
      const string expectedHtml = "<p>5<em>6</em>78</p>";
      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task EmphasisNotStartedBySpaceAsterisk()
   {
      const string markdown = "foo *bar*";
      const string expectedHtml = "<p>foo <em>bar</em></p>";
      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task EmphasisSpaceAfterOpeningNotAllowed()
   {
      const string markdown = "aa * foo*";
      const string expectedHtml = "<p>aa * foo*</p>";
      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task EmphasisSpaceBeforeClosingNotAllowed()
   {
      const string markdown = "*foo *";
      const string expectedHtml = "<p>*foo *</p>";
      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task EmphasisWithPunctuation()
   {
      const string markdown = "*(*foo)";
      const string expectedHtml = "<p>*(*foo)</p>";
      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task EmphasisUnderscoreInternalPunctuation()
   {
      const string markdown = "(_foo_)";
      const string expectedHtml = "<p>(<em>foo</em>)</p>";
      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }
   
   [Test]
   public Task EmphasisInnerOuter()
   {
      const string markdown = "*(*foo*)*";
      const string expectedHtml = "<p><em>(<em>foo</em>)</em></p>";
      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }
}
