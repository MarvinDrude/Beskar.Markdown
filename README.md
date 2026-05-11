# Beskar.Markdown

Beskar.Markdown is a high-performance, low-allocation Markdown parser and HTML renderer 
for .NET. It is built from the ground up to leverage modern C# features like `Span<T>`, 
`ReadOnlySequence<T>`, and efficient memory management to provide a blazing-fast experience.

## Table of Contents

- [Why use this library?](#why-use-this-library)
- [Motivation](#motivation)
- [Getting Started](#getting-started)
- [Features](#features)
  - [Main Features](#main-features)
  - [Currently Supported Blocks & Inlines](#currently-supported-blocks--inlines)
  - [Future Plans](#future-plans)
- [⚠️ Security Warning](#security-warning)
- [Simple custom markdown extensions](#simple-custom-markdown-extensions)
  - [Simple inline extension](#simple-inline-extension)
  - [Simple block extension](#simple-block-extension)
- [Benchmark Results](#benchmark-results)

---

> Disclaimer: This is just my fun project i do on the side. I do not want to replace any of the major
> markdown solutions for csharp, neither could I do that even if i wanted. It's just for me internally
> to use.

## Why use this library?

- **Performance First**: Designed for scenarios where every microsecond and every byte counts.
- **Low Allocation**: Minimizes pressure on the Garbage Collector by using stack-allocated buffers and pooling where possible.
- **Modern C#**: Built for modern .NET, taking advantage of the latest language and runtime optimizations.
- **Simplicity**: A clean, easy-to-use API that gets the job done without unnecessary complexity.

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
- [ ] Footnotes
- [ ] Full CommonMark Compliance suite validation
- [ ] Context based rendering functions
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

If your sanitizer supports spans, you can use the following to prevent double allocation like above:
```csharp
var options = RenderOptions.HtmlDefault;
options.SanitizerFunc = (span) => HtmlSanitizer.Sanitize(span);

var safeHtml = BeMarkdown.ToHtml(userContent, renderOptions: options);
```

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

      public void Render(
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

      public void Render(
         ReadOnlySpan<char> rawText, 
         ref TextWriterIndentSlim writer, 
         in MarkdownNode current, 
         ReadOnlySpan<MarkdownNode> nodes,
         RenderOptions options)
      {
         writer.Write("<div class=\"red-block\">");
         current.RenderChildren(rawText, nodes, ref writer, options);
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

