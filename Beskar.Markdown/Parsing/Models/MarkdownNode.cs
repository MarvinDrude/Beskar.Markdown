using System.Runtime.InteropServices;

namespace Beskar.Markdown.Parsing.Models;

[StructLayout(LayoutKind.Explicit, Size = 36)]
public struct MarkdownNode
{
   // --- General fields
   [FieldOffset(0)]
   public NodeType Type;
   [FieldOffset(4)]
   public TextSpan TextSpan;

   // --- Tree Metadata
   [FieldOffset(12)]
   public int FirstChildIndex;
   [FieldOffset(16)]
   public int LastChildIndex;
   [FieldOffset(20)]
   public int NextSiblingIndex;
   
   // --- Union Metadata
   [FieldOffset(24)] // NodeType.Header (1 - 6)
   public int HeadingLevel;
   [FieldOffset(24)] // NodeType.List ('-', '*', '1')
   public char ListMarker;
   [FieldOffset(26)] // NodeType.List
   public byte IsLoose;
   [FieldOffset(24)] // NodeType.CodeBlock (start of lang string)
   public int CodeLangSpanStart;
   [FieldOffset(24)] // NodeType.ListItem
   public int ListIndent;
   [FieldOffset(24)] // NodeType.Link / NodeType.Image
   public int LinkUrlStart;
   [FieldOffset(24)] // NodeType.AutoLink
   public byte IsEmail;
   [FieldOffset(24)] // NodeType.Paragraph
   public byte IsInsideListItem;
   [FieldOffset(25)] // NodeType.Paragraph
   public byte ParagraphIsWrapped;
   [FieldOffset(24)] // NodeType.HtmlBlock
   public byte HtmlBlockType;
   [FieldOffset(25)] // NodeType.HtmlBlock
   public byte HtmlBlockIsClosed;
   
   // --- Second offset Metadata
   [FieldOffset(28)] // NodeType.List 
   public int ListStartNumber;
   [FieldOffset(28)] // NodeType.CodeBlock (length of lang string)
   public int CodeLangSpanLength;
   [FieldOffset(28)] // NodeType.Link  / NodeType.Image
   public int LinkUrlLength;
   
   // --- Third offset Metadata
   [FieldOffset(32)] // NodeType.CodeBlock
   public char CodeBlockMarker;
   [FieldOffset(32)] // NodeType.Image
   public short LinkTitleOffset;
   
   // --- Fourth offset Metadata
   [FieldOffset(34)] // NodeType.CodeBlock
   public ushort CodeBlockFenceCount;
   [FieldOffset(34)] // NodeType.Image
   public ushort LinkTitleLength;
}