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
      ReadOnlySpan<char> preamble = [];

      if (IsEmailAutolink(url))
      {
         preamble = "mailto:";
      }
      
      writer.Write("<a href=\"");
      writer.WriteHtmlEncoded(preamble, encodeApostrophe: false);
      writer.WriteHtmlEncoded(url, encodeApostrophe: false);
      writer.Write("\">");
      
      if (url.Length > 0)
      {
         writer.WriteHtmlEncoded(url, encodeApostrophe: false);
      }
      writer.Write("</a>");
   }
   
   private static bool IsEmailAutolink(ReadOnlySpan<char> content)
   {
      var atIdx = content.IndexOf('@');
      return atIdx > 0 && atIdx < content.Length - 1;
   }
}