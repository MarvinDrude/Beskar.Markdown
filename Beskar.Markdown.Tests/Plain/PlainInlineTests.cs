using Beskar.Markdown.Rendering;
using Beskar.Markdown.Tests;

namespace Beskar.Markdown.Tests.Plain;

public sealed class PlainInlineTests
{
   [Test]
   public Task SimpleText()
   {
      const string markdown = "Hello, world!";
      const string expected = "Hello, world!";
      return MarkdownAssert.RendersPlainText(markdown, expected);
   }

   [Test]
   public Task TextWithEscapes()
   {
      const string markdown = @"\*Not italic\* and \[not a link\] and \# not a header";
      const string expected = "*Not italic* and [not a link] and # not a header";
      return MarkdownAssert.RendersPlainText(markdown, expected);
   }

   [Test]
   public Task TextWithHtmlEntities()
   {
      const string markdown = "Tom &amp; Jerry &copy; 2026 &#39;quotes&#39; &lt;tag&gt;";
      const string expected = "Tom & Jerry © 2026 'quotes' <tag>";
      return MarkdownAssert.RendersPlainText(markdown, expected);
   }

   [Test]
   public Task Emphasis()
   {
      const string markdown = "This is *italic* and _also italic_.";
      const string expected = "This is italic and also italic.";
      return MarkdownAssert.RendersPlainText(markdown, expected);
   }

   [Test]
   public Task StrongEmphasis()
   {
      const string markdown = "This is **bold** and __also bold__.";
      const string expected = "This is bold and also bold.";
      return MarkdownAssert.RendersPlainText(markdown, expected);
   }

   [Test]
   public Task CombinedEmphasis()
   {
      const string markdown = "This is ***bold italic*** text.";
      const string expected = "This is bold italic text.";
      return MarkdownAssert.RendersPlainText(markdown, expected);
   }

   [Test]
   public Task StrikeThrough()
   {
      const string markdown = "This is ~~struck through~~ text.";
      const string expected = "This is struck through text.";
      return MarkdownAssert.RendersPlainText(markdown, expected);
   }

   [Test]
   public Task InlineCode()
   {
      const string markdown = "Use `var x = 10;` in your code.";
      const string expected = "Use var x = 10; in your code.";
      return MarkdownAssert.RendersPlainText(markdown, expected);
   }

   [Test]
   public Task InlineCodeWithNewlines()
   {
      const string markdown = "Use `var x = 10;\nvar y = 20;` here.";
      const string expected = "Use var x = 10; var y = 20; here.";
      return MarkdownAssert.RendersPlainText(markdown, expected);
   }

   [Test]
   public Task AutolinkUrl()
   {
      const string markdown = "<https://github.com>";
      const string expected = "https://github.com";
      return MarkdownAssert.RendersPlainText(markdown, expected);
   }

   [Test]
   public Task AutolinkEmail()
   {
      const string markdown = "<dev@example.com>";
      const string expected = "dev@example.com";
      return MarkdownAssert.RendersPlainText(markdown, expected);
   }

   [Test]
   public Task LinkWithText()
   {
      const string markdown = "[Google](https://google.com)";
      const string expected = "Google (https://google.com)";
      return MarkdownAssert.RendersPlainText(markdown, expected);
   }

   [Test]
   public Task LinkWithIdenticalTextAndUrl()
   {
      const string markdown = "[https://google.com](https://google.com)";
      const string expected = "https://google.com";
      return MarkdownAssert.RendersPlainText(markdown, expected);
   }

   [Test]
   public Task LinkWithEmptyText()
   {
      const string markdown = "[](https://google.com)";
      const string expected = "https://google.com";
      return MarkdownAssert.RendersPlainText(markdown, expected);
   }

   [Test]
   public Task LinkWithNestedFormatting()
   {
      const string markdown = "[Visit **our website**](https://example.com)";
      const string expected = "Visit our website (https://example.com)";
      return MarkdownAssert.RendersPlainText(markdown, expected);
   }

   [Test]
   public Task ImageWithAltText()
   {
      const string markdown = "![Antigravity Logo](https://example.com/logo.png)";
      const string expected = "Antigravity Logo";
      return MarkdownAssert.RendersPlainText(markdown, expected);
   }

   [Test]
   public Task ImageWithoutAltText()
   {
      const string markdown = "![](https://example.com/logo.png)";
      const string expected = "";
      return MarkdownAssert.RendersPlainText(markdown, expected);
   }

   [Test]
   public Task InlineHtml()
   {
      const string markdown = "Hello <span style=\"color:red\">world</span>!";
      const string expected = "Hello <span style=\"color:red\">world</span>!";
      return MarkdownAssert.RendersPlainText(markdown, expected);
   }

   [Test]
   public Task LineBreak()
   {
      const string markdown = "First line  \nSecond line";
      const string expected = "First line\nSecond line";
      return MarkdownAssert.RendersPlainText(markdown, expected);
   }

   [Test]
   public Task SoftBreakPreserved()
   {
      const string markdown = "First line\nSecond line";
      const string expected = "First line\nSecond line";
      var renderOptions = RenderOptions.PlainDefault;
      renderOptions.PreserveSoftBreaks = true;
      renderOptions.AddBlockNewLines = false;
      return MarkdownAssert.RendersPlainText(markdown, expected, renderOptions: renderOptions);
   }

   [Test]
   public Task SoftBreakAsSpace()
   {
      const string markdown = "First line\nSecond line";
      const string expected = "First line Second line";
      var renderOptions = RenderOptions.PlainDefault;
      renderOptions.PreserveSoftBreaks = false;
      renderOptions.AddBlockNewLines = false;
      return MarkdownAssert.RendersPlainText(markdown, expected, renderOptions: renderOptions);
   }

   [Test]
   public async Task VariableReplacement()
   {
      const string markdown = "Hello, {{ username }}!";
      var data = new Dictionary<string, object> { ["username"] = "Alice" };
      var options = Builders.MarkdownOptionBuilder.Create()
         .WithVariables(true)
         .Build();
      
      var text = BeMarkdown.ToContextualPlainText(markdown.AsSpan(), options.ParserOptions, RenderOptions.PlainDefault, data);
      await Assert.That(text.TrimEnd()).IsEqualTo("Hello, Alice!");
   }
}
