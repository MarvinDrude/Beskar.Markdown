using Beskar.Markdown.Parsing.Interfaces;
using Beskar.Markdown.Parsing.Models;
using Me.Memory.Buffers;

namespace Beskar.Markdown.Parsing.Inlines;

public sealed class LinkParser : IInlineParser
{
   public int Priority => 10_000;
   public int SupportedTypeValue => (int)NodeType.Link;
   
   public char TriggerChar => '[';
   
   public bool TryMatch(ref InlineState state, int parentIndex, ref BufferWriter<MarkdownNode> writer, ref InlineParser parser)
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
         while (currentIndex < text.Length 
                && !char.IsWhiteSpace(text[currentIndex]) && text[currentIndex] != ')') 
         {
            currentIndex++;
         }
      }
      
      var actualUrlLength = currentIndex - actualUrlStart;
      if (isAngleBracketUrl) actualUrlLength--;

      while (currentIndex < text.Length && char.IsWhiteSpace(text[currentIndex])) 
      {
         currentIndex++;
      }
      
      // add title parsing here
      if (currentIndex < text.Length && (text[currentIndex] == '"' || text[currentIndex] == '\''))
      {
         var quote = text[currentIndex++];
         while (currentIndex < text.Length && text[currentIndex] != quote) currentIndex++;
         if (currentIndex < text.Length) currentIndex++;
         
         while (currentIndex < text.Length && char.IsWhiteSpace(text[currentIndex])) currentIndex++;
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
         LinkUrlLength = actualUrlLength
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
         if (text[i] == '\\') 
         {
            i++; // Skip escaped character
            continue;
         }
         
         if (text[i] == '[') 
         {
            depth++;
         }
         else if (text[i] == ']')
         {
            depth--;
            if (depth == 0) return i;
         }
      }
      
      return -1;
   }
}