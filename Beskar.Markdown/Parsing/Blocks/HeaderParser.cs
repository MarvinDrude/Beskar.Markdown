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
      var offset = (hasSpaceAfter ? 1 : 0);

      var contentStart = state.FirstNonSpaceIndex + level + offset;
      var contentEnd = state.RawLine.Length;

      while (contentEnd > contentStart && (state.RawLine[contentEnd - 1] == ' ' || state.RawLine[contentEnd - 1] == '\t'))
         contentEnd--;

      if (contentEnd > contentStart && state.RawLine[contentEnd - 1] == '#')
      {
         var hashEnd = contentEnd;
         while (contentEnd > contentStart && state.RawLine[contentEnd - 1] == '#')
            contentEnd--;

         if (contentEnd == contentStart || state.RawLine[contentEnd - 1] == ' ' || state.RawLine[contentEnd - 1] == '\t')
         {
            while (contentEnd > contentStart && (state.RawLine[contentEnd - 1] == ' ' || state.RawLine[contentEnd - 1] == '\t'))
               contentEnd--;
         }
         else
         {
            contentEnd = hashEnd;
         }
      }

      writer.Add(new MarkdownNode()
      {
         Type = NodeType.Header,
         TextSpan = new TextSpan(state.GlobalOffset + contentStart, contentEnd - contentStart),
         FirstChildIndex = -1,
         NextSiblingIndex = -1,
         HeadingLevel = level
      });

      state.Slice(state.RawLine.Length);
      
      return nodeIndex;
   }

   public bool CanContinue(ref MarkdownNode node, ref LineState state)
   {
      return false;
   }
}