using Beskar.Markdown.Extensions;
using Beskar.Markdown.Parsing;
using Beskar.Markdown.Parsing.Models;
using Beskar.Markdown.Rendering;

const string example = 
   """
   aa
   dsads a <https://foo.bar.baz/test?q=hello&id=22&boolean>
   dsadsa<http://foo.bar.baz>
   ads       <MAILTO:FOO@BAR.BAZ>
   
   <https://foo.bar.baz/test?q=hello&id=22&boolean>
   <MAILTO:FOO@BAR.BAZ>
   
   Test 4 <b>***hallo***</b> s \*bold\*
   
   *(*foo*)*
   
   foo *\**
   
   __foo__ aa_adsa *aa
   
   - - - - -
   
       Code
       Block
       Spaces
       
       Hallo
   dsad
   
         Hallo
         a
         d
   
   > foo
   bar
   ===
   
   Hardbreak spaces test  
   Befpre those
   
   ## Heading Trail ##
   ## Heading Trail 2 ###
   ## Heading Trail 3 \##
   
   setex
   ===
   
   setex 2
   ---
   
   ## Heading 2
   Test
   
   - Item 1
   - Item 2
   - Item 3 **bold** test
      * Item 3.1
      * Item 3.2
   
   Hier ist ein Text.
   Wie sieht er aus? *bold* aa
   
   > Hallo
   > > Inner Hallo
   > > Inner Hallo2
   > Hallo2
   
   -----
   
   Hier ist ein Text. 2
   Wie sieht er aus? 2 `code inline` s
   
   `css`
   
   <div>
      <span>Test</span>
      <!-- Test -->
   </div>
   
   _____
   
   ```csharp
   >
   class Program 
   {
      private string Test = "> **";
   }
   ```
   
   Hier title [Link text](https://google.com)
   Image: ![Alt text](https://google.com)
   
   ###### Heading 6
   ##### Heading 5
   #### Heading 4 *bold extra* a
   ### Heading 3
   ## Heading 2**dsad**
   # Heading 1** * double * **
   
   Hardbeak \
   Test \
   
   1. First
      - aa
         - bb
         - cc
      - aaa
   2. Second
      * adsa
   22. Third
   
   [link](\(foo\))
   [link](foo(and(bar)))
   [link](foo(and(bar))
   [link](#fragment)
   [link]("title")
   
   ~~strikethrough~~
   
   | foo | bar |
   | --- | --- |
   | `baz` | bim |
   
   | abc | defghi |
   :-: | -----------:
   bar | baz
   """;

var options = ParserOptions.Default;
var parser = new MarkdownParser<object>(example, stackalloc MarkdownNode[100]);
var context = parser.Parse(options);

var renderer = new MarkdownRenderer(example);
var html = renderer.Render(context, parser.WrittenNodes, RenderOptions.HtmlDefault);

var debugString = parser.WrittenNodes.ToArray().CreateDebugString(example);
Console.WriteLine(debugString);

Console.WriteLine(html);

while (true)
{
   await Task.Delay(1000);
}

_ = "";