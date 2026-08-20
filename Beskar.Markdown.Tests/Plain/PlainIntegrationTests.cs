using Beskar.Markdown.Rendering;
using Beskar.Markdown.Tests;

namespace Beskar.Markdown.Tests.Plain;

public sealed class PlainIntegrationTests
{
   [Test]
   public async Task FullEmailMarkdownGeneratesBothHtmlAndPlainText()
   {
      const string emailMarkdown = 
         """
         # Welcome to Antigravity!

         Hi **John**,

         Thanks for signing up to our platform. Here is what you can do next:

         - Check out our [Documentation](https://docs.example.com)
         - Join our community on [Discord](https://discord.gg/example)
         - Verify your email address by clicking [here](https://example.com/verify?token=abc)

         ### Quick Code Example

         ```csharp
         var client = new AntigravityClient();
         await client.InitializeAsync();
         ```

         If you have any questions, reply to <support@example.com>.

         ---
         *The Antigravity Team*
         """;

      var html = BeMarkdown.ToHtml(emailMarkdown);
      var plain = BeMarkdown.ToPlainText(emailMarkdown);

      // Verify HTML has proper tags
      await Assert.That(html).Contains("<h1>Welcome to Antigravity!</h1>");
      await Assert.That(html).Contains("<a href=\"https://docs.example.com\">Documentation</a>");
      await Assert.That(html).Contains("<pre><code class=\"language-csharp\">");

      // Verify Plain text has clean text and URLs
      var plainNormalized = plain.Replace("\r\n", "\n");
      await Assert.That(plainNormalized).Contains("Welcome to Antigravity!");
      await Assert.That(plainNormalized).Contains("Hi John,");
      await Assert.That(plainNormalized).Contains("Documentation (https://docs.example.com)");
      await Assert.That(plainNormalized).Contains("Discord (https://discord.gg/example)");
      await Assert.That(plainNormalized).Contains("here (https://example.com/verify?token=abc)");
      await Assert.That(plainNormalized).Contains("support@example.com");
      await Assert.That(plainNormalized).Contains("var client = new AntigravityClient();");
      await Assert.That(plainNormalized).Contains("---");
      await Assert.That(plainNormalized).Contains("The Antigravity Team");
      
      // Ensure no HTML tags leaked into plain text
      await Assert.That(plainNormalized).DoesNotContain("<h1>");
      await Assert.That(plainNormalized).DoesNotContain("</h1>");
      await Assert.That(plainNormalized).DoesNotContain("<p>");
      await Assert.That(plainNormalized).DoesNotContain("</p>");
      await Assert.That(plainNormalized).DoesNotContain("<a href=");
      await Assert.That(plainNormalized).DoesNotContain("<pre>");
      await Assert.That(plainNormalized).DoesNotContain("<code>");
      await Assert.That(plainNormalized).DoesNotContain("<em>");
      await Assert.That(plainNormalized).DoesNotContain("<strong>");
   }

   [Test]
   public async Task OverloadsAndSpanApisWorkIdentically()
   {
      const string markdown = "## Test Heading\n\nSome **bold** and [link](https://test.com).";

      var textFromString = BeMarkdown.ToPlainText(markdown);
      var textFromSpan = BeMarkdown.ToPlainText(markdown.AsSpan());
      var textContextual = BeMarkdown.ToContextualPlainText<object>(markdown.AsSpan());

      await Assert.That(textFromSpan).IsEqualTo(textFromString);
      await Assert.That(textContextual).IsEqualTo(textFromString);
   }

   [Test]
   public async Task ComplexNestedMarkdown()
   {
      const string markdown =
         """
         # Title

         > Quote with **bold** and [link](https://quote.com)

         | Item | Quantity | Price |
         | --- | --- | --- |
         | Widget A | 2 | $10.00 |
         | Widget B | 5 | $25.00 |

         - [x] Task 1 completed
         - [ ] Task 2 pending
         """;

      var plain = BeMarkdown.ToPlainText(markdown).Replace("\r\n", "\n");

      await Assert.That(plain).Contains("Title");
      await Assert.That(plain).Contains("Quote with bold and link (https://quote.com)");
      await Assert.That(plain).Contains("Item\tQuantity\tPrice");
      await Assert.That(plain).Contains("Widget A\t2\t$10.00");
      await Assert.That(plain).Contains("[x] Task 1 completed");
      await Assert.That(plain).Contains("[ ] Task 2 pending");
   }
}
