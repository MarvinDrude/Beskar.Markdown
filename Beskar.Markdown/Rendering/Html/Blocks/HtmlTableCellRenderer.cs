using Beskar.Markdown.Extensions;
using Beskar.Markdown.Parsing.Models;
using Beskar.Markdown.Rendering.Interfaces;
using Beskar.Memory.Buffers;
using Beskar.Memory.Writers;

namespace Beskar.Markdown.Rendering.Html.Blocks;

public sealed class HtmlTableCellRenderer : INodeRenderer
{
   public int TargetTypeValue => (int)NodeType.TableCell;

   public void Render<TData>(
      MarkdownContext<TData> context,
      ReadOnlySpan<char> rawText, 
      ref TextWriterIndentSlim writer, 
      in MarkdownNode current, 
      ReadOnlySpan<MarkdownNode> nodes,
      RenderOptions options)
   {
      var tag = current.IsHeaderCell == 1 ? "th" : "td";
      writer.WriteInterpolated($"<{tag}");

      if (current.TableCellAlignment != 0)
      {
         var align = current.TableCellAlignment switch
         {
            1 => "left",
            2 => "center",
            3 => "right",
            _ => null
         };
         if (align != null)
         {
            writer.WriteInterpolated($" align=\"{align}\"");
         }
      }

      writer.Write(">");
      current.RenderChildren(context, rawText, nodes, ref writer, options);
      writer.WriteInterpolated($"</{tag}>");
   }
}
