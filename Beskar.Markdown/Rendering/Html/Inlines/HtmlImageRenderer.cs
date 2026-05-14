using Beskar.Markdown.Extensions;
using Beskar.Markdown.Parsing.Models;
using Beskar.Markdown.Rendering.Interfaces;
using Me.Memory.Buffers;

namespace Beskar.Markdown.Rendering.Html.Inlines;

public sealed class HtmlImageRenderer : INodeRenderer
{
   public int TargetTypeValue => (int)NodeType.Image;

   public void Render<TData>(
      MarkdownContext<TData> context,
      ReadOnlySpan<char> rawText, 
      ref TextWriterIndentSlim writer, 
      in MarkdownNode current, 
      ReadOnlySpan<MarkdownNode> nodes,
      RenderOptions options)
   {
      var url = rawText.Slice(current.LinkUrlStart, current.LinkUrlLength);
      ReadOnlySpan<char> title = [];
      
      if (current.FirstChildIndex != -1)
      {
         var child = nodes[current.FirstChildIndex];
         title = child.TextSpan.Slice(rawText);
      }
      
      writer.WriteInterpolated($"<img src=\"");
      writer.WriteHtmlEncoded(url, encodeApostrophe: false);
      writer.Write("\" alt=\"");
      writer.WriteHtmlEncoded(title, encodeApostrophe: false);
      writer.Write("\" ");

      if (current.LinkTitleOffset > 0)
      {
         var startIndex = current.LinkUrlStart + current.LinkUrlLength + current.LinkTitleOffset;
         writer.Write("title=\"");
         writer.WriteHtmlEncoded(rawText.Slice(startIndex, current.LinkTitleLength), encodeApostrophe: false);
         writer.Write("\" ");
      }
      
      writer.Write("/>");
   }
}