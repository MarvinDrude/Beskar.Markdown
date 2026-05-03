using System.Runtime.InteropServices;
using Beskar.Markdown.Extensions;
using Beskar.Markdown.Parsing.Models;
using Me.Memory.Buffers;

namespace Beskar.Markdown.Parsing;

[StructLayout(LayoutKind.Auto)]
public ref struct InlineParser(ReadOnlySpan<char> rawText)
{
   private readonly ReadOnlySpan<char> _rawText = rawText;

   public void Parse(ref BufferWriter<MarkdownNode> writer, ParserOptions options)
   {
      // dont process new added nodes
      var originalBlockCount = writer.WrittenSpan.Length;

      for (var i = 0; i < originalBlockCount; i++)
      {
         var type = writer.WrittenSpan[i].Type;

         // We only process inlines inside leaf blocks
         if (type is NodeType.Paragraph or NodeType.Header)
         {
            RunLeafContainer(i, ref writer, options);
         }
      }
   }

   private void RunLeafContainer(
      int parentIndex,
      ref BufferWriter<MarkdownNode> writer,
      ParserOptions options)
   {
      ref var parent = ref writer.GetReference(parentIndex);
      var currentChildIndex = parent.FirstChildIndex;

      parent.FirstChildIndex = -1;
      if (currentChildIndex != -1)
      {
         var nextNode = currentChildIndex;
         while (nextNode != -1)
         {
            var oldNode = writer.WrittenSpan[nextNode];
            var span = _rawText.Slice(oldNode.TextSpan.Start, oldNode.TextSpan.Length);
            
            var state = new InlineState(span, oldNode.TextSpan.Start);
            ProcessState(ref state, parentIndex, ref writer, options);

            nextNode = oldNode.NextSiblingIndex;
            
            if (nextNode != -1)
            {
               // add soft break if another line
               AddInlineNode(ref writer, parentIndex, NodeType.SoftBreak, state.GlobalOffset, 0);
            }
         }
      }
      else if (parent.TextSpan.Length > 0)
      {
         // header, for example, has its text inside itself
         var span = _rawText.Slice(parent.TextSpan.Start, parent.TextSpan.Length);
         var state = new InlineState(span, parent.TextSpan.Start);
         
         ProcessState(ref state, parentIndex, ref writer, options);
      }
   }
   
   private void ProcessState(
      ref InlineState state, 
      int parentIndex, 
      ref BufferWriter<MarkdownNode> writer, 
      ParserOptions options)
   {
      var plainTextStart = state.GlobalOffset;
      var plainTextLength = 0;

      while (state.RemainingText.Length > 0)
      {
         var matched = false;

         for (var i = 0; i < options.InlineParsers.Length; i++)
         {
            var parser = options.InlineParsers[i];
            if (state.RemainingText[0] == parser.TriggerChar)
            {
               if (plainTextLength > 0)
               {
                  // any plain text left over before?
                  AddInlineNode(ref writer, parentIndex, NodeType.Text, plainTextStart, plainTextLength);
                  plainTextLength = 0;
               }

               if (parser.TryMatch(ref state, parentIndex, ref writer, ref this))
               {
                  matched = true;
                  plainTextStart = state.GlobalOffset; // Reset tracker
                  break;
               }
            }
         }

         if (!matched)
         {
            // no match, treat as plain text
            plainTextLength++;
            state.Advance(1);
         }
      }

      // Flush any leftover text at the end of the line
      if (plainTextLength > 0)
      {
         AddInlineNode(ref writer, parentIndex, NodeType.Text, plainTextStart, plainTextLength);
      }
   }
   
   public void AddInlineNode(ref BufferWriter<MarkdownNode> writer, int parentIndex, 
      NodeType type, int start, int length)
   {
      var nodeIndex = writer.WrittenSpan.Length;
      writer.Add(new MarkdownNode()
      {
         Type = type,
         TextSpan = new TextSpan(start, length),
         FirstChildIndex = -1,
         NextSiblingIndex = -1
      });

      LinkInlineNode(ref writer, parentIndex, nodeIndex);
   }

   public void LinkInlineNode(ref BufferWriter<MarkdownNode> writer, int parentIndex, int childIndex)
   {
      ref var parent = ref writer.GetReference(parentIndex);
      
      if (parent.FirstChildIndex == -1)
      {
         parent.FirstChildIndex = childIndex;
         return;
      }

      var current = parent.FirstChildIndex;
      while (writer.WrittenSpan[current].NextSiblingIndex != -1)
      {
         current = writer.WrittenSpan[current].NextSiblingIndex;
      }
      
      writer.GetReference(current).NextSiblingIndex = childIndex;
   }
}