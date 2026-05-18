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

   [Test]
   public async Task ShouldRenderUniqueSlugsForDuplicateHeaders()
   {
      var markdown = """
                     # Duplicate
                     # Duplicate
                     # Duplicate
                     """;

      var options = MarkdownOptionBuilder.Create()
         .WithSluggableHeaders()
         .Build();

      var result = BeMarkdown.Parse(markdown, options);

      await Assert.That(result.Html).Contains("id=\"duplicate\"");
      await Assert.That(result.Html).Contains("id=\"duplicate-1\"");
      await Assert.That(result.Html).Contains("id=\"duplicate-2\"");

      await Assert.That(result.Context.SlugToPlainText["duplicate"]).IsEqualTo("Duplicate");
      await Assert.That(result.Context.SlugToPlainText["duplicate-1"]).IsEqualTo("Duplicate");
      await Assert.That(result.Context.SlugToPlainText["duplicate-2"]).IsEqualTo("Duplicate");
   }

   [Test]
   public async Task ShouldStoreOrderedHeadersInContext()
   {
      var markdown = """
                     # Main Title
                     ## Sub Section
                     ### Third Level
                     """;

      var options = MarkdownOptionBuilder.Create()
         .WithSluggableHeaders()
         .Build();

      var result = BeMarkdown.Parse(markdown, options);

      await Assert.That(result.Context.Headers).HasCount(3);

      await Assert.That(result.Context.Headers[0].Slug).IsEqualTo("main-title");
      await Assert.That(result.Context.Headers[0].PlainText).IsEqualTo("Main Title");
      await Assert.That(result.Context.Headers[0].Level).IsEqualTo(1);

      await Assert.That(result.Context.Headers[1].Slug).IsEqualTo("sub-section");
      await Assert.That(result.Context.Headers[1].PlainText).IsEqualTo("Sub Section");
      await Assert.That(result.Context.Headers[1].Level).IsEqualTo(2);

      await Assert.That(result.Context.Headers[2].Slug).IsEqualTo("third-level");
      await Assert.That(result.Context.Headers[2].PlainText).IsEqualTo("Third Level");
      await Assert.That(result.Context.Headers[2].Level).IsEqualTo(3);
   }
}