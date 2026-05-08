using System.Runtime.InteropServices;
using Beskar.Markdown.Extensions;
using Beskar.Markdown.Parsing.Models;
using Me.Memory.Buffers;

namespace Beskar.Markdown.Parsing;

[StructLayout(LayoutKind.Auto)]
public ref struct InlineParser(ReadOnlySpan<char> rawText) : IDisposable
{
   private readonly ReadOnlySpan<char> _rawText = rawText;
   private BufferWriter<Delimiter> _delimiters;
   
   private bool _hasDelimiterBuffer;

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

      // clear the delimiters
      if (_hasDelimiterBuffer)
      {
         _delimiters.Position = 0;
      }

      parent.FirstChildIndex = -1;
      parent.LastChildIndex = -1;
      
      if (currentChildIndex != -1)
      {
         var nextNode = currentChildIndex;
         while (nextNode != -1)
         {
            var oldNode = writer.WrittenSpan[nextNode];
            var isLastLine = oldNode.NextSiblingIndex == -1;
            var span = _rawText.Slice(oldNode.TextSpan.Start, oldNode.TextSpan.Length);

            var state = new InlineState(_rawText, span, oldNode.TextSpan.Start);
            ProcessState(ref state, parentIndex, ref writer, options, isLastLine);

            nextNode = oldNode.NextSiblingIndex;

            if (nextNode != -1)
            {
               var lastNodeIndex = writer.WrittenSpan.Length - 1;
               if (lastNodeIndex < 0 || writer.WrittenSpan[lastNodeIndex].Type != NodeType.LineBreak)
               {
                  // add soft break if another line
                  AddInlineNode(ref writer, parentIndex, NodeType.SoftBreak, state.GlobalOffset, 0);
               }
            }
         }
      }
      else if (parent.TextSpan.Length > 0)
      {
         // header, for example, has its text inside itself
         var span = _rawText.Slice(parent.TextSpan.Start, parent.TextSpan.Length);
         var state = new InlineState(_rawText, span, parent.TextSpan.Start);

         ProcessState(ref state, parentIndex, ref writer, options, isLastLine: true);
      }

      ProcessDelimiters(ref writer);
   }
   
   private void ProcessState(
      ref InlineState state,
      int parentIndex,
      ref BufferWriter<MarkdownNode> writer,
      ParserOptions options,
      bool isLastLine)
   {
      var plainTextStart = state.GlobalOffset;
      var plainTextLength = 0;

      while (state.RemainingText.Length > 0)
      {
         var matched = false;
         var c = state.RemainingText[0];

         var parser = options.GetInlineParser(c);
         if (parser != null)
         {
            if (plainTextLength > 0)
            {
               // any plain text left over before?
               AddInlineNode(ref writer, parentIndex, NodeType.Text, plainTextStart, plainTextLength);
               plainTextLength = 0;

               plainTextStart = state.GlobalOffset;
            }

            if (parser.TryMatch(ref state, parentIndex, ref writer, ref this))
            {
               matched = true;
               plainTextStart = state.GlobalOffset; // Reset tracker
            }
         }

         if (!matched)
         {
            plainTextLength++;
            state.Advance(1);
         }
      }

      // Flush any leftover text at the end of the line
      if (plainTextLength > 0)
      {
         if (!isLastLine && plainTextLength >= 2)
         {
            var trailingSpaces = 0;
            while (trailingSpaces < plainTextLength &&
                   state.RawText[plainTextStart + plainTextLength - 1 - trailingSpaces] == ' ')
            {
               trailingSpaces++;
            }

            if (trailingSpaces >= 2)
            {
               var textLen = plainTextLength - trailingSpaces;
               if (textLen > 0)
                  AddInlineNode(ref writer, parentIndex, NodeType.Text, plainTextStart, textLen);
               AddInlineNode(ref writer, parentIndex, NodeType.LineBreak, plainTextStart + textLen, trailingSpaces);
               return;
            }
         }

         AddInlineNode(ref writer, parentIndex, NodeType.Text, plainTextStart, plainTextLength);
      }
   }
   
   private void ProcessDelimiters(ref BufferWriter<MarkdownNode> writer)
   {
      if (!_hasDelimiterBuffer) return;

      var delimiters = _delimiters.WrittenSpan;
      if (delimiters.Length == 0) return;

      for (var i = 0; i < delimiters.Length; i++)
      {
         ref var closer = ref _delimiters.GetReference(i);
         if (!closer.Active || !closer.CanClose) continue;

         for (var j = i - 1; j >= 0; j--)
         {
            ref var opener = ref _delimiters.GetReference(j);

            if (!opener.Active || !opener.CanOpen || opener.Marker != closer.Marker)
               continue;

            if ((opener.CanOpen || opener.CanClose) && (closer.CanOpen || closer.CanClose))
            {
               if ((opener.Length + closer.Length) % 3 == 0 && (opener.Length % 3 != 0 || closer.Length % 3 != 0))
               {
                  continue;
               }
            }

            var consumed = Math.Min(opener.Length, closer.Length);
            if (consumed > 2) consumed = 2;

            var openerNodeIdx = opener.NodeIndex;
            var closerNodeIdx = closer.NodeIndex;

            var emphType = consumed == 1 ? NodeType.Emphasis : NodeType.StrongEmphasis;

            var firstChildIdx = writer.WrittenSpan[openerNodeIdx].NextSiblingIndex;
            var afterCloser = writer.WrittenSpan[closerNodeIdx].NextSiblingIndex;

            if (firstChildIdx == closerNodeIdx)
            {
               firstChildIdx = -1;
            }
            else if (closer.PreviousNodeIndex != -1 &&
                     writer.WrittenSpan[closer.PreviousNodeIndex].NextSiblingIndex == closerNodeIdx)
            {
               writer.GetReference(closer.PreviousNodeIndex).NextSiblingIndex = -1;
            }
            else
            {
               var current = firstChildIdx;
               while (current != -1 && writer.WrittenSpan[current].NextSiblingIndex != closerNodeIdx)
                  current = writer.WrittenSpan[current].NextSiblingIndex;
               if (current != -1)
                  writer.GetReference(current).NextSiblingIndex = -1;
            }

            var openerFullyConsumed = opener.Length <= consumed;
            var closerFullyConsumed = closer.Length <= consumed;

            if (openerFullyConsumed)
            {
               ref var openerNode = ref writer.GetReference(openerNodeIdx);
               
               openerNode.Type = emphType;
               openerNode.FirstChildIndex = firstChildIdx;
               openerNode.NextSiblingIndex = closerFullyConsumed ? afterCloser : closerNodeIdx;
            }
            else
            {
               var openerSpan = writer.WrittenSpan[openerNodeIdx].TextSpan;

               writer.GetReference(openerNodeIdx).TextSpan =
                  new TextSpan(openerSpan.Start, opener.Length - consumed);

               var emphIdx = writer.WrittenSpan.Length;
               writer.Add(new MarkdownNode()
               {
                  Type = emphType,
                  TextSpan = new TextSpan(openerSpan.Start + opener.Length - consumed, 0),
                  FirstChildIndex = firstChildIdx,
                  NextSiblingIndex = closerFullyConsumed ? afterCloser : closerNodeIdx
               });

               writer.GetReference(openerNodeIdx).NextSiblingIndex = emphIdx;
            }

            if (!closerFullyConsumed)
            {
               ref var closerNode = ref writer.GetReference(closerNodeIdx);
               closerNode.TextSpan = new TextSpan(
                  closerNode.TextSpan.Start + consumed,
                  closer.Length - consumed);
               closerNode.NextSiblingIndex = afterCloser;
            }

            opener.Length -= consumed;
            closer.Length -= consumed;

            if (opener.Length == 0)
               opener.Active = false;

            if (closer.Length == 0)
               closer.Active = false;
            else
               i--;

            for (var k = j + 1; k < i; k++)
               _delimiters.GetReference(k).Active = false;

            break;
         }
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
         LastChildIndex = -1,
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
         parent.LastChildIndex = childIndex;
         return;
      }

      ref var lastChild = ref writer.GetReference(parent.LastChildIndex);
      lastChild.NextSiblingIndex = childIndex;
      parent.LastChildIndex = childIndex;
   }

   public void AddDelimiter(scoped in Delimiter delimiter)
   {
      if (!_hasDelimiterBuffer)
      {
         _delimiters = new BufferWriter<Delimiter>(16);
         _hasDelimiterBuffer = true;
      }

      _delimiters.Add(delimiter);
   }

   public void Dispose()
   {
      if (_hasDelimiterBuffer)
      {
         _delimiters.Dispose();
      }
   }
}
