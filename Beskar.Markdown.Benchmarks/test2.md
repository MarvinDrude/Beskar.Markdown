# Extended Markdown Benchmark Fixture

This second fixture is intentionally noisier than `test.md`. It mixes common authoring patterns with ambiguous, malformed, nested, and parser-stress cases.

## Heading Coverage

# ATX Level 1
## ATX Level 2 with trailing hashes ##
### ATX Level 3 with escaped trailing \###
#### ATX Level 4 with **strong text** and `inline code`
##### ATX Level 5 immediately followed by punctuation!
###### ATX Level 6
####### Seven hashes should be plain text

Setext Level 1 With Inline *Emphasis*
======================================

Setext Level 2 With Link [reference][ref-main]
----------------------------------------------

Setext candidate with too many characters
--- extra text

## Paragraphs, Breaks, And Text Runs

This paragraph has ordinary text, repeated punctuation!!!!, encoded-looking entities like &amp; and &lt;tag&gt;, bare punctuation <>[](){} and a long run of characters: aaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaaa.

Line one ends with a soft break
and line two continues the same paragraph with `code`, *emphasis*, **strong**, ***combined***, ~~strike~~, <span data-inline="yes">inline html</span>, and <https://example.com/autolink?x=1&y=two>.

This line has two spaces for a hard break.  
The next line should be separate in renderers that preserve hard breaks.

This line uses a backslash hard break.\
The next line follows after the slash break.

Escaped characters: \*not emphasis\*, \_not emphasis\_, \[not link\](target), \`not code\`, \# not a heading, and \\ a literal backslash.

## Inline Emphasis Edge Cases

*simple emphasis* and _underscore emphasis_.
**simple strong** and __underscore strong__.
***triple emphasis*** and ___triple underscore emphasis___.
**strong with *inner emphasis* text** and *emphasis with **inner strong** text*.
***strong-emphasis with `inline code` inside***.
This has intraword underscores: alpha_beta_gamma and many_words_inside_text.
This has dangling delimiters: *open emphasis, **open strong, ~~open strike.
This has repeated delimiters: ****four stars**** and *****five stars*****.
This has mixed delimiters: **bold _with underscore emphasis_ done**.
Strikethrough combinations: ~~deleted **strong deleted** and *emphasis deleted*~~.
Code beats emphasis here: `**not strong**`, ``code with ` tick``, and ``` code with double `` ticks ```.

## Links, Images, And Autolinks

[Inline link](https://example.com/path?q=one&two=2)
[Inline link with title](https://example.com "Example Title")
[Nested text [brackets] in link](https://example.com/brackets)
[Escaped parens](https://example.com/a\(b\)c)
[Balanced parens](https://example.com/a(b(c)d)e)
[Empty destination]()
[Fragment only](#local-fragment)
[Reference link][ref-main]
[Collapsed reference][collapsed]
[Shortcut reference]
[Missing reference][does-not-exist]
[Link with quoted title](https://example.com/title 'Single Quote Title')
[Link with parenthesized title](https://example.com/title (Paren Title))
Bare URL text should remain text: https://example.com/not-autolink
Autolinks should parse: <https://example.org/auto>, <http://example.org/a?b=c&d=e>, <mailto:test@example.org>, and <MAILTO:UPPER@example.org>.

![Image alt text](https://example.com/image.png)
![Image with title](https://example.com/image.png "Image Title")
![Image with *formatting* in alt](https://example.com/format.png)
![Empty destination image]()
![Reference image][img-main]

[ref-main]: https://example.com/reference "Reference Title"
[collapsed]: https://example.com/collapsed
[Shortcut reference]: https://example.com/shortcut
[img-main]: https://example.com/reference-image.png "Reference Image"
[spaced-ref]:    https://example.com/spaced    "Spaced Reference"

## Lists

- Bullet item one
- Bullet item two with continuation
  that should remain attached to the item.
- Bullet item three with children
  - Nested bullet A
    - Nested bullet A.1
      - Nested bullet A.1.a
  - Nested bullet B with **strong**
- Bullet item after nested content

* Asterisk bullet one
* Asterisk bullet two
  1. Ordered child one
  2. Ordered child two
     - Mixed unordered child
     - Mixed child with `code`

+ Plus bullet one
+ Plus bullet two with lazy continuation
lazy continuation text for plus item two

1. Ordered item one
2. Ordered item two
3. Ordered item three
   1. Nested ordered one
   2. Nested ordered two
      1. Very nested ordered item
4. Ordered item four

99. Ordered list starting at ninety-nine
100. Next item
101. Next item with paragraph

    Indented continuation paragraph inside ordered item.

- [x] Completed task
- [X] Completed task uppercase marker
- [ ] Open task
- [ ] Task with [link](https://example.com/task) and ![image](https://example.com/task.png)
- [x] Task with nested list
  - Nested item under a task
  - Nested item with ~~strike~~

- Tight list item
- Tight list item with inline html <kbd>Ctrl</kbd>
- Tight list item ending

- Loose list item paragraph one

  Loose list item paragraph two

- Second loose item

## Block Quotes

> Basic quote with *emphasis* and **strong**.
> Still the same quote with a soft break.
>
> Blank quoted line separates quote paragraphs.
> - Quoted bullet
> - Quoted bullet with `code`
>
> ```text
> quoted fenced code
> > this marker is code, not a nested quote
> ```

> Nested quote starts here.
>> Nested level two.
>>> Nested level three with <https://example.com/deep>.
>> Back to level two.
> Back to level one.

> Lazy quote line one
lazy continuation outside the marker depending on parser behavior

## Code Blocks

Indented code block:

    four spaces
        eight spaces
    <div>not html here</div>
    **not strong here**

Fenced code block with language:

```csharp
using System;

public static class Sample
{
   public static string Render(string markdown)
   {
      return BeMarkdown.ToHtml(markdown);
   }
}
```

Fenced code block with backticks inside:

````markdown
```text
nested fence content
```
````

Tilde fenced code block:

~~~json
{
   "name": "fixture",
   "cases": ["table", "link", "html", "quote"],
   "enabled": true
}
~~~

Unclosed fence starts here:

```text
This fence is intentionally left open until the next thematic break-like text.
It contains ### heading-looking text and - list-looking text.
```

## Thematic Breaks

---
***
___
- - -
* * *
_ _ _
----    
***    

Not thematic: ---, after text
Not thematic: - - text -

## Tables

| Left | Center | Right | Mixed Inline |
| :--- | :----: | ----: | --- |
| alpha | beta | gamma | **strong** and `code` |
| pipe escaped \| value | centered | 12345 | [link](https://example.com/table) |
| empty next | | | ![img](https://example.com/table.png) |

| Compact | Table |
|---|---|
| one | two |
| three | four |

| Alignment | Weird spacing | Extra |
|:---|---:|:---:|
| left | right | center |
| `code` | *em* | ~~strike~~ |

Paragraph with | pipes | should stay a paragraph.

## HTML Blocks And Inline HTML

<div class="fixture" data-kind="block">
   <p>Raw HTML block with <strong>HTML strong</strong> and markdown-looking **text**.</p>
   <span>Inline child span</span>
</div>

<!-- HTML comment block -->

<script type="text/plain">
var markdown = "**not parsed inside script**";
</script>

<table>
   <tr><th>HTML table</th><th>Value</th></tr>
   <tr><td>A</td><td>1</td></tr>
</table>

Inline HTML examples: <em>html emphasis</em>, <strong>html strong</strong>, <br>, <input type="checkbox" checked>, and <a href="https://example.com/html">html link</a>.

## Mixed Stress Section

### Repeated Mixed Paragraph 1

Text before [a link with **strong text**](https://example.com/mixed/1) followed by an image ![alt *one*](https://example.com/one.png), a task-looking string - [ ] not a task in paragraph, and an autolink <https://example.com/mixed/1>.

> Quote with a list:
> 1. first quoted ordered item
> 2. second quoted ordered item
>    - nested quoted bullet
>    - nested quoted bullet with [ref-main]

### Repeated Mixed Paragraph 2

1. Ordered item with a table after a blank line

   | Inner | Table |
   | --- | --- |
   | inside | list |
   | with | pipes \| escaped |

2. Ordered item after table

### Repeated Mixed Paragraph 3

- Bullet with paragraph

  Paragraph continuation with `inline code`, <span>inline html</span>, and ~~strike~~.

- Bullet with quote

  > Quote inside list item
  > continuing quote

- Bullet with fenced code

  ```text
  fenced code inside list item
  - not a list
  ```

### Ambiguous Link And Code Runs

`code before`[link after code](https://example.com/after-code)
[link before code](https://example.com/before-code)`code after`
[nested `code` text](https://example.com/code-text)
[link with <span>html</span> text](https://example.com/html-text)
![image `alt code`](https://example.com/alt-code.png)

`` inline code with spaces at both ends ``
``
empty-looking multi tick code
``

### Escapes And Entities

Escaped punctuation: \! \" \# \$ \% \& \' \( \) \* \+ \, \- \. \/ \: \; \< \= \> \? \@ \[ \\ \] \^ \_ \` \{ \| \} \~.
Named-looking entities: &copy; &notarealentity; &amp; &lt; &gt;.
Numeric-looking entities: &#35; &#x23; &#00035;.

### Deep Nesting

- level 1
  - level 2
    - level 3
      - level 4
        - level 5
          - level 6 with **strong**
            1. ordered level 7
               > quote at level 8
               >
               > ```text
               > code at level 8
               > ```

### More Reference Definitions

[multi-line-title]: https://example.com/multi
   "Title on next line"

[empty-title]: https://example.com/empty ""
[angle-destination]: <https://example.com/angle destination> "Angle Destination"
[case-ref]: https://example.com/case

[CASE-REF]
[case-ref]
[spaced-ref]
[angle link][angle-destination]
[multi title][multi-line-title]
[empty title][empty-title]

## Final Paragraph

The final paragraph intentionally ends with inline markup and punctuation: **strong**, *em*, `code`, ~~strike~~, [link](https://example.com/end), ![img](https://example.com/end.png), <https://example.com/end>, <span>end</span>.
