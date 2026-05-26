using Beskar.Markdown.Parsing.Models;
using Beskar.Memory.Buffers;
using Beskar.Memory.Writers;

namespace Beskar.Markdown.Parsing.Interfaces;

public interface IInlineParser : IParser
{
   public char TriggerChar { get; }
   
   public char TriggerAltChar { get; }
   
   public bool TryMatch<TData>(
      ref InlineState<TData> state, 
      int parentIndex, 
      ref BufferWriter<MarkdownNode> writer, 
      scoped ref InlineParser<TData> parser,
      ParserOptions options);
}
