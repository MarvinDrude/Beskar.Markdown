using Beskar.Markdown.Extensions;
using Beskar.Markdown.Parsing.Models;
using Beskar.Markdown.Rendering.Interfaces;
using Beskar.Memory.Buffers;
using Beskar.Memory.Writers;

namespace Beskar.Markdown.Rendering.Plain.Inlines;

public sealed class PlainInlineCodeRenderer : INodeRenderer
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
      var span = current.TextSpan.Slice(rawText);

      if (current.InlineCodeIsInsideTable == 1)
      {
         var i = 0;
         while (i < span.Length)
         {
            var backslashIndex = span[i..].IndexOf('\\');
            if (backslashIndex == -1 || i + backslashIndex + 1 >= span.Length)
            {
               writer.Write(span[i..]);
               break;
            }

            var absoluteBackslash = i + backslashIndex;
            if (span[absoluteBackslash + 1] == '|')
            {
               writer.Write(span[i..absoluteBackslash]);
               writer.Write("|");
               i = absoluteBackslash + 2;
            }
            else
            {
               writer.Write(span[i..(absoluteBackslash + 1)]);
               i = absoluteBackslash + 1;
            }
         }
      }
      else
      {
         WriteWithLineEndingNormalization(ref writer, span);
      }
   }

   private static void WriteWithLineEndingNormalization(ref TextWriterIndentSlim writer, ReadOnlySpan<char> span)
   {
      var i = 0;
      while (i < span.Length)
      {
         var newlineIdx = span[i..].IndexOfAny('\n', '\r');
         if (newlineIdx == -1)
         {
            writer.Write(span[i..]);
            return;
         }

         var absIdx = i + newlineIdx;
         writer.Write(span[i..absIdx]);
         writer.Write(" ");

         i = absIdx + 1;
         if (i < span.Length && span[absIdx] == '\r' && span[i] == '\n')
            i++;
      }
   }
}
