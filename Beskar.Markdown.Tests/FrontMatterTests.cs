using Beskar.Markdown.Builders;

namespace Beskar.Markdown.Tests;

public class FrontMatterTests
{
   [Test]
   public async Task ShouldParseFrontMatter()
   {
      var markdown = """
                     ---
                     title: Hello World
                     author: Marvin
                     ---
                     # My Content
                     """;

      var options = MarkdownOptionBuilder.Create()
         .WithFrontMatter()
         .Build();

      var result = BeMarkdown.Parse(markdown.AsSpan(), options);

      await Assert.That(result.Context.FrontMatter["title"]).IsEqualTo("Hello World");
      await Assert.That(result.Context.FrontMatter["author"]).IsEqualTo("Marvin");
      await Assert.That(result.Html.Trim()).IsEqualTo("<h1>My Content</h1>");
   }

   [Test]
   public async Task ShouldHandleNoFrontMatter()
   {
      var markdown = "# My Content";

      var options = MarkdownOptionBuilder.Create()
         .WithFrontMatter()
         .Build();

      var result = BeMarkdown.Parse(markdown.AsSpan(), options);

      await Assert.That(result.Context.FrontMatter).IsEmpty();
      await Assert.That(result.Html.Trim()).IsEqualTo("<h1>My Content</h1>");
   }

   [Test]
   public async Task ShouldHandleEmptyFrontMatter()
   {
      var markdown = """
                     ---
                     ---
                     # My Content
                     """;

      var options = MarkdownOptionBuilder.Create()
         .WithFrontMatter()
         .Build();

      var result = BeMarkdown.Parse(markdown.AsSpan(), options);

      await Assert.That(result.Context.FrontMatter).IsEmpty();
      await Assert.That(result.Html.Trim()).IsEqualTo("<h1>My Content</h1>");
   }
}