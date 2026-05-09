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
}
