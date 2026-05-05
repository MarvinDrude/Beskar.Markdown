using Beskar.Markdown.Parsing.Interfaces;
using Beskar.Markdown.Parsing.Models;
using Me.Memory.Buffers;

namespace Beskar.Markdown.Parsing.Inlines;

public sealed class InlineHtmlParser : IInlineParser
{
   public int Priority => 6_000;
   public int SupportedTypeValue => (int)NodeType.InlineHtml;

   public char TriggerChar => '<';
   public char TriggerAltChar => '<';
   
   public bool TryMatch(ref InlineState state, int parentIndex, ref BufferWriter<MarkdownNode> writer, ref InlineParser parser)
   {
      var text = state.RemainingText;
      if (text.Length < 3) return false;

      var next = text[1];
      var isValidStart = char.IsLetter(next) 
         || next == '/' || next == '!' || next == '?';
      
      if (!isValidStart) return false;

      var closeIdx = -1;
      for (var i = 1; i < text.Length; i++)
      {
         if (text[i] == '>')
         {
            closeIdx = i;
            break;
         }
      }

      if (closeIdx == -1) return false;

      var nodeIndex = writer.WrittenSpan.Length;
      writer.Add(new MarkdownNode()
      {
         Type = NodeType.InlineHtml,
         TextSpan = new TextSpan(state.GlobalOffset, closeIdx + 1),
         FirstChildIndex = -1,
         NextSiblingIndex = -1
      });

      parser.LinkInlineNode(ref writer, parentIndex, nodeIndex);
      state.Advance(closeIdx + 1);
      
      return true;
   }
}