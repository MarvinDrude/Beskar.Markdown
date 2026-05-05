using Beskar.Markdown.Extensions;
using Beskar.Markdown.Parsing.Interfaces;
using Beskar.Markdown.Parsing.Models;
using Me.Memory.Buffers;

namespace Beskar.Markdown.Parsing.Blocks;

public sealed class IndentedCodeBlockParser : IBlockParser
{
   public int Priority => 10_095;
   public int SupportedTypeValue => (int)NodeType.IndentedCodeBlock;
   
   public int TryMatch(ref LineState state, int parentIndex, ref BufferWriter<MarkdownNode> writer)
   {
      if (state.IsBlank || state.LeadingSpaces < 4)
      {
         return -1;
      }
      
      var nodeIndex = writer.WrittenSpan.Length;
      writer.Add(new MarkdownNode()
      {
         Type = NodeType.IndentedCodeBlock,
         TextSpan = new TextSpan(state.GlobalOffset, state.RawLine.Length),
         FirstChildIndex = nodeIndex + 1,
         NextSiblingIndex = -1,
         CodeBlockMarker = '\0',
         CodeBlockFenceCount = 0,
         CodeLangSpanStart = 0,
         CodeLangSpanLength = 0
      });
      
      var skipAmount = state.LeadingSpaces;
      writer.Add(new MarkdownNode()
      {
         Type = NodeType.IndentedCodeFragment,
         TextSpan = new TextSpan(state.GlobalOffset + skipAmount, state.RawLine.Length - skipAmount),
         FirstChildIndex = -1,
         NextSiblingIndex = -1
      });

      state.Slice(state.RawLine.Length);
      return nodeIndex;
   }

   public bool CanContinue(ref MarkdownNode node, ref LineState state, ref BufferWriter<MarkdownNode> writer)
   {
      if (state.LeadingSpaces < 4 && !state.IsBlank) 
         return false;
      
      var newLength = (state.GlobalOffset - node.TextSpan.Start) + state.RawLine.Length;
      node.TextSpan = node.TextSpan with { Length = newLength };
      var skipAmount = state.LeadingSpaces;
      
      var lastChildIndex = node.FirstChildIndex;
      while (writer.WrittenSpan[lastChildIndex].NextSiblingIndex != -1)
      {
         lastChildIndex = writer.WrittenSpan[lastChildIndex].NextSiblingIndex;
      }
      
      var newLineIndex = writer.WrittenSpan.Length;
      
      writer.GetReference(lastChildIndex).NextSiblingIndex = newLineIndex;
      writer.Add(new MarkdownNode()
      {
         Type = NodeType.IndentedCodeFragment,
         TextSpan = new TextSpan(state.GlobalOffset + skipAmount, state.RawLine.Length - skipAmount),
         FirstChildIndex = -1,
         NextSiblingIndex = -1
      });
      
      state.Slice(state.RawLine.Length);
      return true;

   }
}