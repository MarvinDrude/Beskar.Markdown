using System.Resources;
using Beskar.Markdown.Extensions;
using Beskar.Markdown.Parsing.Interfaces;
using Beskar.Markdown.Parsing.Models;
using Me.Memory.Buffers;

namespace Beskar.Markdown.Parsing.Blocks;

public sealed class CodeBlockParser : IBlockParser
{
   public int Priority => 10_100;
   public int SupportedTypeValue => (int)NodeType.CodeBlock;
   
   public int TryMatch<TData>(ref LineState<TData> state, int parentIndex, ref BufferWriter<MarkdownNode> writer)
   {
      if (state.IsBlank || state.LeadingSpaces >= 4)
      {
         return -1;
      }
      
      var marker = state.FirstChar;
      if (marker != '`' && marker != '~')
      {
         return -1;
      }
      
      var count = 0;
      var rawLine = state.RawLine;
      var i = state.FirstNonSpaceIndex;
      
      for (; i < rawLine.Length; i++)
      {
         if (rawLine[i] == marker)
         {
            count++;
         }
         else
         {
            break;
         }
      }
      
      if (count < 3) return -1;
      
      var langStart = -1;
      var langLength = 0;
      
      for (; i < rawLine.Length; i++)
      {
         if (rawLine[i] != ' ' && rawLine[i] != '\t')
         {
            langStart = i;
            break;
         }
      }
      
      if (langStart != -1)
      {
         var langEnd = langStart;
         for (; langEnd < rawLine.Length; langEnd++)
         {
            if (rawLine[langEnd] == ' ' || rawLine[langEnd] == '\t') break;
         }
         
         langLength = langEnd - langStart;
         if (marker == '`')
         {
            for (var j = langStart; j < rawLine.Length; j++)
            {
               if (rawLine[j] == '`') return -1;
            }
         }
      }
      
      var nodeIndex = writer.WrittenSpan.Length;
      writer.Add(new MarkdownNode()
      {
         Type = NodeType.CodeBlock,
         TextSpan = new TextSpan(-1, 0),
         FirstChildIndex = -1,
         NextSiblingIndex = -1,
         CodeBlockMarker = marker,
         CodeBlockFenceCount = (ushort)count,
         CodeLangSpanStart = langStart != -1 ? state.GlobalOffset + langStart : 0,
         CodeLangSpanLength = langLength
      });

      state.ConsumeRest();
      return nodeIndex;
   }

   public bool CanContinue<TData>(ref MarkdownNode node, ref LineState<TData> state, 
      ref BufferWriter<MarkdownNode> writer)
   {
      var marker = node.CodeBlockMarker;
      var minCount = node.CodeBlockFenceCount;

      var isClosingFence = false;
      
      // A closing fence cannot be indented by 4 or more spaces, and must start with the same marker
      if (!state.IsBlank && state.LeadingSpaces < 4 && state.FirstChar == marker)
      {
         var count = 0;
         for (var i = state.FirstNonSpaceIndex; i < state.RawLine.Length; i++)
         {
            if (state.RawLine[i] == marker) count++;
            else break;
         }

         if (count >= minCount)
         {
            isClosingFence = true;
            for (var i = state.FirstNonSpaceIndex + count; i < state.RawLine.Length; i++)
            {
               var c = state.RawLine[i];
               if (c != ' ' && c != '\t' && c != '\r' && c != '\n')
               {
                  isClosingFence = false;
                  break;
               }
            }
            
            if (isClosingFence)
            {
               // Correctly get the last new line in the code block
               if (node.TextSpan.Start != -1)
               {
                  var newLength = state.GlobalOffset - node.TextSpan.Start;
                  node.TextSpan = node.TextSpan with { Length = newLength };
               }
            }
         }
      }

      if (isClosingFence)
      {
         state.ConsumeRest();
         if (node.TextSpan.Start == -1)
         {
            node.TextSpan = new TextSpan(state.GlobalOffset, 0);
         }
         
         return false;
      }
      
      if (node.TextSpan.Start == -1)
      {
         node.TextSpan = new TextSpan(state.GlobalOffset, state.RawLine.Length);
      }
      else
      {
         var newLength = (state.GlobalOffset - node.TextSpan.Start) + state.RawLine.Length;
         node.TextSpan = node.TextSpan with { Length = newLength };
      }
      
      state.ConsumeRest();

      var remainingChars = state.FullText.Length - state.GlobalOffset;

      if ((remainingChars == 1 && state.FullText[state.GlobalOffset] == '\n') ||
         (remainingChars == 2 && state.FullText[state.GlobalOffset] == '\r' 
            && state.FullText[state.GlobalOffset + 1] == '\n'))
      {
         // If an open code block isn't closed and the document ends
         if (node.TextSpan.Start != -1)
         {
            var newLength = state.FullText.Length - node.TextSpan.Start;
            node.TextSpan = node.TextSpan with { Length = newLength };
         }
      }
      
      return !isClosingFence;
   }
}
