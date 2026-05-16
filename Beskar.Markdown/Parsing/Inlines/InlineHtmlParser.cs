using Beskar.Markdown.Parsing.Interfaces;
using Beskar.Markdown.Parsing.Models;
using Me.Memory.Buffers;

namespace Beskar.Markdown.Parsing.Inlines;

public sealed class InlineHtmlParser : IInlineParser
{
   public int Priority => 6_000;
   public int SupportedTypeValue => (int)NodeType.InlineHtml;

   public char TriggerChar => '<';
   public char TriggerAltChar => '<';
   
   public bool TryMatch<TData>(ref InlineState<TData> state, int parentIndex, 
      ref BufferWriter<MarkdownNode> writer, scoped ref InlineParser<TData> parser,
      ParserOptions options)
   {
      var text = state.RemainingText;
      if (text.Length < 3) return false;

      var closeIdx = -1;

      if (text.StartsWith("<![CDATA["))
      {
         var terminatorIdx = text.IndexOf("]]>");
         if (terminatorIdx != -1)
         {
            closeIdx = terminatorIdx + 2;
         }
         else
         {
            // Search in full text
            var fullText = state.RawText[state.GlobalOffset..];
            terminatorIdx = fullText.IndexOf("]]>");
            if (terminatorIdx != -1)
            {
               closeIdx = terminatorIdx + 2;
            }
         }
      }
      else
      {
         if (text.Length < 3) return false;
         
         if (text[1] == '/')
         {
            var i = 2;
            while (i < text.Length && (char.IsAsciiLetterOrDigit(text[i]) || text[i] == '-')) 
               i++;
            
            if (i == 2) 
               return false;
            
            while (i < text.Length && char.IsWhiteSpace(text[i])) 
               i++;
            
            if (i < text.Length && text[i] == '>')
            {
               closeIdx = i;
            }
         }
         else if (text[1] == '!')
         {
            if (text.StartsWith("<!--", StringComparison.Ordinal))
            {
               var endIdx = text[4..].IndexOf("-->", StringComparison.Ordinal);
               if (endIdx != -1) closeIdx = endIdx + 4 + 2;
            }
            else if (text.Length > 2 && char.IsAsciiLetterUpper(text[2]))
            {
               var endIdx = text.IndexOf('>');
               if (endIdx != -1) closeIdx = endIdx;
            }
         }
         else if (text[1] == '?')
         {
            var endIdx = text[2..].IndexOf("?>", StringComparison.Ordinal);
            if (endIdx != -1) closeIdx = endIdx + 2 + 1;
         }
         else if (char.IsAsciiLetter(text[1]))
         {
            var i = 2;
            while (i < text.Length && (char.IsAsciiLetterOrDigit(text[i]) || text[i] == '-')) 
               i++;
            
            if (i < text.Length)
            {
               var c = text[i];
               if (c == '>' || char.IsWhiteSpace(c) || (c == '/' && i + 1 < text.Length && text[i + 1] == '>'))
               {
                  var endIdx = text.IndexOf('>');
                  if (endIdx != -1) closeIdx = endIdx;
               }
            }
         }
      }

      if (closeIdx == -1) return false;

      var nodeLength = closeIdx + 1;
      var nodeIndex = writer.WrittenSpan.Length;
      
      writer.Add(new MarkdownNode()
      {
         Type = NodeType.InlineHtml,
         TextSpan = new TextSpan(state.GlobalOffset, nodeLength),
         FirstChildIndex = -1,
         NextSiblingIndex = -1
      });

      parser.LinkInlineNode(ref writer, parentIndex, nodeIndex);
      state.Advance(nodeLength);
      
      return true;
   }
}
