using Beskar.Markdown.Tests;

namespace Beskar.Markdown.Tests.Blocks;

public sealed class TableHtmlTests
{
   [Test]
   public Task SimpleTable()
   {
      const string markdown =
         """
         | foo | bar |
         | --- | --- |
         | baz | bim |
         """;
      const string html =
         """<table><thead><tr><th>foo</th><th>bar</th></tr></thead><tbody><tr><td>baz</td><td>bim</td></tr></tbody></table>""";
      return MarkdownAssert.RendersHtml(markdown, html);
   }

   [Test]
   public Task TableWithAlignment()
   {
      const string markdown =
         """
         | abc | defghi |
         |:-:| -----------:|
         | bar | baz |
         """;
      const string html =
         """<table><thead><tr><th align="center">abc</th><th align="right">defghi</th></tr></thead><tbody><tr><td align="center">bar</td><td align="right">baz</td></tr></tbody></table>""";

      return MarkdownAssert.RendersHtml(markdown, html);
   }

   [Test]
   public Task TableWithEscapedPipe()
   {
      const string markdown =
         """
         | f\|oo  |
         | ------ |
         | b `\|` az |
         """;
      const string html =
         """<table><thead><tr><th>f|oo</th></tr></thead><tbody><tr><td>b <code>|</code> az</td></tr></tbody></table>""";

      return MarkdownAssert.RendersHtml(markdown, html);
   }

   [Test]
   public Task TableInterruptedByBlockQuote()
   {
      const string markdown =
         """
         | abc | def |
         | --- | --- |
         | bar | baz |
         > bar
         """;
      const string html =
         """
         <table><thead><tr><th>abc</th><th>def</th></tr></thead><tbody><tr><td>bar</td><td>baz</td></tr></tbody></table><blockquote>
         <p>bar</p></blockquote>
         """;

      return MarkdownAssert.RendersHtml(markdown, html);
   }

   [Test]
   public Task TableIncompleteDelimiter()
   {
      const string markdown =
         """
         | abc | def |
         | --- |
         | bar |
         """;
      const string html =
         """
         <p>| abc | def |
         | --- |
         | bar |</p>
         """;

      return MarkdownAssert.RendersHtml(markdown, html);
   }

   [Test]
   public Task TableMissingBody()
   {
      const string markdown = 
         """
         | abc | def |
         | --- | --- |
         """;
      const string html = """<table><thead><tr><th>abc</th><th>def</th></tr></thead></table>""";

      return MarkdownAssert.RendersHtml(markdown, html);
   }
   
   [Test]
   public Task TableInterruptedByParagraph()
   {
      const string markdown =
         """
         | abc | def |
         | --- | --- |
         | bar | baz |
         
         bar
         """;
      const string html =
         """<table><thead><tr><th>abc</th><th>def</th></tr></thead><tbody><tr><td>bar</td><td>baz</td></tr></tbody></table><p>bar</p>""";

      return MarkdownAssert.RendersHtml(markdown, html);
   }
}