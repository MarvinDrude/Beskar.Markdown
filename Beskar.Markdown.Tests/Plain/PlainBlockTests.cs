using Beskar.Markdown.Rendering;
using Beskar.Markdown.Tests;

namespace Beskar.Markdown.Tests.Plain;

public sealed class PlainBlockTests
{
   [Test]
   public Task SimpleParagraph()
   {
      const string markdown = "This is a single paragraph.";
      const string expected = "This is a single paragraph.";
      return MarkdownAssert.RendersPlainText(markdown, expected);
   }

   [Test]
   public async Task MultipleParagraphsWithBlockNewLines()
   {
      const string markdown = "Paragraph 1\n\nParagraph 2";
      var renderOptions = RenderOptions.PlainDefault;
      renderOptions.AddBlockNewLines = true;
      
      var result = BeMarkdown.ToPlainText(markdown, renderOptions: renderOptions);
      var normalized = result.Replace("\r\n", "\n");
      
      await Assert.That(normalized).IsEqualTo("Paragraph 1\nParagraph 2\n");
   }

   [Test]
   public async Task Headings()
   {
      const string markdown = "# Heading 1\n## Heading 2\n### Heading 3";
      var renderOptions = RenderOptions.PlainDefault;
      renderOptions.AddBlockNewLines = true;
      
      var result = BeMarkdown.ToPlainText(markdown, renderOptions: renderOptions);
      var normalized = result.Replace("\r\n", "\n");
      
      await Assert.That(normalized).IsEqualTo("Heading 1\nHeading 2\nHeading 3\n");
   }

   [Test]
   public async Task SluggableHeaders()
   {
      const string markdown = "# Main Title\n## Sub Title";
      var renderOptions = RenderOptions.PlainDefault;
      renderOptions.EnableSluggableHeaders = true;
      
      var result = BeMarkdown.Parse(markdown.AsSpan(), new Builders.MarkdownOptions
      {
         ParserOptions = Parsing.Models.ParserOptions.Default,
         RenderOptions = renderOptions
      });
      
      await Assert.That(result.Context.Headers.Count).IsEqualTo(2);
   }

   [Test]
   public Task BlockQuote()
   {
      const string markdown = "> This is a quote.\n> Second line of quote.";
      const string expected = "This is a quote.\nSecond line of quote.";
      return MarkdownAssert.RendersPlainText(markdown, expected);
   }

   [Test]
   public async Task FencedCodeBlock()
   {
      const string markdown = "```csharp\nvar x = 42;\nConsole.WriteLine(x);\n```";
      var renderOptions = RenderOptions.PlainDefault;
      renderOptions.AddBlockNewLines = true;

      var result = BeMarkdown.ToPlainText(markdown, renderOptions: renderOptions);
      var normalized = result.Replace("\r\n", "\n");

      await Assert.That(normalized).IsEqualTo("var x = 42;\nConsole.WriteLine(x);\n");
   }

   [Test]
   public async Task IndentedCodeBlock()
   {
      const string markdown = "    var a = 1;\n    var b = 2;";
      var renderOptions = RenderOptions.PlainDefault;
      renderOptions.AddBlockNewLines = true;

      var result = BeMarkdown.ToPlainText(markdown, renderOptions: renderOptions);
      var normalized = result.Replace("\r\n", "\n");

      await Assert.That(normalized).IsEqualTo("var a = 1;\nvar b = 2;\n\n");
   }

   [Test]
   public async Task UnorderedList()
   {
      const string markdown = "- Item 1\n- Item 2\n- Item 3";
      var renderOptions = RenderOptions.PlainDefault;
      renderOptions.AddBlockNewLines = true;

      var result = BeMarkdown.ToPlainText(markdown, renderOptions: renderOptions);
      var normalized = result.Replace("\r\n", "\n");

      await Assert.That(normalized).IsEqualTo("Item 1\nItem 2\nItem 3\n");
   }

   [Test]
   public async Task OrderedList()
   {
      const string markdown = "1. First\n2. Second\n3. Third";
      var renderOptions = RenderOptions.PlainDefault;
      renderOptions.AddBlockNewLines = true;

      var result = BeMarkdown.ToPlainText(markdown, renderOptions: renderOptions);
      var normalized = result.Replace("\r\n", "\n");

      await Assert.That(normalized).IsEqualTo("First\nSecond\nThird\n");
   }

   [Test]
   public async Task TaskList()
   {
      const string markdown = "- [ ] Todo item\n- [x] Done item";
      var renderOptions = RenderOptions.PlainDefault;
      renderOptions.AddBlockNewLines = true;

      var result = BeMarkdown.ToPlainText(markdown, renderOptions: renderOptions);
      var normalized = result.Replace("\r\n", "\n");

      await Assert.That(normalized).IsEqualTo("[ ] Todo item\n[x] Done item\n");
   }

   [Test]
   public async Task ThematicBreak()
   {
      const string markdown = "Before\n\n---\n\nAfter";
      var renderOptions = RenderOptions.PlainDefault;
      renderOptions.AddBlockNewLines = true;

      var result = BeMarkdown.ToPlainText(markdown, renderOptions: renderOptions);
      var normalized = result.Replace("\r\n", "\n");

      await Assert.That(normalized).IsEqualTo("Before\n---\nAfter\n");
   }

   [Test]
   public async Task Table()
   {
      const string markdown = 
         """
         | Header 1 | Header 2 |
         | --- | --- |
         | Cell 1 | Cell 2 |
         | Cell 3 | Cell 4 |
         """;
      var renderOptions = RenderOptions.PlainDefault;
      renderOptions.AddBlockNewLines = true;

      var result = BeMarkdown.ToPlainText(markdown, renderOptions: renderOptions);
      var normalized = result.Replace("\r\n", "\n");

      await Assert.That(normalized).IsEqualTo("Header 1\tHeader 2\nCell 1\tCell 2\nCell 3\tCell 4\n");
   }

   [Test]
   public async Task HtmlBlock()
   {
      const string markdown = "<div>\n<p>Hello HTML block</p>\n</div>";
      var renderOptions = RenderOptions.PlainDefault;
      renderOptions.AddBlockNewLines = true;

      var result = BeMarkdown.ToPlainText(markdown, renderOptions: renderOptions);
      var normalized = result.Replace("\r\n", "\n");

      await Assert.That(normalized).IsEqualTo("<div>\n<p>Hello HTML block</p>\n</div>\n");
   }

   [Test]
   public async Task NestedLists()
   {
      const string markdown =
         """
         - Parent 1
           - Child 1
           - Child 2
         - Parent 2
         """;
      var renderOptions = RenderOptions.PlainDefault;
      renderOptions.AddBlockNewLines = true;

      var result = BeMarkdown.ToPlainText(markdown, renderOptions: renderOptions);
      var normalized = result.Replace("\r\n", "\n");

      await Assert.That(normalized).Contains("Parent 1");
      await Assert.That(normalized).Contains("Child 1");
      await Assert.That(normalized).Contains("Child 2");
      await Assert.That(normalized).Contains("Parent 2");
   }

   [Test]
   public async Task TableWithInlineFormatting()
   {
      const string markdown =
         """
         | **Name** | *Type* | `Default` |
         | --- | --- | --- |
         | **Foo** | *String* | `null` |
         """;
      var renderOptions = RenderOptions.PlainDefault;
      renderOptions.AddBlockNewLines = true;

      var result = BeMarkdown.ToPlainText(markdown, renderOptions: renderOptions);
      var normalized = result.Replace("\r\n", "\n");

      await Assert.That(normalized).IsEqualTo("Name\tType\tDefault\nFoo\tString\tnull\n");
   }
}
