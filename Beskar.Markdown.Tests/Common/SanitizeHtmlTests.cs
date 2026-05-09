using Beskar.Markdown.Rendering;

namespace Beskar.Markdown.Tests.Common;

public sealed class SanitizeHtmlTests
{
   [Test]
   public Task SanitizeInjectionHtml()
   {
      var options = RenderOptions.HtmlDefault;
      options.SanitizerFunc = (span) => "";

      const string markdown = "Hallo";
      const string html = "";

      return MarkdownAssert.RendersHtml(markdown, html, renderOptions: options);
   }
   
   [Test]
   public Task SanitizeInjectionHtmlWorking()
   {
      var options = RenderOptions.HtmlDefault;
      options.SanitizerFunc = (span) => $"{span[0]}";

      const string markdown = "Hallo";
      const string html = "<";

      return MarkdownAssert.RendersHtml(markdown, html, renderOptions: options);
   }
}