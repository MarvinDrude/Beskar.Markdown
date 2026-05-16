# Markdown Kitchen Sink Benchmark File

This document contains a comprehensive collection of Markdown features designed to test parsers, renderers, and syntax highlighters.

## Headers

# Header 1
## Header 2
### Header 3
#### Header 4
##### Header 5
###### Header 6

---

## Typography & Emphasis

This text is **bold** and this is *italic*.
You can also use __bold__ and _italic_ with underscores.
Combine them for ***bold and italic***.
Here is some ~~strikethrough~~ text.

You can use inline `code` to denote variables, functions, or commands.

---

## Blockquotes

> This is a standard blockquote.
> It can span multiple lines and contain other formatting, like **bold** text.
>> You can even nest blockquotes to indicate replies.
>> > Deeply nested quote!

---

## Lists

### Unordered Lists
* Item 1
* Item 2
    * Nested item 2.1
    * Nested item 2.2
        * Deeply nested item
* Item 3

### Ordered Lists
1. First step
2. Second step
    1. Sub-step A
    2. Sub-step B
3. Third step

### Task Lists
- [x] Completed task
- [ ] Pending task
- [ ] Another pending task

---

## Code Blocks

Below is a block of C# code to test syntax highlighting.

```csharp
using System;

namespace BenchmarkSuite
{
   public class ParserTest
   {
      public void Run()
      {
         // Initialize the test
         Console.WriteLine("Running Markdown parser...");
         
         int testsPassed = 0;
         if (testsPassed == 0)
         {
            Console.WriteLine("Awaiting results.");
         }
      }
   }
}
```

---

## Tables

Tables are excellent for testing alignment and column rendering.

| Feature | Support | Notes |
| :--- | :---: | ---: |
| Left Aligned | Center Aligned | Right Aligned |
| Tables | Yes | Essential for data presentation |
| Footnotes | Partial | Depends on the specific Markdown flavor |
| Custom HTML | Yes | Subject to security filtering |

---

## Links and Images

* [Inline Link](https://example.com)
* [Link with Title](https://example.com "Example Website")
* [Reference Link][1]

![Placeholder Image](https://picsum.photos/400/200 "Example Benchmark Image")

[1]: https://example.com "Reference Target"

---

## HTML Integration

If the renderer supports raw HTML, the following element should render cleanly without breaking the surrounding layout:

<div style="padding: 15px; border: 1px solid #ccc; background-color: #f9f9f9;">
   <strong>HTML Block:</strong> This is a standard <code>&lt;div&gt;</code> element demonstrating HTML pass-through.
</div>