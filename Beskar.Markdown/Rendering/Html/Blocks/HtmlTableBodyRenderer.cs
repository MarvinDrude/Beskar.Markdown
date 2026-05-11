using Beskar.Markdown.Extensions;
using Beskar.Markdown.Parsing.Models;
using Beskar.Markdown.Rendering.Interfaces;
using Me.Memory.Buffers;

namespace Beskar.Markdown.Rendering.Html.Blocks;

public sealed class HtmlTableBodyRenderer : INodeRenderer
{
   public int TargetTypeValue => (int)NodeType.TableBody;

   public void Render<TData>(
      MarkdownContext<TData> context,
      ReadOnlySpan<char> rawText, 
      ref TextWriterIndentSlim writer, 
      in MarkdownNode current, 
      ReadOnlySpan<MarkdownNode> nodes,
      RenderOptions options)
   {
      if (current.FirstChildIndex == -1) return;

      writer.Write("<tbody>");
      current.RenderChildren(context, rawText, nodes, ref writer, options);
      writer.Write("</tbody>");
   }
}
