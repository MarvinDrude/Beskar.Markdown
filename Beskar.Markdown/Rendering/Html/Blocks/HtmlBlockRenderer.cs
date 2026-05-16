using Beskar.Markdown.Extensions;
using Beskar.Markdown.Parsing.Models;
using Beskar.Markdown.Rendering.Interfaces;
using Me.Memory.Buffers;

namespace Beskar.Markdown.Rendering.Html.Blocks;

public sealed class HtmlBlockRenderer : INodeRenderer
{
   public int TargetTypeValue => (int)NodeType.HtmlBlock;

   public void Render<TData>(
      MarkdownContext<TData> context,
      ReadOnlySpan<char> rawText, 
      ref TextWriterIndentSlim writer, 
      in MarkdownNode current, 
      ReadOnlySpan<MarkdownNode> nodes,
      RenderOptions options)
   {
      var span = current.TextSpan;

      if (options.AddBlockNewLines)
      {
         WriteHtmlBlockText(span.Slice(rawText), ref writer);
         writer.WriteLine();
      }
      else
      {
         WriteHtmlBlockText(span.Slice(rawText), ref writer);
      }
   }

   private static void WriteHtmlBlockText(ReadOnlySpan<char> text, ref TextWriterIndentSlim writer)
   {
      var lineStart = 0;
      while (lineStart < text.Length)
      {
         var lineEnd = lineStart;
         
         while (lineEnd < text.Length && text[lineEnd] is not ('\r' or '\n'))
         {
            lineEnd++;
         }

         var line = text[lineStart..lineEnd];
         var contentStart = 0;
         
         while (contentStart < line.Length && contentStart < 3 && line[contentStart] == ' ')
         {
            contentStart++;
         }

         if (contentStart < line.Length && line[contentStart] == '>')
         {
            contentStart++;
            if (contentStart < line.Length && line[contentStart] == ' ')
            {
               contentStart++;
            }
         }
         else
         {
            contentStart = 0;
         }

         writer.Write(line[contentStart..]);

         if (lineEnd >= text.Length)
         {
            break;
         }

         if (text[lineEnd] == '\r' && lineEnd + 1 < text.Length && text[lineEnd + 1] == '\n')
         {
            writer.WriteLine();
            lineStart = lineEnd + 2;
         }
         else
         {
            writer.WriteLine();
            lineStart = lineEnd + 1;
         }
      }
   }
}
