using Beskar.Markdown.Parsing.Interfaces;
using Beskar.Markdown.Parsing.Models;
using Me.Memory.Buffers;

namespace Beskar.Markdown.Parsing.Blocks;

public sealed class HtmlParser : IBlockParser
{
   public int Priority => 4_000;
   public int SupportedTypeValue => (int)NodeType.HtmlBlock;
   
   public int TryMatch(ref LineState state, int parentIndex, ref BufferWriter<MarkdownNode> writer)
   {
      if (state.IsBlank || state.LeadingSpaces >= 4)
      {
         return -1;
      }
      
      var line = state.RawLine;
      var startIndex = state.FirstNonSpaceIndex;

      // Must start with an open angle bracket
      if (line.Length <= startIndex + 1 || line[startIndex] != '<')
      {
         return -1;
      }
      
      var nextChar = line[startIndex + 1];
      var isTag = char.IsLetter(nextChar) || // <div>
         nextChar == '/' || // </div>
         nextChar == '!'; // <!-- or <!DOCTYPE

      if (!isTag)
      {
         return -1;
      }

      var nodeIndex = writer.WrittenSpan.Length;
      writer.Add(new MarkdownNode()
      {
         Type = NodeType.HtmlBlock,
         TextSpan = new TextSpan(state.GlobalOffset, state.RawLine.Length),
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
      
      var newLength = (state.GlobalOffset - node.TextSpan.Start) + state.RawLine.Length;
      
      node.TextSpan = node.TextSpan with { Length = newLength };
      state.Slice(state.RawLine.Length);

      return true;
   }
}