using Beskar.Markdown.Builders;

namespace Beskar.Markdown.Tests;

public class SluggableHeaderTests
{
   [Test]
   public async Task ShouldRenderSluggableHeader()
   {
      var markdown = "# My Header Text";

      var options = MarkdownOptionBuilder.Create()
         .WithSluggableHeaders()
         .Build();

      var html = BeMarkdown.ToHtml(markdown, options);

      await Assert.That(html.Trim()).IsEqualTo("<h1 id=\"my-header-text\">My Header Text</h1>");
   }

   [Test]
   public async Task ShouldHandleComplexSluggableHeader()
   {
      var markdown = "## This Is A - Complex _ Header!";

      var options = MarkdownOptionBuilder.Create()
         .WithSluggableHeaders()
         .Build();

      var html = BeMarkdown.ToHtml(markdown, options);

      // ! should be removed, spaces/-/_ replaced by single -
      await Assert.That(html.Trim())
         .IsEqualTo("<h2 id=\"this-is-a-complex-header\">This Is A - Complex _ Header!</h2>");
   }

   [Test]
   public async Task ShouldNotRenderSlugIfDisabled()
   {
      var markdown = "# My Header Text";

      var options = MarkdownOptionBuilder.Create()
         .Build();

      var html = BeMarkdown.ToHtml(markdown, options);

      await Assert.That(html.Trim()).IsEqualTo("<h1>My Header Text</h1>");
   }
}