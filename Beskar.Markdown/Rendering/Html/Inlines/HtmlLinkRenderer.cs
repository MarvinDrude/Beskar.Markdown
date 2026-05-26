using Beskar.Markdown.Extensions;
using Beskar.Markdown.Parsing.Models;
using Beskar.Markdown.Parsing.Utils;
using Beskar.Markdown.Rendering.Interfaces;
using Beskar.Memory.Buffers;
using Beskar.Memory.Writers;

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
      writer.WriteCommonMarkdownUrlEncoded(url);
      writer.Write("\"");

      if (current.LinkTitleOffset > -1)
      {
         var startIndex = current.LinkUrlStart + current.LinkUrlLength + current.LinkTitleOffset;
         writer.Write(" title=\"");
         writer.WriteHtmlDecodedAndEncoded(rawText.Slice(startIndex, current.LinkTitleLength), encodeApostrophe: false);
         writer.Write("\"");
      }
      
      writer.Write(">");
      current.RenderChildren(context, rawText, nodes, ref writer, options);
      writer.Write("</a>");
   }
}
