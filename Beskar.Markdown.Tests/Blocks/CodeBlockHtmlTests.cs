using Beskar.Markdown.Tests;

namespace Beskar.Markdown.Tests.Blocks;

public sealed class CodeBlockHtmlTests
{
   [Test]
   public Task FencedCodeBlock()
   {
      const string markdown = 
         """
         ```
         code
         ```
         """;
      const string expectedHtml = "<pre><code>code\n</code></pre>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task FencedCodeBlockWithLanguage()
   {
      const string markdown = 
         """
         ```csharp
         var x = 1;
         ```
         """;
      
      const string expectedHtml = "<pre><code class=\"language-csharp\">var x = 1;\n</code></pre>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task TildeFencedCodeBlock()
   {
      const string markdown = 
         """
         ~~~
         code
         ~~~
         """;
      const string expectedHtml = "<pre><code>code\n</code></pre>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task FencedCodeBlockWithInternalBackticks()
   {
      const string markdown = 
         """
         ```
         `backtick`
         ```
         """;
      const string expectedHtml = "<pre><code>`backtick`\n</code></pre>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task FencedCodeBlockClosingWithMoreBackticks()
   {
      const string markdown = 
         """
         ```
         code
         ````
         """;
      const string expectedHtml = "<pre><code>code\n</code></pre>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task FencedCodeBlockNotClosed()
   {
      const string markdown = 
         """
         ```
         code
         """;
      const string expectedHtml = "<pre><code>code</code></pre>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }
   
   [Test]
   public Task FencedCodeBlockNotClosedNewline()
   {
      const string markdown = 
         """
         ```
         code
         
         """;
      const string expectedHtml = "<pre><code>code\n</code></pre>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task FencedCodeBlockIndented()
   {
      const string markdown = 
         """
           ```
           code
           ```
         """;
      const string expectedHtml = "<pre><code>code\n</code></pre>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }
}
