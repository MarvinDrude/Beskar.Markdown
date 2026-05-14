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
      }
      else
      {
         var next = text[1];
         var isValidStart = char.IsLetter(next) || next == '/' || next == '!' || next == '?';

         if (isValidStart)
         {
            for (var i = 1; i < text.Length; i++)
            {
               if (text[i] == '>')
               {
                  closeIdx = i;
                  break;
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
