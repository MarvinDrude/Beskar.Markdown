using Beskar.Markdown.Parsing.Interfaces;
using Beskar.Markdown.Parsing.Models;
using Me.Memory.Buffers;

namespace Beskar.Markdown.Parsing.Blocks;

public sealed class HtmlParser : IBlockParser
{
   public int Priority => 4_000;
   public int SupportedTypeValue => (int)NodeType.HtmlBlock;
   
   public int TryMatch<TData>(ref LineState<TData> state, int parentIndex, ref BufferWriter<MarkdownNode> writer)
   {
      if (state.IsBlank || state.LeadingSpaces >= 4)
      {
         return -1;
      }
      
      var line = state.RawLine[state.FirstNonSpaceIndex..];
      if (line.Length < 2 || line[0] != '<')
      {
         return -1;
      }

      var type = GetHtmlBlockType(line);
      if (type == -1 || IsAutolink(line))
      {
         return -1;
      }

      var nodeIndex = writer.WrittenSpan.Length;
      var isClosed = (type <= 5) && HasEndTag(line, type);

      writer.Add(new MarkdownNode()
      {
         Type = NodeType.HtmlBlock,
         TextSpan = new TextSpan(state.GlobalOffset, state.RawLine.Length),
         HtmlBlockType = (byte)type,
         HtmlBlockIsClosed = (byte)(isClosed ? 1 : 0),
         FirstChildIndex = -1,
         NextSiblingIndex = -1
      });

      state.ConsumeRest();
      return nodeIndex;
   }

   public bool CanContinue<TData>(ref MarkdownNode node, ref LineState<TData> state, 
      ref BufferWriter<MarkdownNode> writer)
   {
      if (node.HtmlBlockIsClosed == 1)
      {
         return false;
      }

      var type = node.HtmlBlockType;
      if (type >= 6)
      {
         if (state.IsBlank)
         {
            return false;
         }
      }
      else
      {
         if (HasEndTag(state.RawLine, type))
         {
            node.HtmlBlockIsClosed = 1;
         }
      }
      
      var newLength = (state.GlobalOffset - node.TextSpan.Start) + state.RawLine.Length;
      
      node.TextSpan = node.TextSpan with { Length = newLength };
      state.ConsumeRest();

      return true;
   }
   
   private static int GetHtmlBlockType(ReadOnlySpan<char> line)
   {
      if (line.StartsWith("<!--")) return 2;
      if (line.StartsWith("<?")) return 3;
      if (line.StartsWith("<!") && line.Length > 2 && char.IsAsciiLetterUpper(line[2])) return 4;
      if (line.StartsWith("<![CDATA[")) return 5;

      if (line.StartsWith("<script", StringComparison.OrdinalIgnoreCase) || 
          line.StartsWith("<pre", StringComparison.OrdinalIgnoreCase) || 
          line.StartsWith("<style", StringComparison.OrdinalIgnoreCase))
      {
         var len = line.StartsWith("<script", StringComparison.OrdinalIgnoreCase) ? 7 : (line.StartsWith("<pre", StringComparison.OrdinalIgnoreCase) ? 4 : 6);
         if (line.Length == len || char.IsWhiteSpace(line[len]) || line[len] == '>')
         {
            return 1;
         }
      }

      var i = 1;
      if (line.Length > 1 && line[1] == '/')
      {
         i = 2;
      }

      var nameStart = i;
      while (i < line.Length && char.IsAsciiLetterOrDigit(line[i])) i++;

      if (i <= nameStart) return -1;
      
      var tagName = line.Slice(nameStart, i - nameStart);
      var isBlockTag = IsBlockTag(tagName);
      
      if (isBlockTag)
      {
         if (i == line.Length || char.IsWhiteSpace(line[i]) || line[i] == '>' || (line[i] == '/' && i + 1 < line.Length && line[i + 1] == '>'))
         {
            return 6;
         }
      }
      else
      {
         if (tagName.Length == 1
             && char.ToLowerInvariant(tagName[0]) == 'a')
         {
            return -1;
         }
         
         // Type 7: not a block tag, but must be a complete tag followed only by whitespace
         var closeBracket = -1;
         for (var j = i; j < line.Length; j++)
         {
            if (line[j] != '>') continue;
            closeBracket = j;
            break;
         }

         if (closeBracket != -1)
         {
            for (var j = closeBracket + 1; j < line.Length; j++)
            {
               if (!char.IsWhiteSpace(line[j])) return -1;
            }
            return 7;
         }
      }

      return -1;
   }

   private static bool HasEndTag(ReadOnlySpan<char> line, int type)
   {
      return type switch
      {
         1 => line.Contains("</script>", StringComparison.OrdinalIgnoreCase) || 
              line.Contains("</pre>", StringComparison.OrdinalIgnoreCase) || 
              line.Contains("</style>", StringComparison.OrdinalIgnoreCase),
         2 => line.Contains("-->", StringComparison.Ordinal),
         3 => line.Contains("?>", StringComparison.Ordinal),
         4 => line.Contains(">", StringComparison.Ordinal),
         5 => line.Contains("]]>", StringComparison.Ordinal),
         _ => false
      };
   }
   
   private static bool IsAutolink(ReadOnlySpan<char> line)
   {
      for (var i = 1; i < line.Length; i++)
      {
         var c = line[i];
         if (char.IsWhiteSpace(c) || c == '>' || c == '/') break;
         if (c is ':' or '@') return true;
      }
      return false;
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
