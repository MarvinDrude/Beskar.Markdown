using Beskar.Markdown.Parsing.Interfaces;
using Beskar.Markdown.Parsing.Models;
using Me.Memory.Buffers;

namespace Beskar.Markdown.Parsing.Blocks;

public sealed class ParagraphParser : IBlockParser
{
   public int Priority => 10;
   public int SupportedTypeValue => (int)NodeType.Paragraph;
   
   public int TryMatch(ref LineState state, int parentIndex, ref BufferWriter<MarkdownNode> writer)
   {
      if (state.IsBlank)
      {
         return -1;
      }
      
      var nodeIndex = writer.WrittenSpan.Length;
      writer.Add(new MarkdownNode()
      {
         Type = NodeType.Paragraph,
         TextSpan = new TextSpan(state.GlobalOffset + state.FirstNonSpaceIndex, state.RawLine.Length),
         FirstChildIndex = -1,
         NextSiblingIndex = -1
      });

      state.Slice(state.RawLine.Length);
      return nodeIndex;
   }

   public bool CanContinue(ref MarkdownNode node, ref LineState state)
   {
      if (state.IsBlank)
      {
         return false;
      }

      var currentLength = (state.GlobalOffset + state.RawLine.Length) - node.TextSpan.Start;
      node.TextSpan = node.TextSpan with { Length = currentLength };
      
      state.Slice(state.RawLine.Length);
      return true;
   }
}