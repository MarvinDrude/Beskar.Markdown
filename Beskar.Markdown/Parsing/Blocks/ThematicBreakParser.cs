using Beskar.Markdown.Parsing.Interfaces;
using Beskar.Markdown.Parsing.Models;
using Beskar.Memory.Buffers;
using Beskar.Memory.Writers;

namespace Beskar.Markdown.Parsing.Blocks;

public sealed class ThematicBreakParser : IBlockParser
{
   public int Priority => 10_085;
   public int SupportedTypeValue => (int)NodeType.ThematicBreak;
   
   public int TryMatch<TData>(ref LineState<TData> state, int parentIndex, ref BufferWriter<MarkdownNode> writer)
   {
      if (state.IsBlank || state.LeadingSpaces >= 4)
      {
         return -1;
      }
      
      var marker = state.FirstChar;
      if (marker != '*' && marker != '-' && marker != '_')
      {
         return -1;
      }
      
      var count = 0;
      var rawLine = state.RawLine;
      
      for (var i = state.FirstNonSpaceIndex; i < rawLine.Length; i++)
      {
         var c = rawLine[i];
         
         if (c == marker)
         {
            count++;
         }
         else if (c != ' ' && c != '\t')
         {
            return -1;
         }
      }

      if (count < 3)
      {
         return -1;
      }

      var nodeIndex = writer.WrittenSpan.Length;
      writer.Add(new MarkdownNode()
      {
         Type = NodeType.ThematicBreak,
         TextSpan = new TextSpan(state.GlobalOffset, state.RawLine.Length),
         FirstChildIndex = -1,
         NextSiblingIndex = -1
      });

      state.ConsumeRest();
      return nodeIndex;
   }

   public bool CanContinue<TData>(ref MarkdownNode node, ref LineState<TData> state, 
      ref BufferWriter<MarkdownNode> writer)
   {
      // only single lines
      return false;
   }
}
