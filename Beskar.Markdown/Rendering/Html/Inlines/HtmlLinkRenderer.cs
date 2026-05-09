using Beskar.Markdown.Extensions;
using Beskar.Markdown.Parsing.Models;
using Beskar.Markdown.Rendering.Interfaces;
using Me.Memory.Buffers;

namespace Beskar.Markdown.Rendering.Html.Inlines;

public sealed class HtmlLinkRenderer : INodeRenderer
{
   public int TargetTypeValue => (int)NodeType.Link;

   public void Render(
      ReadOnlySpan<char> rawText, 
      ref TextWriterIndentSlim writer, 
      in MarkdownNode current, 
      ReadOnlySpan<MarkdownNode> nodes,
      RenderOptions options)
   {
      var url = rawText.Slice(current.LinkUrlStart, current.LinkUrlLength);
      
      writer.WriteInterpolated($"<a href=\"{url}\"");

      if (current.LinkTitleOffset > -1)
      {
         var startIndex = current.LinkUrlStart + current.LinkUrlLength + current.LinkTitleOffset;
         writer.WriteInterpolated($" title=\"{rawText.Slice(startIndex, current.LinkTitleLength)}\"");
      }
      
      writer.Write(">");
      current.RenderChildren(rawText, nodes, ref writer, options);
      writer.Write("</a>");
   }
}