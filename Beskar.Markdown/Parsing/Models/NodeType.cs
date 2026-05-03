namespace Beskar.Markdown.Parsing.Models;

public enum NodeType
{
   // Meta
   Document,
   
   // Blocks
   BlockQuote,
   CodeBlock,
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
   
   // Custom User Space
}