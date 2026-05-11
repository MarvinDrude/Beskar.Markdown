using Beskar.Markdown.Parsing.Interfaces;
using Beskar.Markdown.Parsing.Models;
using Beskar.Markdown.Parsing.Utils;
using Me.Memory.Buffers;

namespace Beskar.Markdown.Parsing.Blocks;

public sealed class ListParser : IBlockParser
{
   public int Priority => 10_080;
   public int SupportedTypeValue => (int)NodeType.List;
   
   public int TryMatch<TData>(ref LineState<TData> state, int parentIndex, ref BufferWriter<MarkdownNode> writer)
   {
      if (state.LeadingSpaces >= 4) return -1;

      if (!ListUtils.IsListMarker(ref state, out var listChar, out var len, out var orderedNumber))
      {
         return -1;
      }

      if (parentIndex != -1)
      {
         var parent = writer.WrittenSpan[parentIndex];
         if (parent.Type is NodeType.List && parent.ListMarker == listChar)
         {
            return -1;
         }
      }
      
      var nodeIndex = writer.WrittenSpan.Length;
      writer.Add(new MarkdownNode()
      {
         Type = NodeType.List,
         ListMarker = listChar,
         ListStartNumber = orderedNumber,
         FirstChildIndex = -1,
         NextSiblingIndex = -1,
         LastChildIndex = -1
      });

      return nodeIndex;
   }

   public bool CanContinue<TData>(ref MarkdownNode node, ref LineState<TData> state, 
      ref BufferWriter<MarkdownNode> writer)
   {
      if (state.IsBlank || state.LeadingSpaces > 0)
      {
         return true;
      }

      if (ListUtils.IsListMarker(ref state, out var listChar, out _, out _))
      {
         return node.ListMarker == listChar;
      }

      return false;
   }
}