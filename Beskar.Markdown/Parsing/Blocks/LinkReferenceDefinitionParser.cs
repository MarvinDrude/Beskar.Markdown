using Beskar.Markdown.Parsing.Models;
using Beskar.Markdown.Parsing.Interfaces;
using Beskar.Markdown.Extensions;
using Beskar.Markdown.Parsing.Utils;
using Me.Memory.Buffers;

namespace Beskar.Markdown.Parsing.Blocks;

public sealed class LinkReferenceDefinitionParser : IBlockParser
{
   public int Priority => 100;
   public int SupportedTypeValue => (int)NodeType.LinkReferenceDefinition;
   
   public int TryMatch<TData>(
      ref LineState<TData> state, 
      int parentIndex, 
      ref BufferWriter<MarkdownNode> writer)
   {
      if (state.LeadingSpaces >= 4 || state.FirstChar != '[')
         return -1;

      var line = state.RawLine;
      var i = state.FirstNonSpaceIndex;

      if (!TryParseLabel(line, i, out var labelEndIndex))
      {
         if (TryParseMultilineDefinition(ref state, i, out var multilineNode, out var multilineNormalizedLabel))
         {
            var multilineNodeIndex = writer.WrittenSpan.Length;
            writer.Add(multilineNode);
            
            state.Context.ReferenceDefinitions.TryAdd(multilineNormalizedLabel, multilineNodeIndex);
            state.ConsumeRest();
            
            return multilineNodeIndex;
         }

         return -1;
      }

      var colonIndex = labelEndIndex + 1;
      if (colonIndex < line.Length && line[colonIndex] == ':')
      {
         if (!TryParseMultilineDefinition(ref state, i, out var fullNode, out var fullNormalizedLabel))
         {
            return -1;
         }

         var fullNodeIndex = writer.WrittenSpan.Length;
         writer.Add(fullNode);
         
         state.Context.ReferenceDefinitions.TryAdd(fullNormalizedLabel, fullNodeIndex);
         state.ConsumeRest();
         
         return fullNodeIndex;
      }

      var labelStart = i + 1;
      var labelLength = labelEndIndex - labelStart;
      i = labelEndIndex + 1;

      if (i >= line.Length || line[i] != ':')
         return -1;
      i++;

      var nodeIndex = writer.WrittenSpan.Length;
      writer.Add(new MarkdownNode
      {
         Type = NodeType.LinkReferenceDefinition,
         TextSpan = default, // URL will be set later
         TitleSpanStart = -1,
         TitleSpanLength = 0,
         FirstChildIndex = -1,
         NextSiblingIndex = -1
      });

      var labelSpan = line.Slice(labelStart, labelLength);
      var normalizedLabel = LinkUtils.NormalizeLabel(labelSpan);
      
      state.Context.ReferenceDefinitions.TryAdd(normalizedLabel, nodeIndex);
      state.Slice(i);
      
      ContinueParsing(ref writer.GetReference(nodeIndex), ref state);
      return nodeIndex;
   }

   public bool CanContinue<TData>(ref MarkdownNode node, ref LineState<TData> state, 
      ref BufferWriter<MarkdownNode> writer)
   {
      if (node.LastChildIndex > 0)
      {
         var endOffset = node.LastChildIndex;
         var lineEnd = state.GlobalOffset + state.RawLine.Length;

         if (lineEnd < endOffset)
         {
            state.ConsumeRest();
            return true;
         }

         var consumeLength = Math.Max(0, Math.Min(state.RawLine.Length, endOffset - state.GlobalOffset));
         state.Slice(consumeLength);
         
         node.LastChildIndex = -1;
         
         return false;
      }

      if (state.IsBlank)
         return false;

      if (node.TitleSpanStart != -1 && node.TitleSpanLength >= 0)
         return false;

      return ContinueParsing(ref node, ref state);
   }

   private static bool TryParseMultilineDefinition<TData>(
      ref LineState<TData> state,
      int start,
      out MarkdownNode node,
      out string normalizedLabel)
   {
      node = default;
      normalizedLabel = string.Empty;

      var definitionStart = state.GlobalOffset + start;
      var full = state.FullText[definitionStart..];
      
      if (full.Length == 0 || full[0] != '[')
      {
         return false;
      }

      if (!TryParseMultilineLabel(full, out var labelEndIndex))
      {
         return false;
      }

      var i = labelEndIndex + 1;
      if (i >= full.Length || full[i] != ':')
      {
         return false;
      }

      i++;
      SkipSpacesAndTabs(full, ref i);

      if (i < full.Length && full[i] is '\r' or '\n')
      {
         SkipLineEndingAndIndent(full, ref i);
      }

      if (!TryParseDestination(full, i, out var urlEnd))
      {
         return false;
      }

      var urlStart = definitionStart + i;
      var urlLength = urlEnd - i;
      if (full[i] == '<')
      {
         urlStart++;
         urlLength -= 2;
      }

      i = urlEnd;
      var skippedTitleSeparator = SkipSpacesAndTabs(full, ref i);

      if (i < full.Length && full[i] is '\r' or '\n')
      {
         var titleLineStart = i;
         SkipLineEndingAndIndent(full, ref titleLineStart);

         if (titleLineStart < full.Length && full[titleLineStart] is '"' or '\'' or '(')
         {
            if (TryParseTitle(full, titleLineStart, out var continuationTitleEnd, out var continuationTitleClosed)
                && continuationTitleClosed
                && !ContainsBlankLine(full[titleLineStart..continuationTitleEnd]))
            {
               var afterTitle = continuationTitleEnd;
               SkipSpacesAndTabs(full, ref afterTitle);
               
               if (afterTitle >= full.Length || full[afterTitle] is '\r' or '\n')
               {
                  skippedTitleSeparator = true;
                  i = titleLineStart;
               }
            }
         }
      }

      var titleStart = -1;
      var titleLength = 0;

      if (skippedTitleSeparator && i < full.Length 
            && full[i] is '"' or '\'' or '(')
      {
         if (!TryParseTitle(full, i, out var titleEnd, out var titleClosed) || !titleClosed)
         {
            return false;
         }

         if (ContainsBlankLine(full[i..titleEnd]))
         {
            return false;
         }

         titleStart = definitionStart + i + 1;
         titleLength = titleEnd - i - 2;
         i = titleEnd;
         
         SkipSpacesAndTabs(full, ref i);
      }

      if (i < full.Length && full[i] is not ('\r' or '\n'))
      {
         return false;
      }

      var label = state.FullText.Slice(definitionStart + 1, labelEndIndex - 1);
      normalizedLabel = LinkUtils.NormalizeLabel(label);
      if (normalizedLabel.Length == 0)
      {
         return false;
      }

      node = new MarkdownNode
      {
         Type = NodeType.LinkReferenceDefinition,
         TextSpan = new TextSpan(urlStart, urlLength),
         TitleSpanStart = titleStart,
         TitleSpanLength = titleLength,
         FirstChildIndex = -1,
         LastChildIndex = definitionStart + i,
         NextSiblingIndex = -1
      };

      return true;
   }

   private static bool ContinueParsing<TData>(ref MarkdownNode node, ref LineState<TData> state)
   {
      var line = state.RawLine;
      var i = 0;

      if (node.TitleSpanStart != -1 
          && node.TitleSpanLength < 0)
      {
         var closeMarker = (char)(-node.TitleSpanLength);
         
         if (TryContinueTitle(line, i, closeMarker, out var titleEnd, out var isClosed))
         {
            if (isClosed)
            {
               node.TitleSpanLength = (state.GlobalOffset + titleEnd - 1) - node.TitleSpanStart;
               i = titleEnd;
               
               while (i < line.Length && (line[i] == ' ' || line[i] == '\t')) 
                  i++;
               
               if (i < line.Length)
               {
                  node.TitleSpanStart = -1;
                  node.TitleSpanLength = 0;
                  
                  return false;
               }
               
               state.Slice(i);
               return false;
            }

            state.ConsumeRest();
            return true;
         }
         
         return false;
      }

      i = state.FirstNonSpaceIndex;
      if (i == -1) i = line.Length;

      if (node.TextSpan is { Length: 0, Start: 0 })
      {
         if (i >= line.Length)
            return true;

         if (TryParseDestination(line, i, out var urlEnd))
         {
            var urlStart = state.GlobalOffset + i;
            var urlLen = urlEnd - i;
            
            if (line[i] == '<')
            {
               urlStart++;
               urlLen -= 2;
            }
            
            node.TextSpan = new TextSpan(urlStart, urlLen);
            i = urlEnd;
            
            while (i < line.Length && (line[i] == ' ' || line[i] == '\t')) 
               i++;
            
            if (i < line.Length)
            {
               if (TryParseTitle(line, i, out var tEnd, out var isClosed))
               {
                  node.TitleSpanStart = state.GlobalOffset + i + 1;
                  
                  if (isClosed)
                  {
                     node.TitleSpanLength = tEnd - i - 2;
                     i = tEnd;
                     
                     while (i < line.Length && (line[i] == ' ' || line[i] == '\t')) 
                        i++;
                     
                     if (i < line.Length)
                     {
                        node.TitleSpanStart = -1;
                        node.TitleSpanLength = 0;
                        state.Slice(urlEnd);
                        
                        return false; 
                     }
                     
                     state.Slice(i);
                     return false;
                  }

                  var openMarker = line[i];
                  var closeMarker = openMarker == '(' ? ')' : openMarker;
                  
                  node.TitleSpanLength = -closeMarker;
                  state.ConsumeRest();
                  
                  return true;
               }

               state.Slice(urlEnd);
               return false;
            }
            
            state.Slice(i);
            return true;
         }
         
         return false;
      }

      if (node.TextSpan.Length > 0 || node.TextSpan.Start > 0)
      {
         if (i >= line.Length) return true;

         if (TryParseTitle(line, i, out var tEnd, out var isClosed))
         {
            node.TitleSpanStart = state.GlobalOffset + i + 1;
            
            if (isClosed)
            {
               node.TitleSpanLength = tEnd - i - 2;
               i = tEnd;
               
               while (i < line.Length && (line[i] == ' ' || line[i] == '\t')) 
                  i++;
               
               if (i < line.Length)
               {
                  node.TitleSpanStart = -1;
                  node.TitleSpanLength = 0;
                  
                  return false;
               }
               
               state.Slice(i);
            }
            else
            {
               var openMarker = line[i];
               var closeMarker = openMarker == '(' ? ')' : openMarker;
               
               node.TitleSpanLength = -closeMarker;
               state.ConsumeRest();
               
               return true;
            }
         }
      }

      return false;
   }

   private static bool TryParseLabel(ReadOnlySpan<char> line, int start, out int endIndex)
   {
      endIndex = -1;
      if (start >= line.Length || line[start] != '[') 
         return false;
      
      var foundContent = false;
      
      for (var i = start + 1; i < line.Length; i++)
      {
         var c = line[i];
         
         if (c == '\\')
         {
            if (i + 1 < line.Length
                && LinkUtils.IsAsciiPunctuation(line[i + 1]))
            {
               i++; foundContent = true; 
               continue;
            }
         }
         
         if (c == '[') 
            return false;
         
         if (c == ']')
         {
            if (!foundContent) 
               return false;
            
            if (i - start > 1000) 
               return false;
            
            endIndex = i;
            return true;
         }
         
         if (!char.IsWhiteSpace(c)) 
            foundContent = true;
      }
      
      return false;
   }

   private static bool TryParseMultilineLabel(ReadOnlySpan<char> text, out int endIndex)
   {
      endIndex = -1;
      if (text.Length == 0 || text[0] != '[')
      {
         return false;
      }

      var foundContent = false;

      for (var i = 1; i < text.Length; i++)
      {
         var c = text[i];

         if (c == '\\')
         {
            if (i + 1 < text.Length
                && LinkUtils.IsAsciiPunctuation(text[i + 1]))
            {
               i++;
               foundContent = true;
               continue;
            }
         }

         if (c == '[')
            return false;

         if (c == ']')
         {
            if (!foundContent)
               return false;

            if (i > 1000)
               return false;

            endIndex = i;
            return true;
         }

         if (!char.IsWhiteSpace(c))
            foundContent = true;
      }

      return false;
   }

   private static bool SkipSpacesAndTabs(ReadOnlySpan<char> text, ref int index)
   {
      var start = index;
      while (index < text.Length && text[index] is ' ' or '\t')
      {
         index++;
      }

      return index > start;
   }

   private static void SkipLineEndingAndIndent(ReadOnlySpan<char> text, ref int index)
   {
      if (index < text.Length && text[index] == '\r')
      {
         index++;
         if (index < text.Length && text[index] == '\n')
         {
            index++;
         }
      }
      else if (index < text.Length && text[index] == '\n')
      {
         index++;
      }

      while (index < text.Length && text[index] is ' ' or '\t')
      {
         index++;
      }
   }

   private static bool ContainsBlankLine(ReadOnlySpan<char> text)
   {
      var lineHasContent = true;
      for (var i = 0; i < text.Length; i++)
      {
         var c = text[i];
         if (c is '\r' or '\n')
         {
            if (!lineHasContent)
            {
               return true;
            }

            lineHasContent = false;
            if (c == '\r' && i + 1 < text.Length 
               && text[i + 1] == '\n')
            {
               i++;
            }
         }
         else if (c is not (' ' or '\t'))
         {
            lineHasContent = true;
         }
      }

      return false;
   }

   private static bool TryParseDestination(ReadOnlySpan<char> line, int start, out int endIndex)
   {
      endIndex = -1;
      
      if (start >= line.Length) return false;
      
      if (line[start] == '<')
      {
         for (var e = start + 1; e < line.Length; e++)
         {
            var c = line[e];
            
            if (c == '\\' && e + 1 < line.Length 
               && LinkUtils.IsAsciiPunctuation(line[e + 1]))
            {
               e++; 
               continue;
            }
            
            if (c is '<' or '\n' or '\r') 
               return false;
            
            if (c == '>')
            {
               endIndex = e + 1; 
               return true;
            }
         }
         return false;
      }

      var parenDepth = 0;
      var i = start;
      
      while (i < line.Length)
      {
         var c = line[i];
         if (c == '\\' && i + 1 < line.Length
            && LinkUtils.IsAsciiPunctuation(line[i + 1]))
         {
            i += 2;
            continue;
         }
         
         if (char.IsWhiteSpace(c)) break;
         if (c == '(') parenDepth++;
         else if (c == ')')
         {
            if (parenDepth == 0) break; 
            parenDepth--;
         }
         
         i++;
      }
      
      if (i == start || parenDepth != 0) 
         return false;
      
      endIndex = i;
      return true;
   }

   private static bool TryParseTitle(ReadOnlySpan<char> line, int start, out int endIndex, out bool isClosed)
   {
      endIndex = -1;
      isClosed = false;
      
      if (start >= line.Length) 
         return false;
      
      var open = line[start];
      if (open != '"' && open != '\'' && open != '(') 
         return false;
      
      var close = open == '(' ? ')' : open;
      
      for (var i = start + 1; i < line.Length; i++)
      {
         var c = line[i];
         
         if (c == '\\' && i + 1 < line.Length
            && LinkUtils.IsAsciiPunctuation(line[i + 1]))
         {
            i++; 
            continue;
         }

         if (c == close)
         {
            endIndex = i + 1; 
            isClosed = true; 
            return true;
         }
      }
      
      endIndex = line.Length;
      isClosed = false;
      
      return true;
   }

   private static bool TryContinueTitle(ReadOnlySpan<char> line, int start, char closeMarker, out int endIndex, out bool isClosed)
   {
      endIndex = -1;
      isClosed = false;
      
      for (var i = start; i < line.Length; i++)
      {
         var c = line[i];

         if (c == '\\' && i + 1 < line.Length
            && LinkUtils.IsAsciiPunctuation(line[i + 1]))
         {
            i++; 
            continue;
         }

         if (c == closeMarker)
         {
            endIndex = i + 1; 
            isClosed = true; 
            
            return true;
         }
      }
      
      endIndex = line.Length;
      isClosed = false;
      
      return true;
   }
}
