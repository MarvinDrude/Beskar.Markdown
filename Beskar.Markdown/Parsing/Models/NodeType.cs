namespace Beskar.Markdown.Parsing.Models;

public enum NodeType
{
   // Meta
   Document,
   
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