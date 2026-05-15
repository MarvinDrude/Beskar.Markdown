namespace Beskar.Markdown.Tests.Blocks;

public class LinkReferenceDefinitionTests
{
   [Test]
   public Task SimpleLinkReferenceDefinition()
   {
      const string markdown = "[foo]: /url \"title\"\n\n[foo]";
      const string expected = "<p><a href=\"/url\" title=\"title\">foo</a></p>";

      return MarkdownAssert.RendersHtml(markdown, expected);
   }

   [Test]
   public Task CaseInsensitiveMatching()
   {
      const string markdown = "[FOO]: /url\n\n[foo]";
      const string expected = "<p><a href=\"/url\">foo</a></p>";

      return MarkdownAssert.RendersHtml(markdown, expected);
   }

   [Test]
   public Task MultiLineTitle()
   {
      const string markdown = "[foo]: /url\n'multi\nline\ntitle'\n\n[foo]";
      const string expected = "<p><a href=\"/url\" title=\"multi\nline\ntitle\">foo</a></p>";

      return MarkdownAssert.RendersHtml(markdown, expected);
   }

   [Test]
   public Task ShortcutLink()
   {
      const string markdown = "[foo]: /url\n\n[foo]";
      const string expected = "<p><a href=\"/url\">foo</a></p>";

      return MarkdownAssert.RendersHtml(markdown, expected);
   }

   [Test]
   public Task CollapsedLink()
   {
      const string markdown = "[foo]: /url\n\n[foo][]";
      const string expected = "<p><a href=\"/url\">foo</a></p>";

      return MarkdownAssert.RendersHtml(markdown, expected);
   }

   [Test]
   public Task FullLink()
   {
      const string markdown = "[bar]: /url\n\n[foo][bar]";
      const string expected = "<p><a href=\"/url\">foo</a></p>";

      return MarkdownAssert.RendersHtml(markdown, expected);
   }

   [Test]
   public Task MultipleDefinitionsFirstWins()
   {
      const string markdown = "[foo]: first\n[foo]: second\n\n[foo]";
      const string expected = "<p><a href=\"first\">foo</a></p>";

      return MarkdownAssert.RendersHtml(markdown, expected);
   }

   [Test]
   public Task DoesNotInterruptParagraph()
   {
      const string markdown = "Foo\n[bar]: /baz\n\n[bar]";
      const string expected = "<p>Foo\n[bar]: /baz</p><p>[bar]</p>";

      return MarkdownAssert.RendersHtml(markdown, expected);
   }

   [Test]
   public Task ReferenceImage()
   {
      const string markdown = "[foo]: /url \"title\"\n\n![bar][foo]";
      const string expected = "<p><img src=\"/url\" alt=\"bar\" title=\"title\" /></p>";

      return MarkdownAssert.RendersHtml(markdown, expected);
   }

   [Test]
   public Task BackslashEscapes()
   {
      const string markdown = "[foo]: /url\\bar\\*baz \"foo\\\"bar\\\\baz\"\n\n[foo]";
      const string expected = "<p><a href=\"/url%5Cbar%5C*baz\" title=\"foo&quot;bar\\baz\">foo</a></p>";

      return MarkdownAssert.RendersHtml(markdown, expected);
   }
}