using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
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
   public async Task DisabledByDefaultStandaloneLineRendersParagraph()
   {
      const string markdown = "{{name:md}}";
      var html = BeMarkdown.ToHtml(markdown);

      await Assert.That(html.Trim()).IsEqualTo("<p>{{name:md}}</p>");
   }

   [Test]
   public async Task ExplicitlyDisabledBuilderRendersLiteral()
   {
      const string markdown = "Hello {{name}}!";
      var options = MarkdownOptionBuilder.Create()
         .WithVariables(false)
         .Build();

      var html = BeMarkdown.ToHtml(markdown, options);

      await Assert.That(html.Trim()).IsEqualTo("<p>Hello {{name}}!</p>");
   }

   [Test]
   public async Task PlainTextVariableReplaced()
   {
      const string markdown = "Hello {{name}}!";
      var options = MarkdownOptionBuilder.Create()
         .WithVariables()
         .Build();

      var variables = new Dictionary<string, string>
      {
         ["name"] = "World"
      };

      var html = BeMarkdown.ToContextualHtml(markdown.AsSpan(), options.ParserOptions, options.RenderOptions, variables);

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
   public async Task PlainTextExplicitModifier()
   {
      const string markdown = "A: {{a:text}} B: {{b:plain}}";
      var options = MarkdownOptionBuilder.Create()
         .WithVariables()
         .Build();

      var variables = new Dictionary<string, string>
      {
         ["a"] = "First",
         ["b"] = "<Second>"
      };

      var html = BeMarkdown.ToContextualHtml(markdown.AsSpan(), options.ParserOptions, options.RenderOptions, variables);

      await Assert.That(html.Trim()).IsEqualTo("<p>A: First B: &lt;Second&gt;</p>");
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
   public async Task VariableFromReadOnlyDictionary()
   {
      const string markdown = "Hello {{user}}!";
      var options = MarkdownOptionBuilder.Create()
         .WithVariables()
         .Build();

      IReadOnlyDictionary<string, string> dict = new ReadOnlyDictionary<string, string>(new Dictionary<string, string>
      {
         ["user"] = "Drude"
      });

      var html = BeMarkdown.ToContextualHtml(markdown.AsSpan(), options.ParserOptions, options.RenderOptions, dict);

      await Assert.That(html.Trim()).IsEqualTo("<p>Hello Drude!</p>");
   }

   [Test]
   public async Task VariableFromObjectDictionary()
   {
      const string markdown = "Score: {{score}} items: {{items}}";
      var options = MarkdownOptionBuilder.Create()
         .WithVariables()
         .Build();

      var dict = new Dictionary<string, object>
      {
         ["score"] = "99.5",
         ["items"] = 4
      };

      var html = BeMarkdown.ToContextualHtml(markdown.AsSpan(), options.ParserOptions, options.RenderOptions, dict);

      await Assert.That(html.Trim()).IsEqualTo("<p>Score: 99.5 items: 4</p>");
   }

   [Test]
   public async Task VariableCaseInsensitiveModifiers()
   {
      const string markdown = "{{a:MD}} {{b:MARKDOWN}} {{c:HTML}} {{d::HTML}}";
      var options = MarkdownOptionBuilder.Create()
         .WithVariables()
         .Build();

      var variables = new Dictionary<string, string>
      {
         ["a"] = "*one*",
         ["b"] = "**two**",
         ["c"] = "<span>three</span>",
         ["d"] = "<span>four</span>"
      };

      var html = BeMarkdown.ToContextualHtml(markdown.AsSpan(), options.ParserOptions, options.RenderOptions, variables);

      await Assert.That(html.Trim()).IsEqualTo("<p><em>one</em> <strong>two</strong> <span>three</span> <span>four</span></p>");
   }

   [Test]
   public async Task VariableWithPunctuationInName()
   {
      const string markdown = "{{user.first-name}} {{user.profile_url:md}}";
      var options = MarkdownOptionBuilder.Create()
         .WithVariables()
         .Build();

      var variables = new Dictionary<string, string>
      {
         ["user.first-name"] = "John",
         ["user.profile_url"] = "[profile](https://example.com/john)"
      };

      var html = BeMarkdown.ToContextualHtml(markdown.AsSpan(), options.ParserOptions, options.RenderOptions, variables);

      await Assert.That(html.Trim()).IsEqualTo("<p>John <a href=\"https://example.com/john\">profile</a></p>");
   }

   [Test]
   public async Task StandaloneBlockMarkdownVariable()
   {
      const string markdown = """
         # Intro

         {{section:md}}

         ## Outro
         """;

      var options = MarkdownOptionBuilder.Create()
         .WithVariables()
         .Build();

      var variables = new Dictionary<string, string>
      {
         ["section"] = """
            ### Dynamic Subtitle

            Dynamic paragraph with **bold**.

            - Item A
            - Item B
            """
      };

      var html = BeMarkdown.ToContextualHtml(markdown.AsSpan(), options.ParserOptions, options.RenderOptions, variables);

      await Assert.That(html).Contains("<h1>Intro</h1>");
      await Assert.That(html).Contains("<h3>Dynamic Subtitle</h3>");
      await Assert.That(html).Contains("<p>Dynamic paragraph with <strong>bold</strong>.</p>");
      await Assert.That(html).Contains("<li>Item A</li>");
      await Assert.That(html).Contains("<li>Item B</li>");
      await Assert.That(html).Contains("<h2>Outro</h2>");
      await Assert.That(html).DoesNotContain("<p><h3>Dynamic Subtitle</h3>");
   }

   [Test]
   public async Task StandaloneBlockHtmlVariable()
   {
      const string markdown = """
         # Dashboard

         {{card::html}}

         Footer text
         """;

      var options = MarkdownOptionBuilder.Create()
         .WithVariables()
         .Build();

      var variables = new Dictionary<string, string>
      {
         ["card"] = "<div class=\"card\"><div class=\"card-body\">Stats</div></div>"
      };

      var html = BeMarkdown.ToContextualHtml(markdown.AsSpan(), options.ParserOptions, options.RenderOptions, variables);

      await Assert.That(html).Contains("<h1>Dashboard</h1>");
      await Assert.That(html).Contains("<div class=\"card\"><div class=\"card-body\">Stats</div></div>");
      await Assert.That(html).Contains("<p>Footer text</p>");
      await Assert.That(html).DoesNotContain("<p><div class=\"card\">");
   }

   [Test]
   public async Task StandaloneBlockPlainTextVariable()
   {
      const string markdown = """
         # Header

         {{announcement}}

         Footer
         """;

      var options = MarkdownOptionBuilder.Create()
         .WithVariables()
         .Build();

      var variables = new Dictionary<string, string>
      {
         ["announcement"] = "Important notice <safe>"
      };

      var html = BeMarkdown.ToContextualHtml(markdown.AsSpan(), options.ParserOptions, options.RenderOptions, variables);

      await Assert.That(html).Contains("<h1>Header</h1>");
      await Assert.That(html).Contains("<p>Important notice &lt;safe&gt;</p>");
      await Assert.That(html).Contains("<p>Footer</p>");
   }

   [Test]
   public async Task MultipleConsecutiveBlockVariables()
   {
      const string markdown = """
         {{first:md}}

         {{second::html}}

         {{third}}
         """;

      var options = MarkdownOptionBuilder.Create()
         .WithVariables()
         .Build();

      var variables = new Dictionary<string, string>
      {
         ["first"] = "### Header One",
         ["second"] = "<hr class=\"custom\" />",
         ["third"] = "Third block"
      };

      var html = BeMarkdown.ToContextualHtml(markdown.AsSpan(), options.ParserOptions, options.RenderOptions, variables);

      await Assert.That(html).Contains("<h3>Header One</h3>");
      await Assert.That(html).Contains("<hr class=\"custom\" />");
      await Assert.That(html).Contains("<p>Third block</p>");
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
   public async Task VariablesInsideBlockquotes()
   {
      const string markdown = """
         > {{quote:md}}
         > Author: {{author}}
         """;

      var options = MarkdownOptionBuilder.Create()
         .WithVariables()
         .Build();

      var variables = new Dictionary<string, string>
      {
         ["quote"] = "**Stay hungry, stay foolish.**",
         ["author"] = "Steve Jobs"
      };

      var html = BeMarkdown.ToContextualHtml(markdown.AsSpan(), options.ParserOptions, options.RenderOptions, variables);

      await Assert.That(html).Contains("<blockquote>");
      await Assert.That(html).Contains("<strong>Stay hungry, stay foolish.</strong>");
      await Assert.That(html).Contains("Author: Steve Jobs");
      await Assert.That(html).Contains("</blockquote>");
   }

   [Test]
   public async Task VariablesInsideTables()
   {
      const string markdown = """
         | Name | Role | Status |
         | --- | --- | --- |
         | {{name}} | {{role:md}} | {{badge::html}} |
         """;

      var options = MarkdownOptionBuilder.Create()
         .WithVariables()
         .Build();

      var variables = new Dictionary<string, string>
      {
         ["name"] = "Alice & Bob",
         ["role"] = "**Admin**",
         ["badge"] = "<span class=\"active\">Active</span>"
      };

      var html = BeMarkdown.ToContextualHtml(markdown.AsSpan(), options.ParserOptions, options.RenderOptions, variables);

      await Assert.That(html).Contains("<td>Alice &amp; Bob</td>");
      await Assert.That(html).Contains("<td><strong>Admin</strong></td>");
      await Assert.That(html).Contains("<td><span class=\"active\">Active</span></td>");
   }

   [Test]
   public async Task VariablesInsideEmphasisAndFormatting()
   {
      const string markdown = "*{{italic}}* **{{bold}}** ~~{{strike}}~~ `{{code}}`";
      var options = MarkdownOptionBuilder.Create()
         .WithVariables()
         .Build();

      var variables = new Dictionary<string, string>
      {
         ["italic"] = "ItalicText",
         ["bold"] = "BoldText",
         ["strike"] = "StrikedText",
         ["code"] = "CodeText"
      };

      var html = BeMarkdown.ToContextualHtml(markdown.AsSpan(), options.ParserOptions, options.RenderOptions, variables);

      await Assert.That(html).Contains("<em>ItalicText</em>");
      await Assert.That(html).Contains("<strong>BoldText</strong>");
      await Assert.That(html).Contains("<del>StrikedText</del>");
      await Assert.That(html).Contains("<code>{{code}}</code>");
   }

   [Test]
   public async Task VariablesInsideLinks()
   {
      const string markdown = "[Visit {{site}}](https://example.com/{{path}})";
      var options = MarkdownOptionBuilder.Create()
         .WithVariables()
         .Build();

      var variables = new Dictionary<string, string>
      {
         ["site"] = "Portal",
         ["path"] = "dashboard"
      };

      var html = BeMarkdown.ToContextualHtml(markdown.AsSpan(), options.ParserOptions, options.RenderOptions, variables);

      await Assert.That(html.Trim()).IsEqualTo("<p><a href=\"https://example.com/{{path}}\">Visit Portal</a></p>");
   }

   [Test]
   public async Task VariablesInsideCodeBlocksPreserved()
   {
      const string markdown = """
         ```csharp
         var name = "{{name}}";
         var format = "{{format:md}}";
         ```
         """;

      var options = MarkdownOptionBuilder.Create()
         .WithVariables()
         .Build();

      var variables = new Dictionary<string, string>
      {
         ["name"] = "ShouldNotReplace",
         ["format"] = "ShouldNotReplace"
      };

      var html = BeMarkdown.ToContextualHtml(markdown.AsSpan(), options.ParserOptions, options.RenderOptions, variables);

      await Assert.That(html).Contains("{{name}}");
      await Assert.That(html).Contains("{{format:md}}");
      await Assert.That(html).DoesNotContain("ShouldNotReplace");
   }

   [Test]
   public async Task VariablesInsideInlineCodePreserved()
   {
      const string markdown = "Here is `{{name}}` and `{{var:md}}` code";

      var options = MarkdownOptionBuilder.Create()
         .WithVariables()
         .Build();

      var variables = new Dictionary<string, string>
      {
         ["name"] = "ShouldNotReplace",
         ["var"] = "ShouldNotReplace"
      };

      var html = BeMarkdown.ToContextualHtml(markdown.AsSpan(), options.ParserOptions, options.RenderOptions, variables);

      await Assert.That(html.Trim()).IsEqualTo("<p>Here is <code>{{name}}</code> and <code>{{var:md}}</code> code</p>");
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

   [Test]
   public async Task UnclosedCurlyBracesHandledAsLiteralText()
   {
      const string markdown = "Unclosed {{name and {{other:md";
      var options = MarkdownOptionBuilder.Create()
         .WithVariables()
         .Build();

      var html = BeMarkdown.ToContextualHtml(markdown.AsSpan(), options.ParserOptions, options.RenderOptions, new Dictionary<string, string>());

      await Assert.That(html.Trim()).IsEqualTo("<p>Unclosed {{name and {{other:md</p>");
   }

   [Test]
   public async Task SingleCurlyBracesHandledAsLiteralText()
   {
      const string markdown = "Single {name} and {other:md}";
      var options = MarkdownOptionBuilder.Create()
         .WithVariables()
         .Build();

      var html = BeMarkdown.ToContextualHtml(markdown.AsSpan(), options.ParserOptions, options.RenderOptions, new Dictionary<string, string>());

      await Assert.That(html.Trim()).IsEqualTo("<p>Single {name} and {other:md}</p>");
   }

   [Test]
   public async Task TripleCurlyBracesHandled()
   {
      const string markdown = "{{{user}}}";
      var options = MarkdownOptionBuilder.Create()
         .WithVariables()
         .Build();

      var variables = new Dictionary<string, string>
      {
         ["user"] = "Marvin"
      };

      var html = BeMarkdown.ToContextualHtml(markdown.AsSpan(), options.ParserOptions, options.RenderOptions, variables);

      await Assert.That(html.Trim()).IsEqualTo("<p>Marvin}</p>");
   }

   [Test]
   public async Task MarkdownVariableWithComplexContent()
   {
      const string markdown = "{{data:md}}";
      var options = MarkdownOptionBuilder.Create()
         .WithVariables()
         .Build();

      var variables = new Dictionary<string, string>
      {
         ["data"] = """
            | Item | Price |
            | --- | --- |
            | Apple | $1.00 |

            > Quote inside variable
            """
      };

      var html = BeMarkdown.ToContextualHtml(markdown.AsSpan(), options.ParserOptions, options.RenderOptions, variables);

      await Assert.That(html).Contains("<table>");
      await Assert.That(html).Contains("<td>Apple</td>");
      await Assert.That(html).Contains("<blockquote>");
      await Assert.That(html).Contains("Quote inside variable");
   }

   [Test]
   public async Task CombinedWithFrontMatterAndSluggableHeaders()
   {
      const string markdown = """
         ---
         title: Frontmatter Title
         ---
         # {{title}}

         Welcome {{user}}!
         """;

      var options = MarkdownOptionBuilder.Create()
         .WithFrontMatter()
         .WithSluggableHeaders()
         .WithVariables()
         .Build();

      var variables = new Dictionary<string, string>
      {
         ["title"] = "Dynamic Header",
         ["user"] = "Alice"
      };

      var result = BeMarkdown.ParseContextual(markdown.AsSpan(), options, variables);

      await Assert.That(result.Context.FrontMatter["title"]).IsEqualTo("Frontmatter Title");
      await Assert.That(result.Html).Contains("<h1 id=\"section\">Dynamic Header</h1>");
      await Assert.That(result.Html).Contains("<p>Welcome Alice!</p>");
   }

   [Test]
   public async Task BlockVariableInOrderedList()
   {
      const string markdown = """
         1. First
         2. {{second:md}}
         3. Third
         """;

      var options = MarkdownOptionBuilder.Create()
         .WithVariables()
         .Build();

      var variables = new Dictionary<string, string>
      {
         ["second"] = "**Bold Second**"
      };

      var html = BeMarkdown.ToContextualHtml(markdown.AsSpan(), options.ParserOptions, options.RenderOptions, variables);

      await Assert.That(html).Contains("<li>First</li>");
      await Assert.That(html).Contains("<strong>Bold Second</strong>");
      await Assert.That(html).Contains("<li>Third</li>");
   }

   [Test]
   public async Task BlockVariableWithMultipleHeadingsAndParagraphs()
   {
      const string markdown = """
         Top Header

         {{body:md}}

         Bottom Footer
         """;

      var options = MarkdownOptionBuilder.Create()
         .WithVariables()
         .Build();

      var variables = new Dictionary<string, string>
      {
         ["body"] = """
            # Heading 1
            Text 1

            ## Heading 2
            Text 2
            """
      };

      var html = BeMarkdown.ToContextualHtml(markdown.AsSpan(), options.ParserOptions, options.RenderOptions, variables);

      await Assert.That(html).Contains("<p>Top Header</p>");
      await Assert.That(html).Contains("<h1>Heading 1</h1>");
      await Assert.That(html).Contains("<p>Text 1</p>");
      await Assert.That(html).Contains("<h2>Heading 2</h2>");
      await Assert.That(html).Contains("<p>Text 2</p>");
      await Assert.That(html).Contains("<p>Bottom Footer</p>");
   }

   [Test]
   public async Task BlockVariableContainingFencedCode()
   {
      const string markdown = """
         # Code Section

         {{code_block:md}}
         """;

      var options = MarkdownOptionBuilder.Create()
         .WithVariables()
         .Build();

      var variables = new Dictionary<string, string>
      {
         ["code_block"] = """
            ```csharp
            var x = 10;
            ```
            """
      };

      var html = BeMarkdown.ToContextualHtml(markdown.AsSpan(), options.ParserOptions, options.RenderOptions, variables);

      await Assert.That(html).Contains("<pre><code class=\"language-csharp\">var x = 10;\n</code></pre>");
   }

   [Test]
   public async Task BlockVariableWithHtmlBlock()
   {
      const string markdown = """
         {{alert:md}}
         """;

      var options = MarkdownOptionBuilder.Create()
         .WithVariables()
         .Build();

      var variables = new Dictionary<string, string>
      {
         ["alert"] = """
            <div class="alert">
               <p>Alert text</p>
            </div>
            """
      };

      var html = BeMarkdown.ToContextualHtml(markdown.AsSpan(), options.ParserOptions, options.RenderOptions, variables);

      await Assert.That(html).Contains("<div class=\"alert\">");
      await Assert.That(html).Contains("<p>Alert text</p>");
      await Assert.That(html).Contains("</div>");
   }

   [Test]
   public async Task EmptyVariableValueInBlockRendersNothing()
   {
      const string markdown = """
         Before

         {{empty_var:md}}

         After
         """;

      var options = MarkdownOptionBuilder.Create()
         .WithVariables()
         .Build();

      var variables = new Dictionary<string, string>
      {
         ["empty_var"] = ""
      };

      var html = BeMarkdown.ToContextualHtml(markdown.AsSpan(), options.ParserOptions, options.RenderOptions, variables);

      await Assert.That(html).Contains("<p>Before</p>");
      await Assert.That(html).Contains("<p>After</p>");
      await Assert.That(html).DoesNotContain("<p></p>");
   }

   [Test]
   public async Task InlineVariableInsideTableHeaderAndBody()
   {
      const string markdown = """
         | {{h1}} | {{h2}} |
         | --- | --- |
         | {{r1}} | {{r2}} |
         """;

      var options = MarkdownOptionBuilder.Create()
         .WithVariables()
         .Build();

      var variables = new Dictionary<string, string>
      {
         ["h1"] = "Col A",
         ["h2"] = "Col B",
         ["r1"] = "Val 1",
         ["r2"] = "Val 2"
      };

      var html = BeMarkdown.ToContextualHtml(markdown.AsSpan(), options.ParserOptions, options.RenderOptions, variables);

      await Assert.That(html).Contains("<th>Col A</th>");
      await Assert.That(html).Contains("<th>Col B</th>");
      await Assert.That(html).Contains("<td>Val 1</td>");
      await Assert.That(html).Contains("<td>Val 2</td>");
   }
}
