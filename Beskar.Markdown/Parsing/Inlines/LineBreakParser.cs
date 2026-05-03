using Beskar.Markdown.Parsing.Interfaces;
using Beskar.Markdown.Parsing.Models;
using Me.Memory.Buffers;

namespace Beskar.Markdown.Parsing.Inlines;

public sealed class LineBreakParser : IInlineParser
{
   public int Priority => 10_000;
   public int SupportedTypeValue => (int)NodeType.LineBreak;

   public char TriggerChar => '\\';
   
   public bool TryMatch(ref InlineState state, int parentIndex, ref BufferWriter<MarkdownNode> writer, ref InlineParser parser)
   {
      if (state.RemainingText.Length != 1) return false;
      
      var nodeIndex = writer.WrittenSpan.Length;
      writer.Add(new MarkdownNode()
      {
         Type = NodeType.LineBreak,
         TextSpan = new TextSpan(state.GlobalOffset, 1),
         FirstChildIndex = -1,
         NextSiblingIndex = -1
      });

      parser.LinkInlineNode(ref writer, parentIndex, nodeIndex);
      state.Advance(1);
      return true;
   }
}