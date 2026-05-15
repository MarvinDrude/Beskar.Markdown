using Beskar.Markdown.Parsing.Interfaces;
using Beskar.Markdown.Parsing.Models;
using Me.Memory.Buffers;

namespace Beskar.Markdown.Parsing.Inlines;

public sealed class InlineCodeParser : IInlineParser
{
   public int Priority => 20_000;
   public int SupportedTypeValue => (int)NodeType.InlineCode;
   
   public char TriggerChar => '`';
   public char TriggerAltChar => '`';
   
   public bool TryMatch<TData>(ref InlineState<TData> state, int parentIndex, 
      ref BufferWriter<MarkdownNode> writer, scoped ref InlineParser<TData> parser,
      ParserOptions options)
   {
      var text = state.RemainingText;

      var markerLength = 0;
      while (markerLength < text.Length && text[markerLength] == '`')
      {
         markerLength++;
      }

      if (markerLength == 0)
      {
         return false;
      }
      
      var rawText = state.RawText;
      var rawBase = state.GlobalOffset;

      var closeIndex = -1;
      var scanIndex = markerLength;

      while (rawBase + scanIndex < rawText.Length)
      {
         var c = rawText[rawBase + scanIndex];

         if (c is '\n' or '\r')
         {
            var afterNewline = scanIndex + 1;
            
            if (c == '\r' && rawBase + afterNewline < rawText.Length 
                  && rawText[rawBase + afterNewline] == '\n')
               afterNewline++;

            var lineCheck = rawBase + afterNewline;
            var lineIsBlank = true;
            
            while (lineCheck < rawText.Length 
                   && rawText[lineCheck] != '\n' 
                   && rawText[lineCheck] != '\r')
            {
               if (rawText[lineCheck] != ' ' 
                   && rawText[lineCheck] != '\t')
               {
                  lineIsBlank = false;
                  break;
               }
               
               lineCheck++;
            }

            if (lineIsBlank)
               break;

            scanIndex++;
            continue;
         }

         if (c == '`')
         {
            var streak = 0;
            while (rawBase + scanIndex + streak < rawText.Length 
                   && rawText[rawBase + scanIndex + streak] == '`')
            {
               streak++;
            }

            if (streak == markerLength)
            {
               closeIndex = scanIndex;
               break;
            }

            scanIndex += streak;
         }
         else
         {
            scanIndex++;
         }
      }
      
      if (closeIndex == -1)
      {
         var literalIndex = writer.WrittenSpan.Length;
         writer.Add(new MarkdownNode()
         {
            Type = NodeType.Text,
            TextSpan = new TextSpan(state.GlobalOffset, markerLength),
            FirstChildIndex = -1,
            NextSiblingIndex = -1
         });

         parser.LinkInlineNode(ref writer, parentIndex, literalIndex);
         
         state.Advance(markerLength);
         return true;
      }
      
      var contentStart = state.GlobalOffset + markerLength;
      var contentLength = closeIndex - markerLength;

      if (contentLength >= 2 && IsSpaceOrLineEnding(rawText[rawBase + markerLength]) && IsSpaceOrLineEnding(rawText[rawBase + closeIndex - 1]))
      {
         var allSpaces = true;
         for (var k = rawBase + markerLength; k < rawBase + closeIndex; k++)
         {
            if (!IsSpaceOrLineEnding(rawText[k])) { allSpaces = false; break; }
         }

         if (!allSpaces)
         {
            contentStart++;
            contentLength -= 2;
         }
      }

      var nodeIndex = writer.WrittenSpan.Length;
      writer.Add(new MarkdownNode()
      {
         Type = NodeType.InlineCode,
         TextSpan = new TextSpan(contentStart, contentLength),
         InlineCodeIsInsideTable = (byte)(writer.WrittenSpan[parentIndex].Type == NodeType.TableCell ? 1 : 0),
         FirstChildIndex = -1,
         NextSiblingIndex = -1
      });

      parser.LinkInlineNode(ref writer, parentIndex, nodeIndex);

      state.Advance(closeIndex + markerLength);
      return true;
   }

   private static bool IsSpaceOrLineEnding(char c) => c is ' ' or '\n' or '\r';
}