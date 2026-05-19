using Beskar.Markdown.Builders;
using Beskar.Markdown.Extensions;
using Beskar.Markdown.Parsing.Models;
using Beskar.Markdown.Rendering.Interfaces;
using Me.Memory.Buffers;

namespace Beskar.Markdown.Tests.Blocks;

public sealed class CustomCodeBlockRendererTests
{
   private class TestCodeBlockRenderer : ICodeBlockRenderer
   {
      public bool TryRender<TData>(
         MarkdownContext<TData> context,
         ref TextWriterIndentSlim writer,
         ReadOnlySpan<char> code,
         ReadOnlySpan<char> language)
      {
         writer.Write("<div class=\"custom-code\" data-lang=\"");
         writer.WriteHtmlDecodedAndEncoded(language);
         writer.Write("\">");
         
         writer.Write(code);
         
         writer.Write("</div>");
         
         return true;
      }
   }

   [Test]
   public Task FencedCodeBlock_WithCustomRenderer_ShouldUseCustomOutput()
   {
      var markdown = "```csharp\nvar x = 1;\n```";
      var expectedHtml = "<div class=\"custom-code\" data-lang=\"csharp\">var x = 1;\n</div>";

      var options = MarkdownOptionBuilder.Create()
         .WithCodeBlockRenderer(new TestCodeBlockRenderer())
         .Build();

      return MarkdownAssert.RendersHtml(markdown, expectedHtml, options.ParserOptions, options.RenderOptions);
   }

   [Test]
   public Task IndentedCodeBlock_WithCustomRenderer_ShouldUseCustomOutput()
   {
      var markdown = "    var x = 1;\n    var y = 2;";
      var expectedHtml = "<div class=\"custom-code\" data-lang=\"\">var x = 1;\nvar y = 2;\n</div>";

      var options = MarkdownOptionBuilder.Create()
         .WithCodeBlockRenderer(new TestCodeBlockRenderer())
         .Build();

      return MarkdownAssert.RendersHtml(markdown, expectedHtml, options.ParserOptions, options.RenderOptions);
   }

   [Test]
   public Task IndentedFencedCodeBlock_WithCustomRenderer_ShouldUseCustomOutput()
   {
      var markdown = "  ```csharp\n  var x = 1;\n  ```";
      var expectedHtml = "<div class=\"custom-code\" data-lang=\"csharp\">var x = 1;\n</div>";

      var options = MarkdownOptionBuilder.Create()
         .WithCodeBlockRenderer(new TestCodeBlockRenderer())
         .Build();

      return MarkdownAssert.RendersHtml(markdown, expectedHtml, options.ParserOptions, options.RenderOptions);
   }
}