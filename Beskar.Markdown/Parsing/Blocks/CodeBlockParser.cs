using Beskar.Markdown.Parsing.Interfaces;
using Beskar.Markdown.Parsing.Models;
using Me.Memory.Buffers;

namespace Beskar.Markdown.Parsing.Blocks;

public sealed class CodeBlockParser : IBlockParser
{
   public int Priority => 10_100;
   public int SupportedTypeValue => (int)NodeType.CodeBlock;
   
   public int TryMatch(ref LineState state, int parentIndex, ref BufferWriter<MarkdownNode> writer)
   {
      return -1;
   }

   public bool CanContinue(ref MarkdownNode node, ref LineState state)
   {
      return false;
   }
}