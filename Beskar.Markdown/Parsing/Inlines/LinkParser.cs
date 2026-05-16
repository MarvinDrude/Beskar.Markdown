using Beskar.Markdown.Parsing.Interfaces;
using Beskar.Markdown.Parsing.Models;
using Beskar.Markdown.Parsing.Utils;
using Me.Memory.Buffers;

namespace Beskar.Markdown.Parsing.Inlines;

public sealed class LinkParser : IInlineParser
{
   public int Priority => 9_000;
   public int SupportedTypeValue => (int)NodeType.Link;
   
   public char TriggerChar => '[';
   public char TriggerAltChar => '[';
   
   public bool TryMatch<TData>(ref InlineState<TData> state, int parentIndex, 
      ref BufferWriter<MarkdownNode> writer, scoped ref InlineParser<TData> parser,
      ParserOptions options)
   {
      var text = state.RemainingText;

      var closeBracketIndex = FindClosingBracket(text);
      if (closeBracketIndex == -1) return false;
      
      if (closeBracketIndex + 1 < text.Length && text[closeBracketIndex + 1] == '(')
      {
         return ParseInlineLink(ref state, parentIndex, ref writer, 
            ref parser, options, text, closeBracketIndex);
      }
      
      return ParseReferenceLink(ref state, parentIndex, ref writer, 
         ref parser, options, text, closeBracketIndex);
   }

   private bool ParseInlineLink<TData>(ref InlineState<TData> state, int parentIndex, 
      ref BufferWriter<MarkdownNode> writer, scoped ref InlineParser<TData> parser,
      ParserOptions options, ReadOnlySpan<char> text, int closeBracketIndex)
   {
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
         
         while (currentIndex < text.Length)
         {
            if (text[currentIndex] == '\\' && currentIndex + 1 < text.Length 
                  && LinkUtils.IsAsciiPunctuation(text[currentIndex + 1]))
            {
               currentIndex += 2;
               continue;
            }
            
            if (text[currentIndex] == '>') 
               break;
               
            currentIndex++;
         }
         
         if (currentIndex >= text.Length) return false;
         currentIndex++;
      }
      else
      {
         var parenDepth = 0;
         while (currentIndex < text.Length)
         {
            var c = text[currentIndex];
            
            if (c == '\\' && currentIndex + 1 < text.Length 
                  && LinkUtils.IsAsciiPunctuation(text[currentIndex + 1]))
            {
               currentIndex += 2;
               continue;
            }
            
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
      if (currentIndex < text.Length && (text[currentIndex] == '"' || text[currentIndex] == '\'' || text[currentIndex] == '('))
      {
         var openQuote = text[currentIndex++];
         var closeQuote = openQuote == '(' ? ')' : openQuote;
         var titleStartIndex = currentIndex;
         
         while (currentIndex < text.Length)
         {
            if (text[currentIndex] == '\\' && currentIndex + 1 < text.Length 
                  && LinkUtils.IsAsciiPunctuation(text[currentIndex + 1]))
            {
               currentIndex += 2;
               continue;
            }
            
            if (text[currentIndex] == closeQuote) 
               break;
               
            currentIndex++;
         }
         
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
      parser.ParseInnerContent(state.Context, ref writer, nodeIndex, contentStart, contentLength, options);

      state.Advance(currentIndex + 1);
      return true;
   }

   private bool ParseReferenceLink<TData>(ref InlineState<TData> state, int parentIndex, 
      ref BufferWriter<MarkdownNode> writer, scoped ref InlineParser<TData> parser,
      ParserOptions options, ReadOnlySpan<char> text, int closeBracketIndex)
   {
      ReadOnlySpan<char> label;
      int nextIndex;

      // [text][label] or [text][]
      if (closeBracketIndex + 1 < text.Length && text[closeBracketIndex + 1] == '[')
      {
         var labelStart = closeBracketIndex + 1;
         var labelEnd = FindClosingBracket(text[labelStart..]);
         
         if (labelEnd == -1) 
            return false;
         
         labelEnd += labelStart;
         var labelContent = text.Slice(labelStart + 1, labelEnd - labelStart - 1);
         
         label = labelContent.IsEmpty
            ? text[1..closeBracketIndex] 
            : labelContent;
         
         nextIndex = labelEnd + 1;
      }
      else
      {
         label = text[1..closeBracketIndex];
         nextIndex = closeBracketIndex + 1;
      }

      if (LinkUtils.TryResolveReference(state.Context, label, out var lrdNodeIndex))
      {
         var lrdNode = writer.WrittenSpan[lrdNodeIndex];
         
         var nodeIndex = writer.WrittenSpan.Length;
         writer.Add(new MarkdownNode()
         {
            Type = NodeType.Link,
            TextSpan = new TextSpan(state.GlobalOffset, nextIndex),
            FirstChildIndex = -1,
            NextSiblingIndex = -1,
            LinkUrlStart = lrdNode.TextSpan.Start,
            LinkUrlLength = lrdNode.TextSpan.Length,
            LinkTitleOffset = lrdNode.TitleSpanStart != -1 ? (short)(lrdNode.TitleSpanStart - (lrdNode.TextSpan.Start + lrdNode.TextSpan.Length)) : (short)-1,
            LinkTitleLength = (ushort)lrdNode.TitleSpanLength
         });

         parser.LinkInlineNode(ref writer, parentIndex, nodeIndex);
         parser.ParseInnerContent(state.Context, ref writer, nodeIndex, state.GlobalOffset + 1, closeBracketIndex - 1, options);

         state.Advance(nextIndex);
         return true;
      }

      return false;
   }

   private static int FindClosingBracket(ReadOnlySpan<char> text)
   {
      var depth = 0;
      for (var i = 0; i < text.Length; i++)
      {
         switch (text[i])
         {
            case '\\':
               if (i + 1 < text.Length && LinkUtils.IsAsciiPunctuation(text[i + 1])) i++;
               continue;
            case '`':
            {
               var markerLen = 1;
               while (i + markerLen < text.Length && text[i + markerLen] == '`') markerLen++;
               
               var closeIdx = -1;
               for (var j = i + markerLen; j < text.Length; j++)
               {
                  if (text[j] == '`')
                  {
                     var endMarkerLen = 0;
                     
                     while (j + endMarkerLen < text.Length && text[j + endMarkerLen] == '`') 
                        endMarkerLen++;
                     
                     if (endMarkerLen == markerLen)
                     {
                        closeIdx = j;
                        break;
                     }
                     
                     j += endMarkerLen - 1;
                  }
               }
               
               if (closeIdx != -1)
               {
                  i = closeIdx + markerLen - 1;
               }
               else
               {
                  i += markerLen - 1;
               }
               break;
            }
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
