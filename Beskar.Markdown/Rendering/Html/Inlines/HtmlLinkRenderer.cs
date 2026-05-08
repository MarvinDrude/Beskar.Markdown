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
      ReadOnlySpan<char> title = [];
      
      if (current.FirstChildIndex != -1)
      {
         var child = nodes[current.FirstChildIndex];
         title = child.TextSpan.Slice(rawText);
      }
      
      writer.WriteInterpolated($"<a href=\"{url}\">");
      if (title.Length > 0)
      {
         writer.Write(title);
      }
      writer.Write("</a>");
   }
}