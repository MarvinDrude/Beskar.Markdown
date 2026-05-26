using Beskar.Markdown.Parsing.Interfaces;
using Beskar.Markdown.Parsing.Models;
using Beskar.Memory.Buffers;
using Beskar.Memory.Writers;

namespace Beskar.Markdown.Parsing.Inlines;

public sealed class LineBreakParser : IInlineParser
{
   public int Priority => 7_000;
   public int SupportedTypeValue => (int)NodeType.LineBreak;

   public char TriggerChar => '\\';
   public char TriggerAltChar => '\\';
   
   public bool TryMatch<TData>(ref InlineState<TData> state, int parentIndex, 
      ref BufferWriter<MarkdownNode> writer, scoped ref InlineParser<TData> parser,
      ParserOptions options)
   {
      if (state.RemainingText.Length != 1) return false;

      var nextPosition = state.GlobalOffset + 1;
      if (state.RawText.Length <= nextPosition) 
         return false;

      var isR = state.RawText[nextPosition] == '\r';
      if (!isR && state.RawText[nextPosition] != '\n')
         return false;
      
      nextPosition++;
      if (isR && (nextPosition >= state.RawText.Length || state.RawText[nextPosition] != '\n'))
         return false;
      
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
