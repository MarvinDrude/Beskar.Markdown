using System.Text.Json;
using System.Text.Json.Serialization;
using Beskar.Markdown.Rendering;

namespace Beskar.Markdown.Tests.Common;

public sealed class SpecTests
{
   public sealed record SpecExample
   {
      [JsonPropertyName("markdown")] 
      public required string Markdown { get; init; }

      [JsonPropertyName("html")] 
      public required string Html { get; init; }

      [JsonPropertyName("example")] 
      public int Example { get; init; }

      [JsonPropertyName("section")] 
      public required string Section { get; init; }
   }

   [Test]
   [MethodDataSource(nameof(GetExamples))]
   public Task Spec(string markdown, string html, int example, string section)
   {
      return MarkdownAssert.RendersHtml(markdown, html, renderOptions: RenderOptions.HtmlDefault);
   }

   public static IEnumerable<(string markdown, string html, int example, string section)> GetExamples()
   {
      var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "spec.json");
      if (!File.Exists(path))
      {
         path = "spec.json";
      }

      var json = File.ReadAllText(path);
      var examples = JsonSerializer.Deserialize<List<SpecExample>>(json);

      if (examples == null) yield break;

      foreach (var example in examples)
      {
         yield return (example.Markdown, example.Html, example.Example, example.Section);
      }
   }
}