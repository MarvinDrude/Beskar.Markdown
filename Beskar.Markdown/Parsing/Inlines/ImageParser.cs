using Beskar.Markdown.Parsing.Interfaces;
using Beskar.Markdown.Parsing.Models;
using Me.Memory.Buffers;

namespace Beskar.Markdown.Parsing.Inlines;

public sealed class ImageParser : IInlineParser
{
   public int Priority => 10_000;
   public int SupportedTypeValue => (int)NodeType.Image;
   
   public char TriggerChar => '!';
   public char TriggerAltChar => '!';
   
   public bool TryMatch(ref InlineState state, int parentIndex, ref BufferWriter<MarkdownNode> writer, scoped ref InlineParser parser)
   {
      var text = state.RemainingText;
      if (text.Length < 2 || text[1] != '[')
      {
         return false;
      }

      var bracketSpan = text[1..];
      var closeBracketIndex = FindClosingBracket(bracketSpan);
      
      if (closeBracketIndex == -1) return false;
      
      closeBracketIndex += 1;
      if (closeBracketIndex + 1 >= text.Length || text[closeBracketIndex + 1] != '(')
      {
         return false;
      }
      
      var urlStartIdx = closeBracketIndex + 2;
      var currentIndex = urlStartIdx;
      
      while (currentIndex < text.Length && char.IsWhiteSpace(text[currentIndex])) 
      {
         currentIndex++;
      }
      
      var actualUrlStart = currentIndex;
      var isAngleBracketUrl = currentIndex < text.Length && text[currentIndex] == '<';
      
      if (isAngleBracketUrl)
      {
         actualUrlStart++;
         currentIndex++;
         while (currentIndex < text.Length && text[currentIndex] != '>') currentIndex++;
         if (currentIndex >= text.Length) return false;
         currentIndex++;
      }
      else
      {
         var parenDepth = 0;
         while (currentIndex < text.Length)
         {
            var c = text[currentIndex];
            if (c == '(') parenDepth++;
            else if (c == ')') { if (parenDepth == 0) break; parenDepth--; }
            else if (char.IsWhiteSpace(c)) break;
            currentIndex++;
         }
      }
      
      var actualUrlLength = currentIndex - actualUrlStart;
      if (isAngleBracketUrl) actualUrlLength--;

      while (currentIndex < text.Length && char.IsWhiteSpace(text[currentIndex])) 
         currentIndex++;
      
      short titleOffset = -1;
      ushort titleLength = 0;
      
      if (currentIndex < text.Length && (text[currentIndex] == '"' || text[currentIndex] == '\''))
      {
         var quote = text[currentIndex++];
         var titleStartIndex = currentIndex;
         
         while (currentIndex < text.Length && text[currentIndex] != quote) 
            currentIndex++;
         
         if (currentIndex < text.Length) 
         {
            titleLength = (ushort)(currentIndex - titleStartIndex);
            var urlEndIndex = actualUrlStart + actualUrlLength;
            titleOffset = (short)(titleStartIndex - urlEndIndex);
            
            currentIndex++; // Skip the closing quote
         }
         
         while (currentIndex < text.Length 
            && char.IsWhiteSpace(text[currentIndex])) 
            currentIndex++;
      }
      
      if (currentIndex >= text.Length || text[currentIndex] != ')') 
         return false;

      var contentStart = state.GlobalOffset + 2; // Skip "!["
      var contentLength = closeBracketIndex - 2;

      var nodeIndex = writer.WrittenSpan.Length;
      writer.Add(new MarkdownNode()
      {
         Type = NodeType.Image,
         TextSpan = new TextSpan(state.GlobalOffset, currentIndex + 1),
         FirstChildIndex = -1,
         NextSiblingIndex = -1,
         LastChildIndex = -1,
         LinkUrlStart = state.GlobalOffset + actualUrlStart,
         LinkUrlLength = actualUrlLength,
         LinkTitleOffset = titleOffset,
         LinkTitleLength = titleLength
      });

      parser.LinkInlineNode(ref writer, parentIndex, nodeIndex);
      parser.AddInlineNode(ref writer, nodeIndex, NodeType.Text, contentStart, contentLength);
      
      state.Advance(currentIndex + 1);
      return true;
   }
   
   private static int FindClosingBracket(ReadOnlySpan<char> text)
   {
      var depth = 0;
      for (var i = 0; i < text.Length; i++)
      {
         if (text[i] == '\\') { i++; continue; }
         
         if (text[i] == '[') depth++;
         else if (text[i] == ']')
         {
            depth--;
            if (depth == 0) return i;
         }
      }
      return -1;
   }
}
