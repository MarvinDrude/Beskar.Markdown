using Beskar.Markdown.Parsing.Interfaces;
using Beskar.Markdown.Parsing.Models;
using Me.Memory.Buffers;

namespace Beskar.Markdown.Parsing.Inlines;

public sealed class InlineCodeParser : IInlineParser
{
   public int Priority => 20_000;
   public int SupportedTypeValue => (int)NodeType.InlineCode;
   
   public char TriggerChar => '`';
   public char TriggerAltChar => '`';
   
   public bool TryMatch(ref InlineState state, int parentIndex, ref BufferWriter<MarkdownNode> writer, scoped ref InlineParser parser)
   {
      var text = state.RemainingText;

      var markerLength = 0;
      while (markerLength < text.Length && text[markerLength] == '`')
      {
         markerLength++;
      }

      if (markerLength == 0)
      {
         return false;
      }
      
      var closeIndex = -1;
      var scanIndex = markerLength;

      while (scanIndex < text.Length)
      {
         if (text[scanIndex] == '`')
         {
            var streak = 0;
            while (scanIndex + streak < text.Length && text[scanIndex + streak] == '`')
            {
               streak++;
            }

            if (streak == markerLength)
            {
               closeIndex = scanIndex;
               break;
            }
            
            scanIndex += streak;
         }
         else
         {
            scanIndex++;
         }
      }
      
      if (closeIndex == -1)
      {
         return false; 
      }
      
      var contentStart = state.GlobalOffset + markerLength;
      var contentLength = closeIndex - markerLength;

      if (contentLength >= 2 && text[markerLength] == ' ' && text[closeIndex - 1] == ' ')
      {
         var allSpaces = true;
         for (var k = markerLength; k < closeIndex; k++)
         {
            if (text[k] != ' ') { allSpaces = false; break; }
         }

         if (!allSpaces)
         {
            contentStart++;
            contentLength -= 2;
         }
      }

      var nodeIndex = writer.WrittenSpan.Length;
      writer.Add(new MarkdownNode()
      {
         Type = NodeType.InlineCode,
         TextSpan = new TextSpan(contentStart, contentLength),
         FirstChildIndex = -1,
         NextSiblingIndex = -1
      });

      parser.LinkInlineNode(ref writer, parentIndex, nodeIndex);
      
      state.Advance(closeIndex + markerLength);
      return true;
   }
}
