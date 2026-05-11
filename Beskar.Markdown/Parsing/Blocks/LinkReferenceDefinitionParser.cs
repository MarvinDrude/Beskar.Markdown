using Beskar.Markdown.Parsing.Models;
using Beskar.Markdown.Parsing.Interfaces;
using Beskar.Markdown.Extensions;
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
         return -1;

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
      var normalizedLabel = NormalizeLabel(labelSpan);
      
      state.Context.ReferenceDefinitions.TryAdd(normalizedLabel, nodeIndex);
      state.Slice(i);
      
      ContinueParsing(ref writer.GetReference(nodeIndex), ref state);
      return nodeIndex;
   }

   public bool CanContinue<TData>(ref MarkdownNode node, ref LineState<TData> state, 
      ref BufferWriter<MarkdownNode> writer)
   {
      if (state.IsBlank)
         return false;

      if (node.TitleSpanStart != -1 && node.TitleSpanLength >= 0)
         return false;

      return ContinueParsing(ref node, ref state);
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
               node.TitleSpanLength = (state.GlobalOffset + titleEnd) - node.TitleSpanStart;
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
                  node.TitleSpanStart = state.GlobalOffset + i;
                  
                  if (isClosed)
                  {
                     node.TitleSpanLength = tEnd - i;
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
            node.TitleSpanStart = state.GlobalOffset + i;
            
            if (isClosed)
            {
               node.TitleSpanLength = tEnd - i;
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
                && IsAsciiPunctuation(line[i + 1]))
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
               && IsAsciiPunctuation(line[e + 1]))
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
                       && IsAsciiPunctuation(line[i + 1]))
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
            && IsAsciiPunctuation(line[i + 1]))
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
            && IsAsciiPunctuation(line[i + 1]))
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

   private static string NormalizeLabel(ReadOnlySpan<char> span)
   {
      Span<char> buffer = stackalloc char[span.Length];
      
      var len = 0;
      var lastWasSpace = false;
      var trimmed = span.Trim();
      
      foreach (var c in trimmed)
      {
         if (char.IsWhiteSpace(c))
         {
            if (!lastWasSpace)
            {
               buffer[len++] = ' '; 
               lastWasSpace = true;
            }
         }
         else
         {
            buffer[len++] = char.ToLowerInvariant(c);
            lastWasSpace = false;
         }
      }
      
      // we need it as dictionary key anyway
      return new string(buffer[..len]);
   }

   private static bool IsAsciiPunctuation(char c)
   {
      return c is >= '!' 
         and <= '/' or >= ':' 
         and <= '@' or >= '[' 
         and <= '`' or >= '{' 
         and <= '~';
   }
}