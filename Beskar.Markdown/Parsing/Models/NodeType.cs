namespace Beskar.Markdown.Parsing.Models;

public enum NodeType
{
   // Meta
   Document,
   IndentedCodeFragment,
   
   // Blocks
   BlockQuote,
   CodeBlock,
   IndentedCodeBlock,
   Header,
   Paragraph,
   List,
   ListItem,
   ThematicBreak,
   HtmlBlock,
   LinkReferenceDefinition,
   Table,
   TableHeader,
   TableBody,
   TableRow,
   TableCell,
   
   // Inline
   Text,
   Emphasis,
   StrongEmphasis,
   InlineCode,
   Link,
   Image,
   InlineHtml,
   SoftBreak,
   LineBreak,
   StrikeThrough,
   Autolink
   
   // Custom User Space
}