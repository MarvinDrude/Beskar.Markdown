using Beskar.Markdown.Tests;

namespace Beskar.Markdown.Tests.Blocks;

public sealed class HeadingHtmlTests
{
   [Test]
   public Task AtxHeadingLevel1()
   {
      const string markdown = "# Heading 1";
      const string expectedHtml = "<h1>Heading 1</h1>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task AtxHeadingLevel6()
   {
      const string markdown = "###### Heading 6";
      const string expectedHtml = "<h6>Heading 6</h6>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task SetextHeadingLevel1()
   {
      const string markdown = "Heading 1\n=";
      const string expectedHtml = "<h1>Heading 1</h1>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task SetextHeadingLevel2()
   {
      const string markdown = "Heading 2\n-";
      const string expectedHtml = "<h2>Heading 2</h2>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task HeadingWithInlines()
   {
      const string markdown = "# Heading with *emphasis*";
      const string expectedHtml = "<h1>Heading with <em>emphasis</em></h1>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task AtxHeadingWithClosingHashes()
   {
      const string markdown = "## Heading ##";
      const string expectedHtml = "<h2>Heading</h2>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task AtxHeadingNotAHeading()
   {
      const string markdown = "####### Heading 7";
      const string expectedHtml = "<p>####### Heading 7</p>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task AtxHeadingRequiresSpace()
   {
      const string markdown = "#Heading";
      const string expectedHtml = "<p>#Heading</p>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task SetextHeadingWithLeadingSpaces()
   {
      const string markdown = "  Heading\n  =";
      const string expectedHtml = "<h1>Heading</h1>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task SetextHeadingWithInternalNewlines()
   {
      const string markdown = "Heading\nLine 2\n=";
      const string expectedHtml = "<h1>Heading\nLine 2</h1>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }
}
