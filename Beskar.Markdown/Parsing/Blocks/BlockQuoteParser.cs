using Beskar.Markdown.Parsing.Interfaces;
using Beskar.Markdown.Parsing.Models;
using Me.Memory.Buffers;

namespace Beskar.Markdown.Parsing.Blocks;

public sealed class BlockQuoteParser : IBlockParser
{
   public int Priority => 9_100;
   public int SupportedTypeValue => (int)NodeType.BlockQuote;

   private const char _quoteChar = '>';
   
   public int TryMatch(ref LineState state, int parentIndex, ref BufferWriter<MarkdownNode> writer)
   {
      if (state.FirstChar != _quoteChar)
      {
         return -1;
      }

      var nodeIndex = writer.WrittenSpan.Length;
      writer.Add(new MarkdownNode()
      {
         Type = NodeType.BlockQuote,
         TextSpan = new TextSpan(state.GlobalOffset + state.FirstNonSpaceIndex, state.RawLine.Length),
         FirstChildIndex = -1,
         NextSiblingIndex = -1
      });
      
      state.Slice(state.FirstNonSpaceIndex + 1);
      if (!state.IsBlank && state.RawLine[0] == ' ')
      {
         state.Slice(1);
      }

      return nodeIndex;
   }

   public bool CanContinue(ref MarkdownNode node, ref LineState state)
   {
      if (state.FirstChar == '>')
      {
         state.Slice(state.FirstNonSpaceIndex + 1);
         
         if (!state.IsBlank && state.RawLine[0] == ' ')
         {
            state.Slice(1);
         }
         
         return true;
      }

      return false;
   }
}