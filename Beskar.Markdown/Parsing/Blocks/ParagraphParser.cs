using Beskar.Markdown.Extensions;
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
      
      var paraIndex = writer.WrittenSpan.Length;
      writer.Add(new MarkdownNode()
      {
         Type = NodeType.Paragraph,
         FirstChildIndex = -1,
         NextSiblingIndex = -1
      });

      var textIndex = writer.WrittenSpan.Length;
      writer.Add(new MarkdownNode()
      {
         Type = NodeType.Text,
         TextSpan = new TextSpan(state.GlobalOffset + state.FirstNonSpaceIndex, state.RawLine.Length),
         FirstChildIndex = -1,
         NextSiblingIndex = -1
      });

      writer.GetReference(paraIndex).FirstChildIndex = textIndex;

      state.Slice(state.RawLine.Length);
      return paraIndex;
   }

   public bool CanContinue(ref MarkdownNode node, ref LineState state, ref BufferWriter<MarkdownNode> writer)
   {
      return false;
   }
}