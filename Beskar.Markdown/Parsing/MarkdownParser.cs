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
   public ReadOnlySpan<MarkdownNode> WrittenNodes => _writer.WrittenSpan;
   
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
         
         // Phase one: continue open blocks if possible
         for (var i = 1; i < openBlockCount; i++)
         {
            var nodeIdx = openBlocks[i];
            ref var node = ref _writer.GetReference(nodeIdx);
            
            // check for interrupts
            if (node.Type is NodeType.Paragraph)
            {
               if (!state.IsBlank) matchedLevels++;
               break;
            }
            
            var parser = options.GetParserForType((int)node.Type);
            if (parser == null || !parser.CanContinue(ref node, ref state))
            {
               break;
            }
            
            matchedLevels++;
         }
         
         openBlockCount = matchedLevels;
         var matchedNew = false;
         
         // Phase two: try to match new blocks
         while (openBlockCount < options.MaxBlockDepth)
         {
            var currentParentIndex = openBlocks[openBlockCount - 1];
            var foundNewNodeIndex = -1;

            // Check if we are currently inside a paragraph that might be interrupted
            var isParagraphOpen = _writer.WrittenSpan[currentParentIndex].Type is NodeType.Paragraph;
            var testParentIndex = isParagraphOpen ? openBlocks[openBlockCount - 2] : currentParentIndex;
            
            for (var index = 0; index < options.BlockParsers.Length; index++)
            {
               var parser = options.BlockParsers[index];
               // Skip the paragraph fallback here, we handle it explicitly in Phase three
               if (parser.SupportedTypeValue == (int)NodeType.Paragraph)
               {
                  continue;
               }
               
               var tempState = state;
               foundNewNodeIndex = parser.TryMatch(ref tempState, testParentIndex, ref _writer);
               
               if (foundNewNodeIndex != -1) 
               {
                  state = tempState; // commit state change
                  break;
               }
            }

            if (foundNewNodeIndex != -1)
            {
               matchedNew = true;
               
               if (isParagraphOpen)
               {
                  // The new block interrupted the paragraph
                  openBlockCount--;
                  currentParentIndex = testParentIndex;
               }
               
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

         if (!matchedNew)
         {
            var currentParentIndex = openBlocks[openBlockCount - 1];
            ref var parentNode = ref _writer.GetReference(currentParentIndex);

            if (state.IsBlank)
            {
               // close paragraph on a blank line
               if (parentNode.Type is NodeType.Paragraph)
               {
                  openBlockCount--;
               }
            }
            else
            {
               if (parentNode.Type is NodeType.Paragraph)
               {
                  // Continuation: Extend the TextSpan of the existing paragraph
                  var currentLength = (state.GlobalOffset + state.RawLine.Length) - parentNode.TextSpan.Start;
                  parentNode.TextSpan = parentNode.TextSpan with { Length = currentLength };
               }
               else
               {
                  // Start a new paragraph
                  var pParser = options.GetParserForType((int)NodeType.Paragraph);
                  if (pParser != null)
                  {
                     var pIndex = pParser.TryMatch(ref state, currentParentIndex, ref _writer);
                     if (pIndex != -1)
                     {
                        LinkNodes(currentParentIndex, pIndex);
                        openBlocks[openBlockCount++] = pIndex;
                     }
                  }
               }
            }
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