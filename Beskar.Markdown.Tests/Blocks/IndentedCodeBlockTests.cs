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
      await Assert.That(html).IsEqualTo("<pre><code>foo    baz        bim\n</code></pre>");
   }
   
   [Test]
   public async Task TabulatedExampleOne()
   {
      const string markdown = 
         "\tfoo\tbaz\t\tbim";
      
      var html = BeMarkdown.ToHtml(markdown);
      await Assert.That(html).IsEqualTo("<pre><code>\tbaz\t\tbim\n</code></pre>");
   }
   
   
}