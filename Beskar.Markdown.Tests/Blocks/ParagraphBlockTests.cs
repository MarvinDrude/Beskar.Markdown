using Beskar.Markdown.Extensions;
using Beskar.Markdown.Parsing;
using Beskar.Markdown.Parsing.Models;

namespace Beskar.Markdown.Tests.Blocks;

public sealed class ParagraphBlockTests
{
   [Test]
   public async Task NewlinesIntoOneParagraph()
   {
      const string markdown = 
         """
         title: CommonMark Spec
         author: John MacFarlane
         version: '0.31.2'
         date: '2024-01-28'
         license: '[CC-BY-SA 4.0](https://creativecommons.org/licenses/by-sa/4.0/)'
         ...
         """;

      var parser = new MarkdownParser<object>(markdown, stackalloc MarkdownNode[32]);
      parser.Parse(ParserOptions.Default);
      var debugString = parser.WrittenNodes.ToArray().CreateDebugString(markdown);
      
      var html = BeMarkdown.ToHtml(markdown);
      parser.Dispose();
      
      await Assert.That(html).IsEqualTo(
         """
         <p>title: CommonMark Spec
         author: John MacFarlane
         version: '0.31.2'
         date: '2024-01-28'
         license: '<a href="https://creativecommons.org/licenses/by-sa/4.0/">CC-BY-SA 4.0</a>'
         ...</p>
         
         """.Replace("\r\n", "\n"));
   }
}