using Beskar.Markdown.Parsing.Interfaces;
using Beskar.Markdown.Parsing.Models;
using Beskar.Markdown.Parsing.Utils;
using Beskar.Memory.Buffers;
using Beskar.Memory.Writers;

namespace Beskar.Markdown.Parsing.Blocks;

public sealed class ListItemParser : IBlockParser
{
   public int Priority => 10_070;
   public int SupportedTypeValue => (int)NodeType.ListItem;
   
   public int TryMatch<TData>(ref LineState<TData> state, int parentIndex, ref BufferWriter<MarkdownNode> writer)
   {
      if (parentIndex == -1) return -1;
      if (state.LeadingSpaces >= 4) return -1;
      
      var parent = writer.WrittenSpan[parentIndex];
      if (parent.Type is not NodeType.List) return -1;

      if (!ListUtils.IsListMarker(ref state, out var listChar, out var markerLength, out _)
          || parent.ListMarker != listChar)
      {
         return -1;
      }

      var nodeIndex = writer.WrittenSpan.Length;
      var markerStartOffset = state.GlobalOffset + state.FirstNonSpaceIndex;
      var visibleMarkerLength = 0;
      var baseColumn = state.Column;

      for (var i = state.FirstNonSpaceIndex; i < state.RawLine.Length; i++)
      {
         if (state.RawLine[i] == ' ' || state.RawLine[i] == '\t') break;
         visibleMarkerLength++;
      }

      state.Slice(markerLength);

      var paddingAfterMarker = state.LeadingSpaces;
      if (state.IsBlank || paddingAfterMarker > 4)
      {
         paddingAfterMarker = 1;
      }

      var contentIndent = state.Column - baseColumn + paddingAfterMarker;
      state.SliceIndentation(paddingAfterMarker);

      writer.Add(new MarkdownNode()
      {
         Type = NodeType.ListItem,
         TextSpan = new TextSpan(markerStartOffset, visibleMarkerLength),
         ListIndent = contentIndent,
         FirstChildIndex = -1,
         NextSiblingIndex = -1,
         LastChildIndex = -1,
      });

      return nodeIndex;
   }

   public bool CanContinue<TData>(ref MarkdownNode node, ref LineState<TData> state, 
      ref BufferWriter<MarkdownNode> writer)
   {
      if (state.IsBlank) 
         return node.FirstChildIndex != -1;

      if (state.LeadingSpaces < node.ListIndent) 
         return false;
      
      state.SliceIndentation(node.ListIndent);
      return true;

   }
}
