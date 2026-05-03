using Beskar.Markdown.Parsing.Interfaces;
using Beskar.Markdown.Parsing.Models;
using Me.Memory.Buffers;

namespace Beskar.Markdown.Parsing.Blocks;

public sealed class HeaderParser : IBlockParser
{
   public int Priority => 10_090;
   public int SupportedTypeValue => (int)NodeType.Header;

   private const char _headingChar = '#';
   
   public int TryMatch(ref LineState state, int parentIndex, ref BufferWriter<MarkdownNode> writer)
   {
      if (state.FirstChar != _headingChar || state.LeadingSpaces >= 4)
      {
         return -1;
      }

      var line = state.RawLine[state.FirstNonSpaceIndex..];
      var level = 0;
      
      while (level < line.Length && line[level] == _headingChar)
      {
         level++;
      }

      if (level > 6) return -1;
      
      var hasSpaceAfter = level < line.Length && line[level] == ' ';
      var isEndOfLine = level == line.Length;
      
      if (!hasSpaceAfter && !isEndOfLine)
      {
         return -1;
      }

      var nodeIndex = writer.WrittenSpan.Length;
      writer.Add(new MarkdownNode()
      {
         Type = NodeType.Header,
         TextSpan = new TextSpan(state.GlobalOffset + level, state.RawLine.Length),
         FirstChildIndex = -1,
         NextSiblingIndex = -1,
         HeadingLevel = level
      });

      var sliceAmount = level + (hasSpaceAfter ? 1 : 0);
      state.Slice(sliceAmount);
      
      return nodeIndex;
   }

   public bool CanContinue(ref MarkdownNode node, ref LineState state)
   {
      return false;
   }
}