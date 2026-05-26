using Beskar.Markdown.Extensions;
using Beskar.Markdown.Parsing.Models;
using Beskar.Markdown.Rendering.Interfaces;
using Beskar.Memory.Buffers;
using Beskar.Memory.Writers;

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

      if (type == "ol" && current.ListStartNumber != 1)
      {
         writer.WriteInterpolated($" start=\"{current.ListStartNumber}\"");
      }

      if (options.AddBlockNewLines)
      {
         writer.WriteLine(">");
      }
      else
      {
         writer.Write(">");
      }
      
      current.RenderChildren(context, rawText, nodes, ref writer, options);
      
      if (options.AddBlockNewLines)
      {
         writer.WriteLineInterpolated($"</{type}>");
      }
      else
      {
         writer.WriteInterpolated($"</{type}>");
      }
   }
}
