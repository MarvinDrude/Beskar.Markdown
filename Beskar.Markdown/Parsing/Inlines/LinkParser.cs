using Beskar.Markdown.Parsing.Interfaces;
using Beskar.Markdown.Parsing.Models;
using Me.Memory.Buffers;

namespace Beskar.Markdown.Parsing.Inlines;

public sealed class LinkParser : IInlineParser
{
   public int Priority => 9_000;
   public int SupportedTypeValue => (int)NodeType.Link;
   
   public char TriggerChar => '[';
   public char TriggerAltChar => '[';
   
   public bool TryMatch(ref InlineState state, int parentIndex, 
      ref BufferWriter<MarkdownNode> writer, scoped ref InlineParser parser,
      ParserOptions options)
   {
      var text = state.RemainingText;

      var closeBracketIndex = FindClosingBracket(text);
      if (closeBracketIndex == -1) return false;
      
      // No space between ]( allowed
      if (closeBracketIndex + 1 >= text.Length 
          || text[closeBracketIndex + 1] != '(')
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
         
         while (currentIndex < text.Length && text[currentIndex] != '>') 
            currentIndex++;
         
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
      {
         currentIndex++;
      }
      
      short titleOffset = -1;
      ushort titleLength = 0;
      
      // try get the optional title
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
         
         while (currentIndex < text.Length && char.IsWhiteSpace(text[currentIndex])) 
            currentIndex++;
      }
      
      if (currentIndex >= text.Length || text[currentIndex] != ')')
      {
         return false; // No closing parenthesis found
      }
      
      var contentStart = state.GlobalOffset + 1;
      var contentLength = closeBracketIndex - 1;

      var nodeIndex = writer.WrittenSpan.Length;
      writer.Add(new MarkdownNode()
      {
         Type = NodeType.Link,
         TextSpan = new TextSpan(state.GlobalOffset, currentIndex + 1),
         FirstChildIndex = -1,
         NextSiblingIndex = -1,
         LinkUrlStart = state.GlobalOffset + actualUrlStart,
         LinkUrlLength = actualUrlLength,
         LinkTitleOffset = titleOffset,
         LinkTitleLength = titleLength
      });

      parser.LinkInlineNode(ref writer, parentIndex, nodeIndex);
      
      // name of link can have inline formatting itself
      parser.ParseInnerContent(ref writer, nodeIndex, contentStart, contentLength, options);

      state.Advance(currentIndex + 1);
      return true;
   }
   
   private static int FindClosingBracket(ReadOnlySpan<char> text)
   {
      var depth = 0;
      for (var i = 0; i < text.Length; i++)
      {
         switch (text[i])
         {
            case '\\':
               i++; // Skip escaped character
               continue;
            case '[':
               depth++;
               break;
            case ']':
            {
               depth--;
               if (depth == 0) return i;
               break;
            }
         }
      }
      
      return -1;
   }
}
