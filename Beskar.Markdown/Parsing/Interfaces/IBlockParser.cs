using Beskar.Markdown.Parsing.Models;
using Me.Memory.Buffers;

namespace Beskar.Markdown.Parsing.Interfaces;

public interface IBlockParser : IParser
{
   /// <summary>
   /// Tries to start a brand-new block.
   /// </summary>
   public int TryMatch(ref LineState state, int parentIndex, ref BufferWriter<MarkdownNode> writer);
   
   /// <summary>
   /// Check if the current block can continue on the current line.
   /// </summary>
   public bool CanContinue(ref MarkdownNode node, ref LineState state);
}