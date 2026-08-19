<h1>
<p align="center">
   <img src="https://github.com/MarvinDrude/Beskar.Markdown/blob/master/Resources/logo.png" alt="Logo" width="256" />
   <br />
   Beskar.Markdown
</p>
</h1>
<p align="center">
   Fast, <code>.NET 10</code> native, high-performance, low-allocation Markdown parser and HTML renderer.<br/>
   Built for modern <code>C#</code> with <code>Span&lt;T&gt;</code>, zero-copy parsing, AST extensions, and template variables.<br/>
   No external runtime dependencies besides <b>Beskar</b>.<br/><br/>
   <a href="#why-use-this-library">Why Use This Library</a>
   ·
   <a href="#motivation">Motivation</a>
   ·
   <a href="#getting-started">Getting Started</a>
   ·
   <a href="#features">Features</a>
   ·
   <a href="#frontmatter-parsing">Frontmatter</a>
   ·
   <a href="#sluggable-headers">Sluggable Headers</a>
   ·
   <a href="#template-variables">Variables</a>
   ·
   <a href="#code-block-intercept">Code Intercept</a>
   ·
   <a href="#simple-custom-markdown-extensions">Extensions</a>
   ·
   <a href="#benchmark-results">Benchmarks</a>
</p>
<br/>

---
<br/>

![.NET 10](https://img.shields.io/badge/.NET-10.0-blueviolet)
![Code Poetry](https://img.shields.io/badge/code-is_poetry-orange)
[![NuGet](https://img.shields.io/nuget/v/Beskar.Markdown)](https://www.nuget.org/packages/Beskar.Markdown)
![Issues](https://img.shields.io/github/issues/MarvinDrude/Beskar.Markdown)
![Repo Size](https://img.shields.io/github/repo-size/MarvinDrude/Beskar.Markdown.svg)
[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](https://raw.githubusercontent.com/MarvinDrude/Beskar.Markdown/master/LICENSE.md)

---

## Why use this library?

- **Performance First**: Designed for scenarios where every microsecond and every byte counts.
- **Low Allocation**: Minimizes pressure on the Garbage Collector by using stack-allocated buffers and pooling where possible.
- **Modern C#**: Built for modern .NET, taking advantage of the latest language and runtime optimizations.
- **Simplicity**: A clean, easy-to-use API that gets the job done without unnecessary complexity.
- **Tests**: 1,009 passing tests (652 **CommonMark** Spec Tests)

## Motivation

I created this library because I love to learn about deep-level code topics. 
Building a Markdown parser is a fantastic way to explore memory layout optimization, 
and the intricacies of text processing at scale. It's a passion project aimed at 
achieving technical excellence and pushing the boundaries of what's possible in .NET.

## Getting Started

Getting started with Beskar.Markdown is easy. Call the static `ToHtml` method:

```csharp
using Beskar.Markdown;

string markdown = "# Hello World\nThis is a **bold** statement.";
string html = BeMarkdown.ToHtml(markdown);

Console.WriteLine(html);
// Output: <h1>Hello World</h1><p>This is a <strong>bold</strong> statement.</p>
```

## Features

### Main Features
- **Fast**: Beskar.Markdown is designed to be fast and leaner than existing solutions.
- **Modern**: Built for modern .NET, taking advantage of the latest language and runtime optimizations.
- **Easy to Use**: A clean, intuitive API that makes Markdown processing straightforward.
- **Extensible**: Easily add support for new Markdown features or extensions.
- **Frontmatter**: Built-in support for parsing document frontmatter.
- **Sluggable Headers**: Automatically generate `id` attributes for headers.
- **Advanced**: Supports contextual rendering
- **Tests**: 1,009 passing tests (652 **CommonMark** Spec Tests)

### Currently Supported Blocks & Inlines
- **Blocks**:
    - Headers (ATX & Setext)
    - Paragraphs
    - Blockquotes
    - Lists (Ordered & Unordered)
    - Task list items (like GitHub)
    - Fenced Code Blocks
    - Indented Code Blocks
    - Thematic Breaks (Horizontal Rules)
    - HTML Blocks
    - GitHub like Tables
    - Full reference links
- **Inlines**:
    - Emphasis (Bold, Italic)
    - Links
    - Autolinks
    - Inline Code
    - Inline HTML
    - Line Breaks
    - Strikethrough
    - Images

### Future Plans
- [ ] In memory assembly baking

## ⚠️ Security Warning

**Important**: Beskar.Markdown does not perform HTML sanitization by default.
If you are processing Markdown input from untrusted users, you **MUST** sanitize
the resulting HTML to prevent Cross-Site Scripting (XSS) attacks.

Example:
```csharp
var rawHtml = BeMarkdown.ToHtml(userContent);
var sanitizer = new HtmlSanitizer();
var safeHtml = sanitizer.Sanitize(rawHtml);
```

If your sanitizer supports spans, you can use the following to prevent double allocation unlike above:
```csharp
var options = RenderOptions.HtmlDefault;
options.SanitizerFunc = (span) => HtmlSanitizer.Sanitize(span);

var safeHtml = BeMarkdown.ToHtml(userContent, renderOptions: options);
```

## Frontmatter Parsing

Beskar.Markdown can automatically parse YAML-like frontmatter into key-value pairs.
To enable this, use the `WithFrontMatter()` option and the `Parse` method:

```csharp
var options = MarkdownOptionBuilder.Create()
    .WithFrontMatter()
    .Build();

var markdown = """
               ---
               title: My Awesome Page
               author: Marvin
               ---
               # Content
               """;

var result = BeMarkdown.Parse(markdown, options);

Console.WriteLine(result.Context.FrontMatter["title"]); // My Awesome Page
Console.WriteLine(result.Html); // <h1>Content</h1>
```

## Sluggable Headers

Beskar.Markdown can automatically generate `id` attributes for headers based on their text content.
To enable this, use the `WithSluggableHeaders()` option:

```csharp
var options = MarkdownOptionBuilder.Create()
    .WithSluggableHeaders()
    .Build();

var markdown = "# My Header Text";
var html = BeMarkdown.ToHtml(markdown, options);

Console.WriteLine(html);
// Output: <h1 id="my-header-text">My Header Text</h1>
```

If you need a table of contents or anchor list, use `Parse` instead of `ToHtml`.
The returned context exposes headers in document order through `Context.Headers`.
Each item contains the generated slug, the plain text, and the heading level:

```csharp
var markdown = """
               # My Header Text
               ## Details
               """;

var result = BeMarkdown.Parse(markdown, options);

foreach (var header in result.Context.Headers)
{
    Console.WriteLine($"{header.Level}: {header.PlainText} -> #{header.Slug}");
}

// Output:
// 1: My Header Text -> #my-header-text
// 2: Details -> #details
```

## Template Variables

Beskar.Markdown supports optional context-rendered template variables using standard `{{ ... }}` syntax.

### Enabling the Feature

To preserve standard CommonMark handling by default, template variables are **disabled by default**.
To enable them, call `.WithVariables()` on `MarkdownOptionBuilder`:

```csharp
var options = MarkdownOptionBuilder.Create()
    .WithVariables()
    .Build();
```

### Supported Variable Formats

| Format | Syntax | Description |
|---|---|---|
| **Plain Text** | `{{name}}`<br>`{{name:text}}` | Replaces with value and **HTML-encodes** special characters (`<`, `>`, `&`, `"`, `'`). |
| **Markdown** | `{{name:md}}`<br>`{{name:markdown}}` | Parses and renders the value as **Markdown** into the HTML stream. |
| **Raw HTML** | `{{name::html}}`<br>`{{name:html}}` | Emits the value **1:1 directly** into the HTML output without escaping. |

> **Tip**: Whitespace inside braces is automatically trimmed (e.g. `{{ name }}`, `{{ name : md }}`, and `{{ name :: html }}` are all valid).

### Inline vs. Block-Level Usage

- **Inline Variables**: When placed within running text (e.g. `Hello {{name}}!`), plain text is encoded, and Markdown variables (`{{name:md}}`) embed their inline formatting without injecting unwanted outer `<p>` tags.
- **Block-Level Variables**: When placed **alone on their own line** (e.g. `{{content:md}}` or `{{widget::html}}`), they are treated as top-level blocks. Multi-line Markdown variables render headings, lists, tables, and paragraphs cleanly without being wrapped in an outer `<p>...</p>`.

### Supplying Context Variables

You can supply variable values in three ways:

1. **Dictionary Context**: Pass a dictionary directly to `ToContextualHtml` or `ParseContextual`:
   ```csharp
   var variables = new Dictionary<string, string>
   {
       ["user"] = "Marvin",
       ["badge"] = "**Admin**",
       ["icon"] = "<span class=\"badge\">★</span>"
   };

   var html = BeMarkdown.ToContextualHtml(markdown, options.ParserOptions, options.RenderOptions, variables);
   ```

2. **Context Variables Dictionary**:
   ```csharp
   var result = BeMarkdown.ParseContextual(markdown, options);
   result.Context.Variables["user"] = "Marvin";
   ```

3. **Dynamic Variable Resolver Delegate**:
   ```csharp
   using var parser = new MarkdownParser<object>(markdown, stackalloc MarkdownNode[32]);
   var context = parser.Parse(options.ParserOptions);
   context.VariableResolver = key => key switch
   {
       "time" => DateTime.UtcNow.ToString("yyyy-MM-dd"),
       "calc" => (40 + 2).ToString(),
       _ => null
   };
   ```

### Complete Example

```csharp
using Beskar.Markdown;
using Beskar.Markdown.Builders;

var options = MarkdownOptionBuilder.Create()
    .WithVariables()
    .Build();

var markdown = """
   # Welcome {{user}}!

   {{announcement:md}}

   {{card::html}}

   Contact: {{email}}
   """;

var variables = new Dictionary<string, string>
{
    ["user"] = "Alice",
    ["announcement"] = """
       ### Maintenance Notice
       The server will restart at **midnight**. Please save your work:
       - Item 1
       - Item 2
       """,
    ["card"] = "<div class=\"alert alert-info\">System Status: Green</div>",
    ["email"] = "support@example.com <do-not-reply>"
};

var html = BeMarkdown.ToContextualHtml(markdown, options.ParserOptions, options.RenderOptions, variables);

Console.WriteLine(html);
```

**Output:**

```html
<h1>Welcome Alice!</h1>
<h3>Maintenance Notice</h3>
<p>The server will restart at <strong>midnight</strong>. Please save your work:</p>
<ul>
<li>Item 1</li>
<li>Item 2</li>
</ul>
<div class="alert alert-info">System Status: Green</div>
<p>Contact: support@example.com &lt;do-not-reply&gt;</p>
```

## Code Block Intercept

You can intercept the rendering of code blocks (both fenced and indented) to provide your own 
HTML output. This is particularly useful for server-side syntax highlighting (e.g., using 
libraries like `ColorCode` or calling a highlighting service).

To use this, implement the `ICodeBlockRenderer` interface and register it via `WithCodeBlockRenderer`:

```csharp
public sealed class MySyntaxHighlighter : ICodeBlockRenderer
{
    public bool TryRender<TData>(
        MarkdownContext<TData> context,
        ref TextWriterIndentSlim writer,
        ReadOnlySpan<char> code,
        ReadOnlySpan<char> language)
    {
        // Intercept only C# blocks
        if (language.Equals("csharp", StringComparison.OrdinalIgnoreCase))
        {
            writer.Write("<div class=\"highlight\">");
            writer.WriteHtmlDecodedAndEncoded(language);
            writer.Write(":");
            
            // Your custom highlighting logic here
            writer.Write(code); 
            
            writer.Write("</div>");
            return true; // Successfully intercepted
        }

        return false; // Fallback to default renderer
    }
}

// Usage:
var options = MarkdownOptionBuilder.Create()
    .WithCodeBlockRenderer(new MySyntaxHighlighter())
    .Build();

var html = BeMarkdown.ToHtml("```csharp\nvar x = 1;\n```", options);
```

> **Note**: When writing the `language` span to the output, always use `writer.WriteHtmlDecodedAndEncoded(language)` 
> to ensure proper HTML encoding.


## Simple custom markdown extensions

### Simple inline extension

This example inline extension adds a random emoji if you use `.RandomEmoji.`:

```csharp
var options = MarkdownOptionBuilder.Create()
   .WithExtension(new EmojiInlineExtension())
   .Build();

const string markdown = 
   """
   Hello, World! .RandomEmoji.
   """;
var result = BeMarkdown.ToHtml(markdown, options);
Console.WriteLine(result); // <p>Hello, World! <span class="emoji">💻</span></p>
```

Implementation:
```csharp
public sealed class EmojiInlineExtension : BaseInlineExtension
{
   private const int _targetTypeValue = BeMarkdown.BuiltInNodeTypeValueOffset + 4;
   private static readonly ImmutableArray<string> _emojis = ImmutableArray.CreateRange([
      "😀", "🎉", "🚀", "🌟", "🔥", "🐱", "🍕", "💻", "☕"]);

   public EmojiInlineExtension()
   {
      Parsers = [new EmojiInlineParser()];
      Renderers = [new HtmlEmojiInlineRenderer()];
   }

   private sealed class HtmlEmojiInlineRenderer : INodeRenderer
   {
      public int TargetTypeValue => _targetTypeValue;

      public void Render<TData>(
         MarkdownContext<TData> context,
         ReadOnlySpan<char> rawText, 
         ref TextWriterIndentSlim writer, 
         in MarkdownNode current, 
         ReadOnlySpan<MarkdownNode> nodes,
         RenderOptions options)
      {
         writer.Write("<span class=\"emoji\">");
         writer.Write(_emojis[Random.Shared.Next(0, _emojis.Length)]);
         writer.Write("</span>");
      }
   }
   
   private sealed class EmojiInlineParser : IInlineParser
   {
      private const string _identifier = ".RandomEmoji.";
      
      public int Priority => 8_000;
      public int SupportedTypeValue => _targetTypeValue;

      public char TriggerChar => '.';
      public char TriggerAltChar => '.';

      public bool TryMatch<TData>(
         ref InlineState<TData> state, 
         int parentIndex, 
         ref BufferWriter<MarkdownNode> writer, 
         scoped ref InlineParser<TData> parser,
         ParserOptions options)
      {
         if (state.RemainingText.Length < _identifier.Length) 
            return false;
         
         if (!state.RemainingText.StartsWith(_identifier))
            return false;

         var nodeIndex = writer.WrittenSpan.Length;
         writer.Add(new MarkdownNode()
         {
            Type = (NodeType)SupportedTypeValue,
            TextSpan = new TextSpan(state.GlobalOffset, _identifier.Length),
            
            NextSiblingIndex = -1,
            FirstChildIndex = -1,
            LastChildIndex = -1
         });
         
         parser.LinkInlineNode(ref writer, parentIndex, nodeIndex);
         state.Advance(_identifier.Length);
         return true;
      }
   }
}
```

### Simple block extension

This example block extension adds a basic parsing for a div that is red:

```csharp
var options = MarkdownOptionBuilder.Create()
   .WithExtension(new RedBlockExtension())
   .WithMaxBlockDepth(16)
   .Build();

const string markdown = 
   """
   +red block+
   inside of `code` inline
   red
   > blockquote

   """;
var result = BeMarkdown.ToHtml(markdown, options);
Console.WriteLine(result); // <div class="red-block"><p>inside of <code>code</code> inline\nred</p><blockquote><p>blockquote</p></blockquote></div>
```

Implementation:
```csharp
public sealed class RedBlockExtension : BaseBlockExtension
{
   private const int _targetTypeValue = BeMarkdown.BuiltInNodeTypeValueOffset + 5;
   
   public RedBlockExtension()
   {
      Parsers = [new RedBlockParser()];
      Renderers = [new HtmlRedBlockRenderer()];
   }

   private sealed class HtmlRedBlockRenderer : INodeRenderer
   {
      public int TargetTypeValue => _targetTypeValue;

      public void Render<TData>(
         MarkdownContext<TData> context,
         ReadOnlySpan<char> rawText, 
         ref TextWriterIndentSlim writer, 
         in MarkdownNode current, 
         ReadOnlySpan<MarkdownNode> nodes,
         RenderOptions options)
      {
         writer.Write("<div class=\"red-block\">");
         current.RenderChildren(context, rawText, nodes, ref writer, options);
         writer.Write("</div>");
      }
   }

   private sealed class RedBlockParser : IBlockParser
   {
      private const string _identifier = "+red block+";
      
      public int Priority => 10; // low priority
      public int SupportedTypeValue => _targetTypeValue;
      
      public int TryMatch<TData>(ref LineState<TData> state, int parentIndex, ref BufferWriter<MarkdownNode> writer)
      {
         if (state.IsBlank || state.LeadingSpaces > 0)
         {
            return -1;
         }
         
         if (state.FirstChar != '+' && state.RawLine.Length < _identifier.Length)
         {
            return -1;
         }
         
         if (!state.RawLine.StartsWith(_identifier))
         {
            return -1;
         }
         
         var nodeIndex = writer.WrittenSpan.Length;
         writer.Add(new MarkdownNode()
         {
            Type = (NodeType)SupportedTypeValue,
            TextSpan = new TextSpan(-1, 0),
            
            FirstChildIndex = -1,
            NextSiblingIndex = -1,
            LastChildIndex = -1
         });

         state.ConsumeRest();
         return nodeIndex;
      }

      public bool CanContinue<TData>(ref MarkdownNode node, ref LineState<TData> state, ref BufferWriter<MarkdownNode> writer)
      {
         // simple example only an empty line can stop the block
         if (state.IsBlank)
         {
            return false;
         }
         
         if (node.TextSpan.Start == -1)
         {
            node.TextSpan = new TextSpan(state.GlobalOffset, state.RawLine.Length);
         }
         else
         {
            var newLength = (state.GlobalOffset - node.TextSpan.Start) + state.RawLine.Length;
            node.TextSpan = node.TextSpan with { Length = newLength };
         }

         return true;
      }
   }
}
```

## Benchmark Results

Beskar.Markdown is designed to be fast and leaner than existing solutions. 
Here is a comparison with some other libraries (especially when it comes to memory usage):

| Method          | Categories | Mean           | Rank | Gen0      | Gen1      | Gen2      | Allocated  |
|---------------- |----------- |---------------:|-----:|----------:|----------:|----------:|-----------:|
| Markdig         | Bigger     |     222.117 us |    2 |         - |         - |         - |    37256 B |
| Beskar.Markdown | Bigger     |      95.211 us |    1 |         - |         - |         - |     5288 B |
| CommonMark.Net  | Bigger     |      86.785 us |    1 |         - |         - |         - |    66024 B |
| MarkdownSharp   | Bigger     |     429.370 us |    3 |         - |         - |         - |   273996 B |
|                 |            |                |      |           |           |           |            |
| Markdig         | Full Spec  |   1,919.994 us |    3 |  125.0000 |  125.0000 |  125.0000 |  2077013 B |
| Beskar.Markdown | Full Spec  |   1,302.109 us |    1 |   31.2500 |   31.2500 |   31.2500 |   437967 B |
| CommonMark.Net  | Full Spec  |   1,577.537 us |    2 |  125.0000 |  125.0000 |  125.0000 |  3153930 B |
| MarkdownSharp   | Full Spec  | 346,914.621 us |    4 | 1656.2500 | 1625.0000 | 1468.7500 | 15909676 B |
|                 |            |                |      |           |           |           |            |
| Markdig         | Small      |       5.425 us |    3 |         - |         - |         - |     1144 B |
| Beskar.Markdown | Small      |       2.736 us |    2 |         - |         - |         - |      504 B |
| CommonMark.Net  | Small      |       1.768 us |    1 |         - |         - |         - |    11488 B |
| MarkdownSharp   | Small      |       5.056 us |    3 |         - |         - |         - |     2752 B |

