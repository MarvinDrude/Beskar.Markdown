using Beskar.Markdown.Tests;

namespace Beskar.Markdown.Tests.Blocks;

public sealed class TaskListHtmlTests
{
   [Test]
   public Task Example279()
   {
      const string markdown = 
         """
         - [ ] foo
         - [x] bar
         """;
      const string expectedHtml = "<ul><li><input disabled=\"\" type=\"checkbox\"> foo</li><li><input checked=\"\" disabled=\"\" type=\"checkbox\"> bar</li></ul>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task Example280()
   {
      const string markdown = 
         """
         - [x] foo
           - [ ] bar
           - [x] baz
         - [ ] bim
         """;
      const string expectedHtml = "<ul><li><input checked=\"\" disabled=\"\" type=\"checkbox\"> foo<ul><li><input disabled=\"\" type=\"checkbox\"> bar</li><li><input checked=\"\" disabled=\"\" type=\"checkbox\"> baz</li></ul></li><li><input disabled=\"\" type=\"checkbox\"> bim</li></ul>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }
   
   [Test]
   public Task TaskListWithUpperCaseX()
   {
      const string markdown = "- [X] foo";
      const string expectedHtml = "<ul><li><input checked=\"\" disabled=\"\" type=\"checkbox\"> foo</li></ul>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task TaskListWithDifferentMarkers()
   {
      const string markdown = 
         """
         * [ ] star
         + [x] plus
         1. [ ] ordered
         """;
      const string expectedHtml = "<ul><li><input disabled=\"\" type=\"checkbox\"> star</li></ul><ul><li><input checked=\"\" disabled=\"\" type=\"checkbox\"> plus</li></ul><ol><li><input disabled=\"\" type=\"checkbox\"> ordered</li></ol>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task TaskListNotRecognizedWithoutSpace()
   {
      const string markdown = "- [ ]foo";
      const string expectedHtml = "<ul><li>[ ]foo</li></ul>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task TaskListNotRecognizedWithTooManySpaces()
   {
      const string markdown = "- [  ] foo";
      const string expectedHtml = "<ul><li>[  ] foo</li></ul>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task TaskListAtEndOfLine()
   {
      const string markdown = "- [ ]";
      const string expectedHtml = "<ul><li><input disabled=\"\" type=\"checkbox\"> </li></ul>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task TaskListOnlyFirstMarkerIsExtracted()
   {
      const string markdown = 
         """
         - [ ]
           [x] bar
         """;
      const string expectedHtml = "<ul><li><input disabled=\"\" type=\"checkbox\"> [x] bar</li></ul>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }

   [Test]
   public Task TaskListInLooseList()
   {
      const string markdown = 
         """
         - [ ]
         
           foo
         """;
      const string expectedHtml = "<ul><li><input disabled=\"\" type=\"checkbox\"> <p></p><p>foo</p></li></ul>";

      return MarkdownAssert.RendersHtml(markdown, expectedHtml);
   }
}
