using Beskar.Markdown.Parsing.Interfaces;
using Beskar.Markdown.Parsing.Models;
using Me.Memory.Buffers;

namespace Beskar.Markdown.Parsing.Inlines;

public sealed class StrikethroughParser : IInlineParser
{
   public int Priority => 6_500;
   public int SupportedTypeValue => (int)NodeType.StrikeThrough;
   
   public char TriggerChar => '~';
   public char TriggerAltChar => '~';
   
   public bool TryMatch(ref InlineState state, int parentIndex, ref BufferWriter<MarkdownNode> writer, ref InlineParser parser)
   {
      var text = state.RemainingText;
      if (text.Length < 5 || text[0] != '~' || text[1] != '~')
      {
         return false;
      }
      
      var closeIdx = -1;
      for (var i = 2; i < text.Length - 1; i++)
      {
         if (text[i] == '~' && text[i + 1] == '~')
         {
            closeIdx = i;
            break;
         }
      }

      if (closeIdx == -1) return false;
      
      var contentStart = state.GlobalOffset + 2;
      var contentLength = closeIdx - 2;

      var nodeIndex = writer.WrittenSpan.Length;
      writer.Add(new MarkdownNode()
      {
         Type = NodeType.StrikeThrough,
         TextSpan = new TextSpan(contentStart, contentLength),
         FirstChildIndex = -1,
         NextSiblingIndex = -1
      });

      parser.LinkInlineNode(ref writer, parentIndex, nodeIndex);
      
      state.Advance(closeIdx + 2);
      return true;
   }
}