namespace Beskar.Markdown.Tests.Blocks;

public sealed class IndentedCodeBlockTests
{
   [Test]
   public async Task SpacesTabulatedExampleOne()
   {
      const string markdown = 
         """
             foo    baz        bim
         """;
      
      var html = BeMarkdown.ToHtml(markdown);
      await Assert.That(html).IsEqualTo("<pre><code>foo    baz        bim\n</code></pre>\n");
   }
   
   [Test]
   public Task TabulatedExampleOne()
   {
      const string markdown = 
         "\tfoo\tbaz\t\tbim";

      const string expected = "<pre><code>foo\tbaz\t\tbim\n</code></pre>";
      return MarkdownAssert.RendersHtml(markdown, expected);
   }
}