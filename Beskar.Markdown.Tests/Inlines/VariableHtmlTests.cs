using System.Collections.Generic;
using System.Threading.Tasks;
using Beskar.Markdown.Builders;
using Beskar.Markdown.Parsing;
using Beskar.Markdown.Parsing.Models;
using Beskar.Markdown.Rendering;

namespace Beskar.Markdown.Tests.Inlines;

public sealed class VariableHtmlTests
{
   [Test]
   public async Task DisabledByDefaultRendersLiteral()
   {
      const string markdown = "Hello {{name}}!";
      var html = BeMarkdown.ToHtml(markdown);

      await Assert.That(html.Trim()).IsEqualTo("<p>Hello {{name}}!</p>");
   }

   [Test]
   public async Task PlainTextVariableReplaced()
   {
      const string markdown = "Hello {{name}}!";
      var options = MarkdownOptionBuilder.Create()
         .WithVariables()
         .Build();

      var context = new MarkdownContext<object>();
      context.Variables["name"] = "World";

      var result = BeMarkdown.ParseContextual(markdown.AsSpan(), options, context.Data);
      result.Context.Variables["name"] = "World";
      var html = BeMarkdown.ToContextualHtml(markdown.AsSpan(), options.ParserOptions, options.RenderOptions, context.Variables);

      await Assert.That(html.Trim()).IsEqualTo("<p>Hello World!</p>");
   }

   [Test]
   public async Task PlainTextVariableEscapesHtml()
   {
      const string markdown = "Result: {{content}}";
      var options = MarkdownOptionBuilder.Create()
         .WithVariables()
         .Build();

      var variables = new Dictionary<string, string>
      {
         ["content"] = "<script>alert('xss')</script> & <b>bold</b>"
      };

      var html = BeMarkdown.ToContextualHtml(markdown.AsSpan(), options.ParserOptions, options.RenderOptions, variables);

      await Assert.That(html.Trim()).IsEqualTo("<p>Result: &lt;script&gt;alert('xss')&lt;/script&gt; &amp; &lt;b&gt;bold&lt;/b&gt;</p>");
   }

   [Test]
   public async Task MarkdownVariableRendersFormattedHtml()
   {
      const string markdown = "Welcome {{intro:md}}!";
      var options = MarkdownOptionBuilder.Create()
         .WithVariables()
         .Build();

      var variables = new Dictionary<string, string>
      {
         ["intro"] = "**bold** and *italic*"
      };

      var html = BeMarkdown.ToContextualHtml(markdown.AsSpan(), options.ParserOptions, options.RenderOptions, variables);

      await Assert.That(html.Trim()).IsEqualTo("<p>Welcome <strong>bold</strong> and <em>italic</em>!</p>");
   }

   [Test]
   public async Task MarkdownVariableAlternativeKeyword()
   {
      const string markdown = "Welcome {{intro:markdown}}!";
      var options = MarkdownOptionBuilder.Create()
         .WithVariables()
         .Build();

      var variables = new Dictionary<string, string>
      {
         ["intro"] = "`code block`"
      };

      var html = BeMarkdown.ToContextualHtml(markdown.AsSpan(), options.ParserOptions, options.RenderOptions, variables);

      await Assert.That(html.Trim()).IsEqualTo("<p>Welcome <code>code block</code>!</p>");
   }

   [Test]
   public async Task HtmlVariableRendersRawOneToOne()
   {
      const string markdown = "Header: {{custom::html}} end";
      var options = MarkdownOptionBuilder.Create()
         .WithVariables()
         .Build();

      var variables = new Dictionary<string, string>
      {
         ["custom"] = "<span class=\"badge\">NEW</span>"
      };

      var html = BeMarkdown.ToContextualHtml(markdown.AsSpan(), options.ParserOptions, options.RenderOptions, variables);

      await Assert.That(html.Trim()).IsEqualTo("<p>Header: <span class=\"badge\">NEW</span> end</p>");
   }

   [Test]
   public async Task HtmlVariableSingleColon()
   {
      const string markdown = "Header: {{custom:html}} end";
      var options = MarkdownOptionBuilder.Create()
         .WithVariables()
         .Build();

      var variables = new Dictionary<string, string>
      {
         ["custom"] = "<div id=\"test\">raw</div>"
      };

      var html = BeMarkdown.ToContextualHtml(markdown.AsSpan(), options.ParserOptions, options.RenderOptions, variables);

      await Assert.That(html.Trim()).IsEqualTo("<p>Header: <div id=\"test\">raw</div> end</p>");
   }

   [Test]
   public async Task VariableWithWhitespace()
   {
      const string markdown = "A: {{ name }} B: {{ user : md }} C: {{ badge :: html }}";
      var options = MarkdownOptionBuilder.Create()
         .WithVariables()
         .Build();

      var variables = new Dictionary<string, string>
      {
         ["name"] = "Alice",
         ["user"] = "**Bob**",
         ["badge"] = "<span>VIP</span>"
      };

      var html = BeMarkdown.ToContextualHtml(markdown.AsSpan(), options.ParserOptions, options.RenderOptions, variables);

      await Assert.That(html.Trim()).IsEqualTo("<p>A: Alice B: <strong>Bob</strong> C: <span>VIP</span></p>");
   }

   [Test]
   public async Task MissingVariableRendersEmpty()
   {
      const string markdown = "Before {{unknown}} after";
      var options = MarkdownOptionBuilder.Create()
         .WithVariables()
         .Build();

      var html = BeMarkdown.ToContextualHtml(markdown.AsSpan(), options.ParserOptions, options.RenderOptions, new Dictionary<string, string>());

      await Assert.That(html.Trim()).IsEqualTo("<p>Before  after</p>");
   }

   [Test]
   public async Task VariableResolverCallback()
   {
      const string markdown = "Val: {{calc}}";
      var options = MarkdownOptionBuilder.Create()
         .WithVariables()
         .Build();

      string html;
      {
         using var parser = new MarkdownParser<object>(markdown.AsSpan(), new MarkdownNode[32]);
         var context = parser.Parse(options.ParserOptions);
         context.VariableResolver = key => key == "calc" ? "42" : null;

         var renderer = new MarkdownRenderer(markdown.AsSpan());
         html = renderer.Render(context, parser.WrittenNodes, options.RenderOptions);
      }

      await Assert.That(html.Trim()).IsEqualTo("<p>Val: 42</p>");
   }

   [Test]
   public async Task VariablesInsideHeadingsAndLists()
   {
      const string markdown = """
         # Title for {{user}}

         - Item 1: {{item1}}
         - Item 2: {{item2:md}}
         """;

      var options = MarkdownOptionBuilder.Create()
         .WithVariables()
         .Build();

      var variables = new Dictionary<string, string>
      {
         ["user"] = "Marvin",
         ["item1"] = "First",
         ["item2"] = "*Second*"
      };

      var html = BeMarkdown.ToContextualHtml(markdown.AsSpan(), options.ParserOptions, options.RenderOptions, variables);

      await Assert.That(html).Contains("<h1>Title for Marvin</h1>");
      await Assert.That(html).Contains("<li>Item 1: First</li>");
      await Assert.That(html).Contains("<li>Item 2: <em>Second</em></li>");
   }

   [Test]
   public async Task VariablesInsideTables()
   {
      const string markdown = """
         | Name | Role |
         | --- | --- |
         | {{name}} | {{role:md}} |
         """;

      var options = MarkdownOptionBuilder.Create()
         .WithVariables()
         .Build();

      var variables = new Dictionary<string, string>
      {
         ["name"] = "Alice",
         ["role"] = "**Admin**"
      };

      var html = BeMarkdown.ToContextualHtml(markdown.AsSpan(), options.ParserOptions, options.RenderOptions, variables);

      await Assert.That(html).Contains("<td>Alice</td>");
      await Assert.That(html).Contains("<td><strong>Admin</strong></td>");
   }

   [Test]
   public async Task VariablesInsideCodeBlocksPreserved()
   {
      const string markdown = """
         ```
         {{name}}
         ```
         """;

      var options = MarkdownOptionBuilder.Create()
         .WithVariables()
         .Build();

      var variables = new Dictionary<string, string>
      {
         ["name"] = "ShouldNotReplace"
      };

      var html = BeMarkdown.ToContextualHtml(markdown.AsSpan(), options.ParserOptions, options.RenderOptions, variables);

      await Assert.That(html).Contains("{{name}}");
      await Assert.That(html).DoesNotContain("ShouldNotReplace");
   }

   [Test]
   public async Task VariablesInsideInlineCodePreserved()
   {
      const string markdown = "Here is `{{name}}` code";

      var options = MarkdownOptionBuilder.Create()
         .WithVariables()
         .Build();

      var variables = new Dictionary<string, string>
      {
         ["name"] = "ShouldNotReplace"
      };

      var html = BeMarkdown.ToContextualHtml(markdown.AsSpan(), options.ParserOptions, options.RenderOptions, variables);

      await Assert.That(html.Trim()).IsEqualTo("<p>Here is <code>{{name}}</code> code</p>");
   }

   [Test]
   public async Task NestedMarkdownVariables()
   {
      const string markdown = "Outer: {{content:md}}";

      var options = MarkdownOptionBuilder.Create()
         .WithVariables()
         .Build();

      var variables = new Dictionary<string, string>
      {
         ["content"] = "Inner {{inner}}",
         ["inner"] = "Value"
      };

      var html = BeMarkdown.ToContextualHtml(markdown.AsSpan(), options.ParserOptions, options.RenderOptions, variables);

      await Assert.That(html.Trim()).IsEqualTo("<p>Outer: Inner Value</p>");
   }

   [Test]
   public async Task MultipleVariablesInSameParagraph()
   {
      const string markdown = "{{greeting}}, {{first}} {{last}}! Check {{link:md}} or {{tag::html}}.";

      var options = MarkdownOptionBuilder.Create()
         .WithVariables()
         .Build();

      var variables = new Dictionary<string, string>
      {
         ["greeting"] = "Hi",
         ["first"] = "John",
         ["last"] = "Doe",
         ["link"] = "[docs](https://example.com)",
         ["tag"] = "<em>raw</em>"
      };

      var html = BeMarkdown.ToContextualHtml(markdown.AsSpan(), options.ParserOptions, options.RenderOptions, variables);

      await Assert.That(html.Trim()).IsEqualTo("<p>Hi, John Doe! Check <a href=\"https://example.com\">docs</a> or <em>raw</em>.</p>");
   }
}
