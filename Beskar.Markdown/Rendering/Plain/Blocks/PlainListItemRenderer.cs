using Beskar.Markdown.Extensions;
using Beskar.Markdown.Parsing.Models;
using Beskar.Markdown.Rendering.Interfaces;
using Beskar.Memory.Buffers;
using Beskar.Memory.Writers;

namespace Beskar.Markdown.Rendering.Plain.Blocks;

public sealed class PlainListItemRenderer : INodeRenderer
{
   public int TargetTypeValue => (int)NodeType.ListItem;

   public void Render<TData>(
      MarkdownContext<TData> context,
      ReadOnlySpan<char> rawText, 
      ref TextWriterIndentSlim writer, 
      in MarkdownNode current, 
      ReadOnlySpan<MarkdownNode> nodes,
      RenderOptions options)
   {
      if (current.TaskListStatus != 0)
      {
         if (current.TaskListStatus == 1) // unchecked
         {
            writer.Write("[ ] ");
         }
         else // checked
         {
            writer.Write("[x] ");
         }
      }

      current.RenderChildren(context, rawText, nodes, ref writer, options);

      if (options.AddBlockNewLines)
      {
         writer.WriteLine();
      }
   }
}
