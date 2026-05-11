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
      var text = state.RemainingText;
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
      
      if (currentIndex < text.Length && (text[currentIndex] == '"' || text[currentIndex] == '\'' || text[currentIndex] == '('))
      {
         var openQuote = text[currentIndex++];
         var closeQuote = openQuote == '(' ? ')' : openQuote;
         var titleStartIndex = currentIndex;
         
         while (currentIndex < text.Length && text[currentIndex] != closeQuote) 
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
      parser.AddInlineNode(ref writer, nodeIndex, NodeType.Text, state.GlobalOffset + 2, closeBracketIndex - 2);

      state.Advance(nextIndex);
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
