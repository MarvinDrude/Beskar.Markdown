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
      
      state.SliceIndentation(4);
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
      
      writer.Add(new MarkdownNode()
      {
         Type = NodeType.IndentedCodeFragment,
         LeadingVirtualSpaces = (byte)state.VirtualSpaces,
         TextSpan = new TextSpan(state.GlobalOffset, state.RawLine.Length),
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

      state.SliceIndentation(4);
      
      var newLineIndex = writer.WrittenSpan.Length;
       
      writer.GetReference(node.LastChildIndex).NextSiblingIndex = newLineIndex;
      writer.Add(new MarkdownNode()
      {
         Type = NodeType.IndentedCodeFragment,
         LeadingVirtualSpaces = (byte)state.VirtualSpaces,
         TextSpan = new TextSpan(state.GlobalOffset, state.RawLine.Length),
         FirstChildIndex = -1,
         NextSiblingIndex = -1
      });
      node.LastChildIndex = newLineIndex;
       
      state.ConsumeRest();
      return true;

   }
   
   private static int CalculatePhysicalSkipAmount(ReadOnlySpan<char> line)
   {
      var physicalChars = 0;
      var logicalSpaces = 0;

      for (var i = 0; i < line.Length && logicalSpaces < 4; i++)
      {
         if (line[i] == '\t')
         {
            physicalChars++;
            break;
         }
         
         if (line[i] == ' ')
         {
            physicalChars++;
            logicalSpaces++;
         }
         else
         {
            break;
         }
      }

      return physicalChars;
   }
}
