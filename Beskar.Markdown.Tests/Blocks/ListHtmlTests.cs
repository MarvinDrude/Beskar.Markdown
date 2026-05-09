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
}
