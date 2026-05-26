using Beskar.Markdown.Parsing.Interfaces;
using Beskar.Markdown.Parsing.Models;
using Beskar.Markdown.Parsing.Utils;
using Beskar.Memory.Buffers;
using Beskar.Memory.Writers;

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

      var blockText = state.RawText[state.GlobalOffset..state.BlockEnd];
      var closeIdx = -1;

      if (blockText.StartsWith("<![CDATA["))
      {
         var terminatorIdx = blockText.IndexOf("]]>");

         if (terminatorIdx != -1)
         {
            closeIdx = terminatorIdx + 2;
         }
      }
      else
      {
         if (blockText.Length < 3) return false;

         if (HtmlTagUtils.TryParseClosingTag(blockText, out var closingTagEnd))
         {
            closeIdx = closingTagEnd - 1;
         }
         else if (text[1] == '!')
         {
            if (text.StartsWith("<!--", StringComparison.Ordinal))
            {
               if (blockText.StartsWith("<!-->", StringComparison.Ordinal))
               {
                  closeIdx = 4;
               }
               else if (blockText.StartsWith("<!--->", StringComparison.Ordinal))
               {
                  closeIdx = 5;
               }
               else
               {
                  var endIdx = blockText[4..].IndexOf("-->", StringComparison.Ordinal);
                  if (endIdx != -1) closeIdx = endIdx + 4 + 2;
               }
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
         else if (HtmlTagUtils.TryParseOpenTag(blockText, out var openTagEnd))
         {
            closeIdx = openTagEnd - 1;
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
