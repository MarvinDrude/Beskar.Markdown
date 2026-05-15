using Beskar.Markdown.Extensions;
using Beskar.Markdown.Parsing.Models;
using Beskar.Markdown.Rendering.Interfaces;
using Me.Memory.Buffers;

namespace Beskar.Markdown.Rendering.Html.Inlines;

public sealed class HtmlInlineCodeRenderer : INodeRenderer
{
   public int TargetTypeValue => (int)NodeType.InlineCode;

   public void Render<TData>(
      MarkdownContext<TData> context,
      ReadOnlySpan<char> rawText,
      ref TextWriterIndentSlim writer,
      in MarkdownNode current,
      ReadOnlySpan<MarkdownNode> nodes,
      RenderOptions options)
   {
      writer.Write("<code>");
      var span = current.TextSpan.Slice(rawText);

      if (current.InlineCodeIsInsideTable == 1)
      {
         var i = 0;
         while (i < span.Length)
         {
            var backslashIndex = span[i..].IndexOf('\\');
            if (backslashIndex == -1 || i + backslashIndex + 1 >= span.Length)
            {
               writer.WriteHtmlEncoded(span[i..], encodeApostrophe: false);
               break;
            }

            var absoluteBackslash = i + backslashIndex;
            if (span[absoluteBackslash + 1] == '|')
            {
               writer.WriteHtmlEncoded(span[i..absoluteBackslash], encodeApostrophe: false);
               writer.Write("|");
               i = absoluteBackslash + 2;
            }
            else
            {
               writer.WriteHtmlEncoded(span[i..(absoluteBackslash + 1)], encodeApostrophe: false);
               i = absoluteBackslash + 1;
            }
         }
      }
      else
      {
         WriteWithLineEndingNormalization(ref writer, span);
      }

      writer.Write("</code>");
   }

   private static void WriteWithLineEndingNormalization(ref TextWriterIndentSlim writer, ReadOnlySpan<char> span)
   {
      var i = 0;
      while (i < span.Length)
      {
         var newlineIdx = span[i..].IndexOfAny('\n', '\r');
         if (newlineIdx == -1)
         {
            writer.WriteHtmlEncoded(span[i..], encodeApostrophe: false);
            return;
         }

         var absIdx = i + newlineIdx;
         writer.WriteHtmlEncoded(span[i..absIdx], encodeApostrophe: false);
         writer.Write(" ");

         i = absIdx + 1;
         if (i < span.Length && span[absIdx] == '\r' && span[i] == '\n')
            i++;
      }
   }
}