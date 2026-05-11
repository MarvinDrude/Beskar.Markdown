using Beskar.Markdown.Parsing.Models;
using Beskar.Markdown.Parsing.Interfaces;
using Me.Memory.Buffers;

namespace Beskar.Markdown.Parsing.Blocks;

public sealed class LinkReferenceDefinitionParser : IBlockParser
{
   public int Priority => 100;
   public int SupportedTypeValue => (int)NodeType.LinkReferenceDefinition;

   public int TryMatch(ref LineState state, int parentIndex, ref BufferWriter<MarkdownNode> writer)
   {
      if (state.LeadingSpaces >= 4 || state.FirstChar != '[')
         return -1;


      return -1;
   }

   public bool CanContinue(ref MarkdownNode node, ref LineState state, ref BufferWriter<MarkdownNode> writer)
   {
      if (state.IsBlank)
         return false;

      return false;
   }
}