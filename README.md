# Beskar.Markdown

Beskar.Markdown is a high-performance, low-allocation Markdown parser and HTML renderer 
for .NET. It is built from the ground up to leverage modern C# features like `Span<T>`, 
`ReadOnlySequence<T>`, and efficient memory management to provide a blazing-fast experience.

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

### Currently Supported
- **Blocks**:
    - Headers (ATX & Setext)
    - Paragraphs
    - Blockquotes
    - Lists (Ordered & Unordered)
    - Fenced Code Blocks
    - Indented Code Blocks
    - Thematic Breaks (Horizontal Rules)
    - HTML Blocks
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
- [ ] Tables
- [ ] Task Lists
- [ ] Footnotes
- [ ] Full CommonMark Compliance suite validation

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

