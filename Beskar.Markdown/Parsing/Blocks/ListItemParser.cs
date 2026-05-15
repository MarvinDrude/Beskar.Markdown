using Beskar.Markdown.Parsing.Interfaces;
using Beskar.Markdown.Parsing.Models;
using Beskar.Markdown.Parsing.Utils;
using Me.Memory.Buffers;

namespace Beskar.Markdown.Parsing.Blocks;

public sealed class ListItemParser : IBlockParser
{
   public int Priority => 10_070;
   public int SupportedTypeValue => (int)NodeType.ListItem;
   
   public int TryMatch<TData>(ref LineState<TData> state, int parentIndex, ref BufferWriter<MarkdownNode> writer)
   {
      if (parentIndex == -1) return -1;
      
      var parent = writer.WrittenSpan[parentIndex];
      if (parent.Type is not NodeType.List) return -1;

      if (!ListUtils.IsListMarker(ref state, out _, out var markerLength, out _))
      {
         return -1;
      }

      var nodeIndex = writer.WrittenSpan.Length;
      var markerStartOffset = state.GlobalOffset + state.FirstNonSpaceIndex;
      var visibleMarkerLength = 0;

      for (var i = state.FirstNonSpaceIndex; i < state.RawLine.Length; i++)
      {
         if (state.RawLine[i] == ' ' || state.RawLine[i] == '\t') break;
         visibleMarkerLength++;
      }

      state.Slice(markerLength);

      var spacesAfterMarker = 0;
      while (spacesAfterMarker < state.RawLine.Length && state.RawLine[spacesAfterMarker] == ' ')
      {
         spacesAfterMarker++;
      }

      if (spacesAfterMarker == 0 && state.IsBlank || spacesAfterMarker > 4)
      {
         spacesAfterMarker = 1;
      }

      var contentIndent = markerLength + spacesAfterMarker;
      state.Slice(spacesAfterMarker);

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
         return true;

      if (state.LeadingSpaces < node.ListIndent) 
         return false;
      
      var charsToSlice = 0;
      var currentIndent = 0;
         
      for (var i = 0; i < state.RawLine.Length; i++)
      {
         if (currentIndent >= node.ListIndent) break;
         
         if (state.RawLine[i] == ' ')
         {
            currentIndent++;
            charsToSlice++;
         }
         else if (state.RawLine[i] == '\t')
         {
            currentIndent += 4;
            charsToSlice++;
         }
         else
         {
            break;
         }
      }
         
      state.Slice(charsToSlice);
      return true;

   }
}