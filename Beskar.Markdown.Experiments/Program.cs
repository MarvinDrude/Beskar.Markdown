using Beskar.Markdown.Extensions;
using Beskar.Markdown.Parsing;
using Beskar.Markdown.Parsing.Models;

const string example = 
   """
   Hier ist ein Text.
   Wie sieht er aus?
   
   > Hallo
   > Hallo2
   
   Hier ist ein Text. 2
   Wie sieht er aus? 2
   
   ###### Heading 6
   ##### Heading 5
   #### Heading 4
   ### Heading 3
   ## Heading 2
   # Heading 1
   """;

var options = ParserOptions.Default;
var parser = new MarkdownParser(example, stackalloc MarkdownNode[100]);
parser.Parse(options);

var debugString = parser.WrittenNodes.ToArray().CreateDebugString(example);
Console.WriteLine(debugString);

_ = "";