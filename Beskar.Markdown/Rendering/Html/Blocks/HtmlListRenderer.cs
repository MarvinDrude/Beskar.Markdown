using Beskar.Markdown.Extensions;
using Beskar.Markdown.Parsing.Models;
using Beskar.Markdown.Rendering.Interfaces;
using Me.Memory.Buffers;

namespace Beskar.Markdown.Rendering.Html.Blocks;

public sealed class HtmlListRenderer : INodeRenderer
{
   public int TargetTypeValue => (int)NodeType.List;

   public void Render<TData>(
      MarkdownContext<TData> context,
      ReadOnlySpan<char> rawText, 
      ref TextWriterIndentSlim writer, 
      in MarkdownNode current, 
      ReadOnlySpan<MarkdownNode> nodes,
      RenderOptions options)
   {
      var type = current.ListMarker is '.' or ')' ? "ol" : "ul";
      
      writer.WriteInterpolated($"<{type}");

      if (current.ListStartNumber > 1)
      {
         writer.WriteInterpolated($" start=\"{current.ListStartNumber}\"");
      }
      writer.Write(">");
      
      current.RenderChildren(context, rawText, nodes, ref writer, options);
      
      writer.WriteInterpolated($"</{type}>");
   }
}