using Beskar.Markdown.Parsing.Models;
using Me.Memory.Buffers;

namespace Beskar.Markdown.Parsing.Interfaces;

public interface IInlineParser : IParser
{
   public char TriggerChar { get; }
   
   public char TriggerAltChar { get; }
   
   public bool TryMatch(ref InlineState state, int parentIndex, ref BufferWriter<MarkdownNode> writer, ref InlineParser parser);
}