using Beskar.Markdown.Tests;

namespace Beskar.Markdown.Tests.Blocks;

public sealed class BlockNodeHtmlTests
{
   [Test]
   public Task AtxHeadingRendersHeadingLevelAndInlineContent()
   {
      const string markdown = "### Ship **Beskar** parser";
      const string expectedHtml = "<h3>Ship <strong>Beskar</strong> parser</h3>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task SetextHeadingRendersAsHeading()
   {
      const string markdown =
         """
         Beskar Markdown
         ===============
         """;

      const string expectedHtml = "<h1>Beskar Markdown</h1>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task ParagraphKeepsSoftBreaksInsideOneParagraph()
   {
      const string markdown =
         """
         alpha
         beta
         gamma
         """;

      const string expectedHtml =
         """
         <p>alpha
         beta
         gamma</p>
         """;

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task ThematicBreakRendersHorizontalRule()
   {
      const string markdown = "  * * *";
      const string expectedHtml = "<hr />";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task FencedCodeBlockRendersLiteralContent()
   {
      const string markdown =
         """
         ```csharp
         Console.WriteLine("hello");
         ```
         """;

      const string expectedHtml = """<pre><code>Console.WriteLine("hello");</code></pre>""";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task TildeFencedCodeBlockRendersContentUntilClosingFence()
   {
      const string markdown =
         """
         ~~~
         first line
         second line
         ~~~
         """;

      const string expectedHtml =
         """
         <pre><code>first line
         second line</code></pre>
         """;

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task IndentedCodeBlockRendersEachIndentedLine()
   {
      const string markdown =
         """
             var answer = 42;
             return answer;
         """;

      const string expectedHtml =
         """
         <pre><code>var answer = 42;
         return answer;
         </code></pre>
         """;

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task BlockQuoteRendersNestedBlocks()
   {
      const string markdown =
         """
         > # Title
         > quoted text
         """;

      const string expectedHtml = "<blockquote><h1>Title</h1><p>quoted text</p></blockquote>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task HtmlBlockPassesThroughRawHtml()
   {
      const string markdown =
         """
         <section>
         <p>Raw HTML</p>
         </section>
         """;

      const string expectedHtml =
         """
         <section>
         <p>Raw HTML</p>
         </section>
         """;

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task UnorderedListRendersListItems()
   {
      const string markdown =
         """
         - beskar
         - cortosis
         """;

      const string expectedHtml = "<ul><li>beskar</li><li>cortosis</li></ul>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task OrderedListRendersListItems()
   {
      const string markdown =
         """
         1. parse
         2. render
         """;

      const string expectedHtml = "<ol><li>parse</li><li>render</li></ol>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }
}
