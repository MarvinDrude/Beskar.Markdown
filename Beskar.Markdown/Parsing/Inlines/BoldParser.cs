using Beskar.Markdown.Parsing.Interfaces;
using Beskar.Markdown.Parsing.Models;
using Me.Memory.Buffers;

namespace Beskar.Markdown.Parsing.Inlines;

public sealed class BoldParser : IInlineParser
{
   public int Priority => 10_000;
   public int SupportedTypeValue => (int)NodeType.StrongEmphasis;
   
   public char TriggerChar => '*';
   
   public bool TryMatch(ref InlineState state, int parentIndex, 
      ref BufferWriter<MarkdownNode> writer, ref InlineParser parser)
   {
      var text = state.RemainingText;
      var markerLength = 0;
      
      if (text.Length > 0 && text[0] == '*') markerLength = 1;
      if (text.Length > 1 && text[1] == '*') markerLength = 2;
      
      if (markerLength == 0 || text.Length < (markerLength * 2 + 1))
      {
         return false;
      }
      
      var closeIdx = -1;
      for (var i = markerLength; i <= text.Length - markerLength; i++)
      {
         if (markerLength == 2 && text[i] == '*' && text[i + 1] == '*')
         {
            closeIdx = i;
            break;
         }
         
         if (markerLength == 1 && text[i] == '*')
         {
            if (i + 1 < text.Length && text[i + 1] == '*') 
            {
               continue;
            }
            
            closeIdx = i;
            break;
         }
      }

      if (closeIdx == -1)
      {
         return false; // No closing tag found
      }
      
      var contentStart = state.GlobalOffset + markerLength;
      var contentLength = closeIdx - markerLength;

      var nodeIndex = writer.WrittenSpan.Length;
      writer.Add(new MarkdownNode()
      {
         Type = markerLength == 1 ? NodeType.Emphasis : NodeType.StrongEmphasis, 
         TextSpan = new TextSpan(contentStart, contentLength),
         FirstChildIndex = -1,
         NextSiblingIndex = -1
      });

      parser.LinkInlineNode(ref writer, parentIndex, nodeIndex);
      state.Advance(closeIdx + markerLength);
      
      return true;
   }
}