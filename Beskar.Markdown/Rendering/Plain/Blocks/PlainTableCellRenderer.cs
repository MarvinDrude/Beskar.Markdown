using Beskar.Markdown.Extensions;
using Beskar.Markdown.Parsing.Models;
using Beskar.Markdown.Rendering.Interfaces;
using Beskar.Memory.Buffers;
using Beskar.Memory.Writers;

namespace Beskar.Markdown.Rendering.Plain.Blocks;

public sealed class PlainTableCellRenderer : INodeRenderer
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
      current.RenderChildren(context, rawText, nodes, ref writer, options);

      if (current.NextSiblingIndex != -1)
      {
         writer.Write("\t");
      }
   }
}
