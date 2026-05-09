using Beskar.Markdown.Parsing.Interfaces;
using Beskar.Markdown.Parsing.Models;
using Me.Memory.Buffers;

namespace Beskar.Markdown.Parsing.Blocks;

public sealed class HtmlParser : IBlockParser
{
   public int Priority => 4_000;
   public int SupportedTypeValue => (int)NodeType.HtmlBlock;
   
   public int TryMatch(ref LineState state, int parentIndex, ref BufferWriter<MarkdownNode> writer)
   {
      if (state.IsBlank || state.LeadingSpaces >= 4)
      {
         return -1;
      }
      
      var line = state.RawLine;
      var startIndex = state.FirstNonSpaceIndex;

      // Must start with an open angle bracket
      if (line.Length <= startIndex + 1 || line[startIndex] != '<')
      {
         return -1;
      }
      
      var nextChar = line[startIndex + 1];
      var isTag = char.IsLetter(nextChar) || // <div>
         nextChar == '/' || // </div>
         nextChar == '!' || // <!-- or <!DOCTYPE
         nextChar == '?';   // <?xml processing instruction

      if (!isTag)
      {
         return -1;
      }
      
      var nameStart = startIndex + 1;
      var isClosing = false;

      if (nextChar == '/')
      {
         isClosing = true;
         nameStart++;
      }
      
      if (nameStart < line.Length && char.IsLetter(line[nameStart]))
      {
         var nameEnd = nameStart;
         while (nameEnd < line.Length && char.IsLetterOrDigit(line[nameEnd]))
         {
            nameEnd++;
         }

         var tagName = line.Slice(nameStart, nameEnd - nameStart);
         var isBlockTag = IsBlockTag(tagName);

         if (!isClosing)
         {
            var isAutolink = false;
            for (var i = startIndex + 1; i < line.Length; i++)
            {
               var c = line[i];
               if (char.IsWhiteSpace(c) || c == '>' || c == '/')
               {
                  break;
               }

               if (c != ':' && c != '@') continue;
               
               isAutolink = true;
               break;
            }

            if (isAutolink)
            {
               return -1;
            }
         }

         if (!isBlockTag)
         {
            var closeBracket = -1;
            for (var i = nameEnd; i < line.Length; i++)
            {
               if (line[i] != '>') continue;
               
               closeBracket = i;
               break;
            }

            if (closeBracket == -1)
            {
               return -1;
            }

            for (var i = closeBracket + 1; i < line.Length; i++)
            {
               if (!char.IsWhiteSpace(line[i]))
               {
                  return -1;
               }
            }
         }
      }

      var nodeIndex = writer.WrittenSpan.Length;
      writer.Add(new MarkdownNode()
      {
         Type = NodeType.HtmlBlock,
         TextSpan = new TextSpan(state.GlobalOffset, state.RawLine.Length),
         FirstChildIndex = -1,
         NextSiblingIndex = -1
      });

      state.ConsumeRest();
      return nodeIndex;
   }

   public bool CanContinue(ref MarkdownNode node, ref LineState state, ref BufferWriter<MarkdownNode> writer)
   {
      if (state.IsBlank)
      {
         return false;
      }
      
      var newLength = (state.GlobalOffset - node.TextSpan.Start) + state.RawLine.Length;
      
      node.TextSpan = node.TextSpan with { Length = newLength };
      state.ConsumeRest();

      return true;
   }
   
   private static bool IsBlockTag(ReadOnlySpan<char> tagName)
   {
      if (tagName.Length > 20) return false;

      Span<char> lower = stackalloc char[tagName.Length];
      tagName.ToLowerInvariant(lower);

      return lower switch
      {
         "address" or "article" or "aside" or "base" or "basefont" or "blockquote" or
            "body" or "caption" or "center" or "col" or "colgroup" or "dd" or "details" or
            "dialog" or "dir" or "div" or "dl" or "dt" or "fieldset" or "figcaption" or
            "figure" or "footer" or "form" or "frame" or "frameset" or "h1" or "h2" or
            "h3" or "h4" or "h5" or "h6" or "head" or "header" or "hr" or "html" or
            "iframe" or "legend" or "li" or "link" or "main" or "menu" or "menuitem" or
            "nav" or "noframes" or "ol" or "optgroup" or "option" or "p" or "param" or
            "section" or "source" or "summary" or "table" or "tbody" or "td" or "tfoot" or
            "th" or "thead" or "title" or "tr" or "track" or "ul" or
            "script" or "pre" or "style" => true,
         _ => false
      };
   }
}
