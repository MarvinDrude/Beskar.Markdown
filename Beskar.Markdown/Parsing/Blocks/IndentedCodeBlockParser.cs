using Beskar.Markdown.Extensions;
using Beskar.Markdown.Parsing.Interfaces;
using Beskar.Markdown.Parsing.Models;
using Me.Memory.Buffers;

namespace Beskar.Markdown.Parsing.Blocks;

public sealed class IndentedCodeBlockParser : IBlockParser
{
   public int Priority => 10_095;
   public int SupportedTypeValue => (int)NodeType.IndentedCodeBlock;
   
   public int TryMatch<TData>(ref LineState<TData> state, int parentIndex, ref BufferWriter<MarkdownNode> writer)
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
         LastChildIndex = nodeIndex + 1,
         NextSiblingIndex = -1,
         CodeBlockMarker = '\0',
         CodeBlockFenceCount = 0,
         CodeLangSpanStart = 0,
         CodeLangSpanLength = 0
      });
      
      const int skipAmount = 4;
      writer.Add(new MarkdownNode()
      {
         Type = NodeType.IndentedCodeFragment,
         TextSpan = new TextSpan(state.GlobalOffset + skipAmount, state.RawLine.Length - skipAmount),
         FirstChildIndex = -1,
         NextSiblingIndex = -1
      });

      state.ConsumeRest();
      return nodeIndex;
   }

   public bool CanContinue<TData>(ref MarkdownNode node, ref LineState<TData> state, 
      ref BufferWriter<MarkdownNode> writer)
   {
      if (state.LeadingSpaces < 4 && !state.IsBlank) 
         return false;
      
      var newLength = (state.GlobalOffset - node.TextSpan.Start) + state.RawLine.Length;
      node.TextSpan = node.TextSpan with { Length = newLength };
      const int skipAmount = 4;
      
      var newLineIndex = writer.WrittenSpan.Length;
       
      writer.GetReference(node.LastChildIndex).NextSiblingIndex = newLineIndex;
      writer.Add(new MarkdownNode()
      {
         Type = NodeType.IndentedCodeFragment,
         TextSpan = new TextSpan(state.GlobalOffset + skipAmount, state.RawLine.Length - skipAmount),
         FirstChildIndex = -1,
         NextSiblingIndex = -1
      });
      node.LastChildIndex = newLineIndex;
       
      state.ConsumeRest();
      return true;

   }
}
