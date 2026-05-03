using System.Runtime.InteropServices;
using Beskar.Markdown.Extensions;
using Beskar.Markdown.Parsing.Models;
using Me.Memory.Buffers;

namespace Beskar.Markdown.Parsing;

/// <summary>
/// Main parser struct to run the raw text to AST conversion.
/// </summary>
[StructLayout(LayoutKind.Auto)]
public ref struct MarkdownParser(
   ReadOnlySpan<char> rawText,
   Span<MarkdownNode> initialNodeBuffer)
{
   private ReadOnlySpan<char> _rawText = rawText;
   private BufferWriter<MarkdownNode> _writer = new(initialNodeBuffer);

   /// <summary>
   /// Main parse logic to construct the AST of the Markdown document.
   /// </summary>
   public void Parse(ParserOptions options)
   {
      var documentIndex = _writer.WrittenSpan.Length;
      _writer.Add(new MarkdownNode()
      {
         Type = NodeType.Document, 
         FirstChildIndex = -1, 
         NextSiblingIndex = -1
      });

      // Secured to be a max length of 32 by setter
      Span<int> openBlocks = stackalloc int[options.MaxBlockDepth];
      openBlocks[0] = documentIndex;
      var openBlockCount = 1;
      
      var iterator = new LineIterator(_rawText);
      while (iterator.TryMoveNext(out var state))
      {
         var matchedLevels = 1; // ignore the document node
         for (var i = 1; i < openBlockCount; i++)
         {
            var nodeIdx = openBlocks[i];
            ref var node = ref _writer.GetReference(nodeIdx);
            
            var parser = options.GetParserForType((int)node.Type);
            if (parser == null || !parser.CanContinue(ref node, ref state))
            {
               break;
            }
            
            matchedLevels++;
         }
         
         openBlockCount = matchedLevels;
         var matchedNew = false;
         
         while (openBlockCount < options.MaxBlockDepth)
         {
            var currentParentIndex = openBlocks[openBlockCount - 1];
            var foundNewNodeIndex = -1;

            for (var index = 0; index < options.BlockParsers.Length; index++)
            {
               var parser = options.BlockParsers[index];
               
               foundNewNodeIndex = parser.TryMatch(ref state, currentParentIndex, ref _writer);
               if (foundNewNodeIndex != -1) break;
            }

            if (foundNewNodeIndex != -1)
            {
               matchedNew = true;
               LinkNodes(currentParentIndex, foundNewNodeIndex);

               var newType = (int)_writer.WrittenSpan[foundNewNodeIndex].Type;
               if (options.IsParserType(newType))
               {
                  openBlocks[openBlockCount++] = foundNewNodeIndex;
                  continue; // Line might contain more: e.g., "> - Item"
               }

               // It's a leaf (Heading, Code), no more nesting possible
            }

            break; // no more block parsers matched
         }

         if (!matchedNew && !state.IsBlank)
         {
            // handle leaf
            _ = "";
         }
      }
   }

   private void LinkNodes(int parentIndex, int childIndex)
   {
      ref var parent = ref _writer.GetReference(parentIndex);

      if (parent.FirstChildIndex == -1)
      {
         parent.FirstChildIndex = childIndex;
         return;
      }

      var nodes = _writer.WrittenSpan;
      var current = parent.FirstChildIndex;
      
      while (nodes[current].NextSiblingIndex != -1)
      {
         current = nodes[current].NextSiblingIndex;
      }
      
      ref var child = ref _writer.GetReference(current);
      child.NextSiblingIndex = childIndex;
   }
}