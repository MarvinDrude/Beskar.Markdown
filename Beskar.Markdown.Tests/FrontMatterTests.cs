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

   [Test]
   public async Task ShouldHandleCorrectly()
   {
      var markdown = """
                     ---
                     Hallo:   Welt
                     ---
                     # My Content *hallo* `test` a

                     - item 1
                     - item 2
                     """;

      var options = MarkdownOptionBuilder.Create()
         .WithFrontMatter()
         .Build();

      var result = BeMarkdown.Parse(markdown.AsSpan(), options);

      await Assert.That(result.Context.FrontMatter["Hallo"]).IsEqualTo("Welt");
   }

   [Test]
   public async Task ShouldHandleMalformedFrontMatter()
   {
      var markdown = """
                     ---
                     title Hello World
                     author:Marvin:Extra
                     ---
                     # My Content
                     """;

      var options = MarkdownOptionBuilder.Create()
         .WithFrontMatter()
         .Build();

      var result = BeMarkdown.Parse(markdown.AsSpan(), options);

      await Assert.That(result.Context.FrontMatter).Count().IsEqualTo(1);
      await Assert.That(result.Context.FrontMatter["author"]).IsEqualTo("Marvin:Extra");
      await Assert.That(result.Html.Trim()).IsEqualTo("<h1>My Content</h1>");
   }

   [Test]
   public async Task ShouldHandleDifferentLineEndings()
   {
      var markdown = "---\r\ntitle: Windows\r\n---\r\n# Content";

      var options = MarkdownOptionBuilder.Create()
         .WithFrontMatter()
         .Build();

      var result = BeMarkdown.Parse(markdown.AsSpan(), options);

      await Assert.That(result.Context.FrontMatter["title"]).IsEqualTo("Windows");
      await Assert.That(result.Html.Trim()).IsEqualTo("<h1>Content</h1>");
   }

   [Test]
   public async Task ShouldNotParseFrontMatterIfNotEnabled()
   {
      var markdown = """
                     ---
                     title: Hello
                     ---
                     # Content
                     """;

      var options = MarkdownOptionBuilder.Create()
         .Build();

      var result = BeMarkdown.Parse(markdown.AsSpan(), options);

      await Assert.That(result.Context.FrontMatter).IsEmpty();
      // It will be parsed as a thematic break and paragraph/heading
      await Assert.That(result.Html).Contains("<hr />");
   }
}