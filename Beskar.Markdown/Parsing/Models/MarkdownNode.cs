using System.Runtime.InteropServices;

namespace Beskar.Markdown.Parsing.Models;

[StructLayout(LayoutKind.Explicit, Size = 32)]
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
   public int NextSiblingIndex;
   
   // --- Union Metadata
   [FieldOffset(20)] // NodeType.Header (1 - 6)
   public int HeadingLevel;
   [FieldOffset(20)] // NodeType.List ('-', '*', '1')
   public char ListMarker;
   [FieldOffset(20)] // NodeType.CodeBlock (start of lang string)
   public int CodeLangSpanStart;
   [FieldOffset(20)] // NodeType.ListItem
   public int ListIndent;
   [FieldOffset(20)] // NodeType.Link
   public int LinkUrlStart;
   
   // --- Second offset Metadata
   [FieldOffset(24)] // NodeType.List 
   public byte IsListOrdered;
   [FieldOffset(24)] // NodeType.CodeBlock (length of lang string)
   public int CodeLangSpanLength;
   [FieldOffset(24)] // NodeType.Link
   public int LinkUrlLength;
   
   // --- Third offset Metadata
   [FieldOffset(28)] // NodeType.CodeBlock
   public char CodeBlockMarker;
   
   // --- Fourth offset Metadata
   [FieldOffset(30)]
   public ushort CodeBlockFenceCount; // NodeType.CodeBlock
}