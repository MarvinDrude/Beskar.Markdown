using Beskar.Markdown.Parsing;
using Beskar.Markdown.Parsing.Models;
using Me.Memory.Buffers;

Console.WriteLine("Hello, World!");

const string example = 
   """
   > Hallo
   > Hallo2
   
   ####### Heading 6
   ###### Heading 5
   ##### Heading 4
   ### Heading 3
   ## Heading 2
   # Heading 1
   """;

var options = ParserOptions.Default;
var parser = new MarkdownParser(example, stackalloc MarkdownNode[100]);
parser.Parse(options);


_ = "";