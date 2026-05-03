using Beskar.Markdown.Extensions;
using Beskar.Markdown.Parsing;
using Beskar.Markdown.Parsing.Models;

const string example = 
   """
   Test 4
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
   """;

var options = ParserOptions.Default;
var parser = new MarkdownParser(example, stackalloc MarkdownNode[100]);
parser.Parse(options);

var debugString = parser.WrittenNodes.ToArray().CreateDebugString(example);
Console.WriteLine(debugString);

_ = "";