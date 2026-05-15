using Beskar.Markdown.Extensions;
using Beskar.Markdown.Parsing.Models;
using Beskar.Markdown.Rendering.Interfaces;
using Me.Memory.Buffers;

namespace Beskar.Markdown.Rendering.Html.Inlines;

public sealed class HtmlLinkRenderer : INodeRenderer
{
   public int TargetTypeValue => (int)NodeType.Link;

   public void Render<TData>(
      MarkdownContext<TData> context,
      ReadOnlySpan<char> rawText, 
      ref TextWriterIndentSlim writer, 
      in MarkdownNode current, 
      ReadOnlySpan<MarkdownNode> nodes,
      RenderOptions options)
   {
      var url = rawText.Slice(current.LinkUrlStart, current.LinkUrlLength);
      
      writer.Write("<a href=\"");
      writer.WriteMarkdownUrlEncoded(url);
      writer.Write("\"");

      if (current.LinkTitleOffset > -1)
      {
         var startIndex = current.LinkUrlStart + current.LinkUrlLength + current.LinkTitleOffset;
         writer.Write(" title=\"");
         WriteUnescapedHtmlEncoded(ref writer, rawText.Slice(startIndex, current.LinkTitleLength));
         writer.Write("\"");
      }
      
      writer.Write(">");
      current.RenderChildren(context, rawText, nodes, ref writer, options);
      writer.Write("</a>");
   }
   
   private static void WriteUnescapedHtmlEncoded(ref TextWriterIndentSlim writer, ReadOnlySpan<char> text)
   {
      var currentIndex = 0;
      var chunkStart = 0;

      while (currentIndex < text.Length)
      {
         if (text[currentIndex] == '\\' && currentIndex + 1 < text.Length && IsAsciiPunctuation(text[currentIndex + 1]))
         {
            if (currentIndex > chunkStart)
            {
               writer.WriteHtmlEncoded(text.Slice(chunkStart, currentIndex - chunkStart));
            }
            
            writer.WriteHtmlEncoded(text.Slice(currentIndex + 1, 1));
            
            currentIndex += 2;
            chunkStart = currentIndex;
         }
         else
         {
            currentIndex++;
         }
      }

      if (chunkStart < text.Length)
      {
         writer.WriteHtmlEncoded(text[chunkStart..]);
      }
   }

   private static bool IsAsciiPunctuation(char c)
   {
      return c is >= '!' and <= '/' 
         or >= ':' and <= '@' 
         or >= '[' and <= '`' 
         or >= '{' and <= '~';
   }
}