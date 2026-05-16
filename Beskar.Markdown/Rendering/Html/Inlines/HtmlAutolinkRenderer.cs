using Beskar.Markdown.Extensions;
using Beskar.Markdown.Parsing.Models;
using Beskar.Markdown.Rendering.Interfaces;
using Me.Memory.Buffers;

namespace Beskar.Markdown.Rendering.Html.Inlines;

public sealed class HtmlAutolinkRenderer : INodeRenderer
{
   public int TargetTypeValue => (int)NodeType.Autolink;

   public void Render<TData>(
      MarkdownContext<TData> context,
      ReadOnlySpan<char> rawText, 
      ref TextWriterIndentSlim writer, 
      in MarkdownNode current, 
      ReadOnlySpan<MarkdownNode> nodes,
      RenderOptions options)
   {
      var url = current.TextSpan.Slice(rawText);

      writer.Write("<a href=\"");
      if (current.IsEmail != 0)
      {
         writer.Write("mailto:");
      }
      
      writer.WriteCommonMarkdownUrlEncoded(url, processEscapes: false);
      writer.Write("\">");
      
      writer.WriteHtmlEncoded(url, encodeApostrophe: false);
      writer.Write("</a>");
   }
}