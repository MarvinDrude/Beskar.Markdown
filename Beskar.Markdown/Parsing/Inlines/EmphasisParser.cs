using Beskar.Markdown.Parsing.Interfaces;
using Beskar.Markdown.Parsing.Models;
using Me.Memory.Buffers;

namespace Beskar.Markdown.Parsing.Inlines;

public sealed class EmphasisParser : IInlineParser
{
   public int Priority => 10_000;
   public int SupportedTypeValue => (int)NodeType.Emphasis;
   
   public char TriggerChar => '*';
   public char TriggerAltChar => '_';

   public bool TryMatch(ref InlineState state, int parentIndex, 
      ref BufferWriter<MarkdownNode> writer, ref InlineParser parser)
   {
      var text = state.RemainingText;
      var marker = text[0];
      
      if (marker != TriggerChar && marker != TriggerAltChar) 
         return false;

      var length = 0;
      while (length < text.Length && text[length] == marker) length++;

      GetFlankingInfo(marker, state, length, out var canOpen, out var canClose);

      if (!canOpen && !canClose) return false;
      
      var nodeIndex = writer.WrittenSpan.Length;
      writer.Add(new MarkdownNode()
      {
         Type = NodeType.Text,
         TextSpan = new TextSpan(state.GlobalOffset, length)
      });

      parser.AddDelimiter(new Delimiter()
      {
         Marker = marker,
         NodeIndex = nodeIndex,
         Length = length,
         CanOpen = canOpen,
         CanClose = canClose,
         Active = true
      });

      state.Advance(length);
      return true;
   }
   
   private static void GetFlankingInfo(char marker, in InlineState state, int length, 
      out bool canOpen, out bool canClose)
   {
      var text = state.RemainingText;
      var nextChar = '\n';
      
      if (length < text.Length)
      {
         nextChar = text[length];
      }

      var prevChar = '\n';
      if (state.GlobalOffset > 0)
      {
         prevChar = state.RawText[state.GlobalOffset - 1]; 
      }

      var isPrevSpace = char.IsWhiteSpace(prevChar);
      var isNextSpace = char.IsWhiteSpace(nextChar);

      var isPrevPunct = char.IsPunctuation(prevChar) || char.IsSymbol(prevChar);
      var isNextPunct = char.IsPunctuation(nextChar) || char.IsSymbol(nextChar);

      var isLeftFlanking = !isNextSpace && (!isNextPunct || isPrevSpace || isPrevPunct);
      var isRightFlanking = !isPrevSpace && (!isPrevPunct || isNextSpace || isNextPunct);

      if (marker == '*')
      {
         canOpen = isLeftFlanking;
         canClose = isRightFlanking;
      }
      else
      {
         canOpen = isLeftFlanking && (!isRightFlanking || isPrevPunct);
         canClose = isRightFlanking && (!isLeftFlanking || isNextPunct);
      }
   }
}