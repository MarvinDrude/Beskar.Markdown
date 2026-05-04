using Beskar.Markdown.Parsing.Interfaces;
using Beskar.Markdown.Parsing.Models;
using Me.Memory.Buffers;

namespace Beskar.Markdown.Parsing.Inlines;

public sealed class BoldParser : IInlineParser
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
      
      if (marker != TriggerChar && marker != TriggerAltChar) return false;

      var openRunLen = 0;
      while (openRunLen < text.Length && text[openRunLen] == marker)
      {
         openRunLen++;
      }
      
      var prevChar = state.GlobalOffset > 0 ? GetPreviousChar(ref state) : '\n';
      
      GetFlankingInfo(marker, prevChar, text, openRunLen, 
         out var canOpen, out _, out var isOpenLeft, out var isOpenRight);

      if (!canOpen) return false;

      var closeIndex = -1;
      var closeRunLen = 0;
      var consumedMarkers = 0;

      for (var i = openRunLen; i < text.Length; i++)
      {
         if (text[i] == '\\' && i + 1 < text.Length)
         {
            i++; // Skip the backslash
            continue;
         }
         
         if (text[i] != marker) continue;
         
         closeRunLen = 0;
         while (i + closeRunLen < text.Length && text[i + closeRunLen] == marker) closeRunLen++;

         var prevCloseChar = text[i - 1];
         GetFlankingInfo(marker, prevCloseChar, text[i..], closeRunLen, 
            out _, out var canClose, out var isCloseLeft, out var isCloseRight);

         if (canClose)
         {
            var violatesModulo3 = false;
            if ((isOpenLeft || isOpenRight) && (isCloseLeft || isCloseRight))
            {
               if ((openRunLen + closeRunLen) % 3 == 0 && (openRunLen % 3 != 0 || closeRunLen % 3 != 0))
               {
                  violatesModulo3 = true;
               }
            }

            if (!violatesModulo3)
            {
               closeIndex = i;
                  
               consumedMarkers = Math.Min(openRunLen, closeRunLen);
               if (consumedMarkers > 3) consumedMarkers = 3; 
               break;
            }
         }
            
         i += closeRunLen - 1;
      }

      if (closeIndex == -1) return false;

      var contentStart = state.GlobalOffset + openRunLen;
      var contentLength = closeIndex - openRunLen;
      var nodeIndex = writer.WrittenSpan.Length;

      if (consumedMarkers == 3)
      {
         writer.Add(new MarkdownNode()
         {
            Type = NodeType.StrongEmphasis, 
            TextSpan = new TextSpan(contentStart, contentLength),
            FirstChildIndex = nodeIndex + 1,
            NextSiblingIndex = -1
         });
         
         writer.Add(new MarkdownNode()
         {
            Type = NodeType.Emphasis, 
            TextSpan = new TextSpan(contentStart, contentLength),
            FirstChildIndex = -1,
            NextSiblingIndex = -1
         });
      }
      else
      {
         writer.Add(new MarkdownNode()
         {
            Type = consumedMarkers == 1 ? NodeType.Emphasis : NodeType.StrongEmphasis, 
            TextSpan = new TextSpan(contentStart, contentLength),
            FirstChildIndex = -1,
            NextSiblingIndex = -1
         });
      }

      parser.LinkInlineNode(ref writer, parentIndex, nodeIndex);
      state.Advance(closeIndex + closeRunLen);
      
      return true;
   }
   
   private static void GetFlankingInfo(char marker, char prevChar, ReadOnlySpan<char> text, int runLen,
      out bool canOpen, out bool canClose, out bool isLeftFlanking, out bool isRightFlanking)
   {
      var nextChar = runLen < text.Length ? text[runLen] : '\n';

      var isPrevSpace = char.IsWhiteSpace(prevChar) || prevChar == '\n';
      var isNextSpace = char.IsWhiteSpace(nextChar) || nextChar == '\n';

      var isPrevPunct = char.IsPunctuation(prevChar) || char.IsSymbol(prevChar);
      var isNextPunct = char.IsPunctuation(nextChar) || char.IsSymbol(nextChar);

      isLeftFlanking = !isNextSpace && (!isNextPunct || isPrevSpace || isPrevPunct);
      isRightFlanking = !isPrevSpace && (!isPrevPunct || isNextSpace || isNextPunct);

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

   private static char GetPreviousChar(ref InlineState state)
   {
      return state.RawText[state.GlobalOffset - 1]; 
   }
}