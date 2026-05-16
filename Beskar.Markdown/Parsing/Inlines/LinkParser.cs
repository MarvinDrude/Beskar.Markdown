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
      var text = state.RawText[state.GlobalOffset..state.BlockEnd];

      var closeBracketIndex = FindClosingBracket(text);
      if (closeBracketIndex == -1) return false;

      if (ContainsInlineLink(state.Context, text[1..closeBracketIndex]))
      {
         return false;
      }
      
      if (closeBracketIndex + 1 < text.Length && text[closeBracketIndex + 1] == '(')
      {
         if (ParseInlineLink(ref state, parentIndex, ref writer, 
                ref parser, options, text, closeBracketIndex))
         {
            return true;
         }
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
      
      while (currentIndex < text.Length 
         && IsLinkWhitespace(text[currentIndex])) 
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
            
            if (text[currentIndex] is '\n' or '\r')
               return false;

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
            else if (IsLinkWhitespace(c)) break;
            
            currentIndex++;
         }
      }
      
      var actualUrlLength = currentIndex - actualUrlStart;
      if (isAngleBracketUrl) actualUrlLength--;

      while (currentIndex < text.Length && IsLinkWhitespace(text[currentIndex])) 
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
         
         while (currentIndex < text.Length && IsLinkWhitespace(text[currentIndex])) 
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

   private static bool ContainsInlineLink<TData>(
      MarkdownContext<TData> context,
      ReadOnlySpan<char> text)
   {
      for (var i = 0; i < text.Length; i++)
      {
         switch (text[i])
         {
            case '\\':
               if (i + 1 < text.Length && LinkUtils.IsAsciiPunctuation(text[i + 1])) i++;
               break;
            case '`':
               i = SkipCodeSpan(text, i);
               break;
            case '<':
               if (TrySkipAngleInline(text[i..], out var angleLength))
               {
                  i += angleLength - 1;
               }
               break;
            case '[':
               if (i > 0 && text[i - 1] == '!')
               {
                  break;
               }

               if (WouldParseAsLink(context, text[i..]))
               {
                  return true;
               }
               break;
         }
      }

      return false;
   }

   private static bool WouldParseAsLink<TData>(
      MarkdownContext<TData> context,
      ReadOnlySpan<char> text)
   {
      var closeBracketIndex = FindClosingBracket(text);
      if (closeBracketIndex == -1) return false;

      if (ContainsInlineLink(context, text[1..closeBracketIndex]))
      {
         return true;
      }

      if (closeBracketIndex + 1 < text.Length && text[closeBracketIndex + 1] == '(')
      {
         return TryScanInlineLink(text, closeBracketIndex, out _);
      }

      return TryScanReferenceLabel(text, closeBracketIndex, out var label)
         && LinkUtils.TryResolveReference(context, label, out _);
   }

   private static bool TryScanInlineLink(
      ReadOnlySpan<char> text,
      int closeBracketIndex,
      out int endIndex)
   {
      endIndex = -1;
      var currentIndex = closeBracketIndex + 2;

      while (currentIndex < text.Length && IsLinkWhitespace(text[currentIndex]))
      {
         currentIndex++;
      }

      var isAngleBracketUrl = currentIndex < text.Length && text[currentIndex] == '<';

      if (isAngleBracketUrl)
      {
         currentIndex++;

         while (currentIndex < text.Length)
         {
            if (text[currentIndex] == '\\'
                && currentIndex + 1 < text.Length
                && LinkUtils.IsAsciiPunctuation(text[currentIndex + 1]))
            {
               currentIndex += 2;
               continue;
            }

            if (text[currentIndex] is '\n' or '\r')
               return false;

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
         var urlStart = currentIndex;

         while (currentIndex < text.Length)
         {
            var c = text[currentIndex];

            if (c == '\\'
                && currentIndex + 1 < text.Length
                && LinkUtils.IsAsciiPunctuation(text[currentIndex + 1]))
            {
               currentIndex += 2;
               continue;
            }

            if (c == '(') parenDepth++;
            else if (c == ')')
            {
               if (parenDepth == 0) break;
               parenDepth--;
            }
            else if (IsLinkWhitespace(c)) break;

            currentIndex++;
         }

         if (currentIndex == urlStart && (currentIndex >= text.Length || text[currentIndex] != ')'))
            return false;
      }

      while (currentIndex < text.Length && IsLinkWhitespace(text[currentIndex]))
      {
         currentIndex++;
      }

      if (currentIndex < text.Length
          && (text[currentIndex] == '"' || text[currentIndex] == '\'' || text[currentIndex] == '('))
      {
         var openQuote = text[currentIndex++];
         var closeQuote = openQuote == '(' ? ')' : openQuote;

         while (currentIndex < text.Length)
         {
            if (text[currentIndex] == '\\'
                && currentIndex + 1 < text.Length
                && LinkUtils.IsAsciiPunctuation(text[currentIndex + 1]))
            {
               currentIndex += 2;
               continue;
            }

            if (text[currentIndex] == closeQuote)
               break;

            currentIndex++;
         }

         if (currentIndex >= text.Length) return false;
         currentIndex++;

         while (currentIndex < text.Length && IsLinkWhitespace(text[currentIndex]))
         {
            currentIndex++;
         }
      }

      if (currentIndex >= text.Length || text[currentIndex] != ')')
         return false;

      endIndex = currentIndex + 1;
      return true;
   }

   private static bool TryScanReferenceLabel(
      ReadOnlySpan<char> text,
      int closeBracketIndex,
      out ReadOnlySpan<char> label)
   {
      label = default;

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

         return true;
      }

      label = text[1..closeBracketIndex];
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
               if (i + 1 < text.Length && LinkUtils.IsAsciiPunctuation(text[i + 1])) i++;
               continue;
            case '`':
               i = SkipCodeSpan(text, i);
               break;
            case '<':
               if (TrySkipAngleInline(text[i..], out var angleLength))
               {
                  i += angleLength - 1;
               }
               break;
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

   private static int SkipCodeSpan(ReadOnlySpan<char> text, int start)
   {
      var markerLen = 1;
      while (start + markerLen < text.Length && text[start + markerLen] == '`') markerLen++;

      for (var j = start + markerLen; j < text.Length; j++)
      {
         if (text[j] != '`') continue;

         var endMarkerLen = 0;
         while (j + endMarkerLen < text.Length && text[j + endMarkerLen] == '`')
            endMarkerLen++;

         if (endMarkerLen == markerLen)
         {
            return j + markerLen - 1;
         }

         j += endMarkerLen - 1;
      }

      return start + markerLen - 1;
   }

   private static bool TrySkipAngleInline(ReadOnlySpan<char> text, out int length)
   {
      length = 0;
      if (text.Length < 3 || text[0] != '<')
      {
         return false;
      }

      if (HtmlTagUtils.TryParseOpenTag(text, out length)
          || HtmlTagUtils.TryParseClosingTag(text, out length)
          || TryScanAutolink(text, out length))
      {
         return true;
      }

      return false;
   }

   private static bool TryScanAutolink(ReadOnlySpan<char> text, out int length)
   {
      length = 0;
      var closeIdx = -1;

      for (var i = 1; i < text.Length; i++)
      {
         var c = text[i];
         if (c == '<' || IsLinkWhitespace(c))
         {
            return false;
         }

         if (c == '>')
         {
            closeIdx = i;
            break;
         }
      }

      if (closeIdx == -1) return false;

      var content = text[1..closeIdx];
      if (!IsUriAutolink(content) && !IsEmailAutolink(content))
      {
         return false;
      }

      length = closeIdx + 1;
      return true;
   }

   private static bool IsUriAutolink(ReadOnlySpan<char> content)
   {
      var colonIdx = content.IndexOf(':');

      if (colonIdx is < 2 or > 32) return false;
      if (!char.IsAsciiLetter(content[0])) return false;

      for (var i = 1; i < colonIdx; i++)
      {
         var c = content[i];
         if (!char.IsAsciiLetterOrDigit(c)
             && c != '+' && c != '-' && c != '.')
         {
            return false;
         }
      }

      return content.Length > colonIdx + 1;
   }

   private static bool IsEmailAutolink(ReadOnlySpan<char> content)
   {
      var atIdx = content.IndexOf('@');
      if (atIdx <= 0 || atIdx >= content.Length - 1) return false;

      var localPart = content[..atIdx];
      foreach (var c in localPart)
      {
         if (!char.IsAsciiLetterOrDigit(c) && !".!#$%&'*+/=?^_`{|}~-".Contains(c))
            return false;
      }

      var domainPart = content[(atIdx + 1)..];
      if (domainPart.Length < 3) return false;

      var lastDotIdx = -1;

      for (var i = 0; i < domainPart.Length; i++)
      {
         var c = domainPart[i];
         if (char.IsAsciiLetterOrDigit(c)) continue;

         if (c == '.')
         {
            if (i == 0 || i == domainPart.Length - 1 || domainPart[i - 1] == '.')
               return false;
            lastDotIdx = i;

            continue;
         }

         if (c != '-')
            return false;

         if (i == 0 || i == domainPart.Length - 1 || domainPart[i - 1] == '.')
            return false;
      }

      return lastDotIdx != -1;
   }

   private static bool IsLinkWhitespace(char c)
   {
      return c is ' ' or '\t' or '\n' or '\r';
   }
}
