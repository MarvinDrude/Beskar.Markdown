using Beskar.Markdown.Tests;

namespace Beskar.Markdown.Tests.Blocks;

public sealed class ListHtmlTests
{
   [Test]
   public Task UnorderedList()
   {
      const string markdown = "- item 1\n- item 2";
      const string expectedHtml = "<ul><li>item 1</li><li>item 2</li></ul>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task OrderedList()
   {
      const string markdown = "1. item 1\n2. item 2";
      const string expectedHtml = "<ol><li>item 1</li><li>item 2</li></ol>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task OrderedListStartingAtOtherThanOne()
   {
      const string markdown = "3. item 3";
      const string expectedHtml = "<ol start=\"3\"><li>item 3</li></ol>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task NestedLists()
   {
      const string markdown = 
         """
         - parent
           - child
         """;
      const string expectedHtml = "<ul><li>parent<ul><li>child</li></ul></li></ul>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task ListWithMixedContent()
   {
      const string markdown = 
         """
         - item with *emphasis*
         - item with `code`
         """;
      const string expectedHtml = "<ul><li>item with <em>emphasis</em></li><li>item with <code>code</code></li></ul>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task TightList()
   {
      const string markdown = 
         """
         - a
         - b
         """;
      const string expectedHtml = "<ul><li>a</li><li>b</li></ul>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task LooseList()
   {
      const string markdown = 
         """
         - a

         - b
         """;
      const string expectedHtml = "<ul><li><p>a</p></li><li><p>b</p></li></ul>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task ListWithMultipleParagraphs()
   {
      const string markdown = 
         """
         - item 1, para 1

           item 1, para 2
         - item 2
         """;
      const string expectedHtml = "<ul><li><p>item 1, para 1</p><p>item 1, para 2</p></li><li><p>item 2</p></li></ul>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task OrderedListWithDifferentDelimiters()
   {
      const string markdown = "1) item 1\n2) item 2";
      const string expectedHtml = "<ol><li>item 1</li><li>item 2</li></ol>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task UnorderedListWithDifferentMarkers()
   {
      const string markdown = "* item 1\n+ item 2\n- item 3";
      const string expectedHtml = "<ul><li>item 1</li></ul><ul><li>item 2</li></ul><ul><li>item 3</li></ul>"; // CommonMark: changing marker starts new list

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task OrderedListWithDifferentDelimitersStartsNewList()
   {
      const string markdown = "1. item 1\n2) item 2";
      const string expectedHtml = "<ol><li>item 1</li></ol><ol start=\"2\"><li>item 2</li></ol>";
      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task ListInterruptedByThematicBreak()
   {
      const string markdown = "- item 1\n***\n- item 2";
      const string expectedHtml = "<ul><li>item 1</li></ul><hr /><ul><li>item 2</li></ul>";
      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task ListWithBlankLineAndIndentedCode()
   {
      const string markdown = "- item 1\n\n      code";
      const string expectedHtml = "<ul><li><p>item 1</p><pre><code>code\n</code></pre></li></ul>";
      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task ListItemWithMultipleBlocks()
   {
      const string markdown = 
         """
         - item 1
           > quote
           
           para
         """;
      const string expectedHtml = "<ul><li><p>item 1</p><blockquote>\n<p>quote</p></blockquote><p>para</p></li></ul>";
      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task ListStartingWithBlankLine()
   {
      const string markdown = "- \n  foo";
      const string expectedHtml = "<ul><li>foo</li></ul>";
      
      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task OrderedListNotInterruptedByParagraph()
   {
      const string markdown = "1. foo\nbar";
      const string expectedHtml = "<ol><li>foo\nbar</li></ol>";
      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }
}
