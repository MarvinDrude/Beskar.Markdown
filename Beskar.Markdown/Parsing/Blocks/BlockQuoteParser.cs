using Beskar.Markdown.Parsing.Interfaces;
using Beskar.Markdown.Parsing.Models;
using Beskar.Memory.Buffers;
using Beskar.Memory.Writers;

namespace Beskar.Markdown.Parsing.Blocks;

public sealed class BlockQuoteParser : IBlockParser
{
   public int Priority => 9_100;
   public int SupportedTypeValue => (int)NodeType.BlockQuote;

   private const char _quoteChar = '>';
   
   public int TryMatch<TData>(ref LineState<TData> state, int parentIndex, ref BufferWriter<MarkdownNode> writer)
   {
      if (state.FirstChar != _quoteChar)
      {
         return -1;
      }

      var nodeIndex = writer.WrittenSpan.Length;
      var increment = state.FirstNonSpaceIndex + 1;
      state.Slice(increment);
      
      writer.Add(new MarkdownNode()
      {
         Type = NodeType.BlockQuote,
         FirstChildIndex = -1,
         NextSiblingIndex = -1
      });

      if (!state.IsBlank && state.RawLine[0] is ' ' or '\t')
      {
         state.SliceIndentation(1);
      }
      
      return nodeIndex;
   }

   public bool CanContinue<TData>(ref MarkdownNode node, ref LineState<TData> state, 
      ref BufferWriter<MarkdownNode> writer)
   {
      if (state.FirstChar == '>')
      {
         state.Slice(state.FirstNonSpaceIndex + 1);
         
         if (!state.IsBlank && state.RawLine[0] is ' ' or '\t')
         {
            state.SliceIndentation(1);
         }
         
         return true;
      }

      return false;
   }
}
