using Beskar.Markdown.Parsing.Interfaces;
using Beskar.Markdown.Parsing.Models;
using Beskar.Markdown.Parsing.Utils;
using Me.Memory.Buffers;

namespace Beskar.Markdown.Parsing.Inlines;

public sealed class ImageParser : IInlineParser
{
   public int Priority => 10_000;
   public int SupportedTypeValue => (int)NodeType.Image;
   
   public char TriggerChar => '!';
   public char TriggerAltChar => '!';
   
   public bool TryMatch<TData>(ref InlineState<TData> state, int parentIndex, 
      ref BufferWriter<MarkdownNode> writer, scoped ref InlineParser<TData> parser,
      ParserOptions options)
   {
      var text = state.RawText[state.GlobalOffset..state.BlockEnd];
      
      if (text.Length < 2 || text[1] != '[')
      {
         return false;
      }

      var bracketSpan = text[1..];
      var closeBracketIndex = FindClosingBracket(bracketSpan);
      
      if (closeBracketIndex == -1) return false;
      
      closeBracketIndex += 1;
      // Check for inline link: ](
      if (closeBracketIndex + 1 < text.Length && text[closeBracketIndex + 1] == '(')
      {
         return ParseInlineImage(ref state, parentIndex, ref writer, 
            ref parser, options, text, closeBracketIndex);
      }
      
      // Check for reference link
      return ParseReferenceImage(ref state, parentIndex, ref writer, 
         ref parser, options, text, closeBracketIndex);
   }

   private bool ParseInlineImage<TData>(ref InlineState<TData> state, int parentIndex, 
      ref BufferWriter<MarkdownNode> writer, scoped ref InlineParser<TData> parser,
      ParserOptions options, ReadOnlySpan<char> text, int closeBracketIndex)
   {
      var urlStartIdx = closeBracketIndex + 2;
      var currentIndex = urlStartIdx;
      
      while (currentIndex < text.Length && IsLinkWhitespace(text[currentIndex])) 
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
            else if (c == ')') { if (parenDepth == 0) break; parenDepth--; }
            else if (IsLinkWhitespace(c)) break;
            
            currentIndex++;
         }
      }
      
      var actualUrlLength = currentIndex - actualUrlStart;
      if (isAngleBracketUrl) actualUrlLength--;

      while (currentIndex < text.Length && IsLinkWhitespace(text[currentIndex])) 
         currentIndex++;
      
      short titleOffset = -1;
      ushort titleLength = 0;
      
      if (currentIndex < text.Length && (text[currentIndex] == '"' || text[currentIndex] == '\'' || text[currentIndex] == '('))
      {
         var openQuote = text[currentIndex++];
         var closeQuote = openQuote == '(' ? ')' : openQuote;
         var titleStartIndex = currentIndex;
         
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
         
         if (currentIndex < text.Length) 
         {
            titleLength = (ushort)(currentIndex - titleStartIndex);
            var urlEndIndex = actualUrlStart + actualUrlLength;
            titleOffset = (short)(titleStartIndex - urlEndIndex);
            
            currentIndex++; // Skip the closing quote
         }
         
         while (currentIndex < text.Length 
            && IsLinkWhitespace(text[currentIndex])) 
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
      parser.ParseInnerContent(state.Context, ref writer, nodeIndex, contentStart, contentLength, options);
      
      state.Advance(currentIndex + 1);
      return true;
   }

   private bool ParseReferenceImage<TData>(ref InlineState<TData> state, int parentIndex, 
      ref BufferWriter<MarkdownNode> writer, scoped ref InlineParser<TData> parser,
      ParserOptions options, ReadOnlySpan<char> text, int closeBracketIndex)
   {
      ReadOnlySpan<char> label;
      int nextIndex;

      if (closeBracketIndex + 1 < text.Length && text[closeBracketIndex + 1] == '[')
      {
         var labelStart = closeBracketIndex + 1;
         var labelEnd = FindClosingBracket(text[labelStart..]);
         
         if (labelEnd == -1) return false;
         
         labelEnd += labelStart;
         var labelContent = text.Slice(
            labelStart + 1, labelEnd - labelStart - 1);
         
         if (labelContent.IsEmpty)
         {
            label = text[2..closeBracketIndex];
         }
         else
         {
            label = labelContent;
         }
         nextIndex = labelEnd + 1;
      }
      else
      {
         label = text[2..closeBracketIndex];
         nextIndex = closeBracketIndex + 1;
      }

      if (!LinkUtils.TryResolveReference(state.Context, label, out var lrdNodeIndex))
         return false;
      
      var lrdNode = writer.WrittenSpan[lrdNodeIndex];
      var nodeIndex = writer.WrittenSpan.Length;
      
      writer.Add(new MarkdownNode()
      {
         Type = NodeType.Image,
         TextSpan = new TextSpan(state.GlobalOffset, nextIndex),
         FirstChildIndex = -1,
         NextSiblingIndex = -1,
         LastChildIndex = -1,
         LinkUrlStart = lrdNode.TextSpan.Start,
         LinkUrlLength = lrdNode.TextSpan.Length,
         LinkTitleOffset = lrdNode.TitleSpanStart != -1 
            ? (short)(lrdNode.TitleSpanStart - (lrdNode.TextSpan.Start + lrdNode.TextSpan.Length)) 
            : (short)-1,
         LinkTitleLength = (ushort)lrdNode.TitleSpanLength
      });

      parser.LinkInlineNode(ref writer, parentIndex, nodeIndex);
      parser.ParseInnerContent(state.Context, ref writer, nodeIndex, state.GlobalOffset + 2, closeBracketIndex - 2, options);

      state.Advance(nextIndex);
      return true;
   }
   
   private static int FindClosingBracket(ReadOnlySpan<char> text)
   {
      var depth = 0;
      for (var i = 0; i < text.Length; i++)
      {
         if (text[i] == '\\') { i++; continue; }

         if (text[i] == '`')
         {
            i = SkipCodeSpan(text, i);
            continue;
         }

         if (text[i] == '<' 
             && TrySkipAngleInline(text[i..], out var angleLength))
         {
            i += angleLength - 1;
            continue;
         }
         
         if (text[i] == '[') depth++;
         else if (text[i] == ']')
         {
            depth--;
            if (depth == 0) return i;
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

      return HtmlTagUtils.TryParseOpenTag(text, out length)
             || HtmlTagUtils.TryParseClosingTag(text, out length)
             || TryScanAutolink(text, out length);
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
