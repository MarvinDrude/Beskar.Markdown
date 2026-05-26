using Beskar.Markdown.Extensions;
using Beskar.Markdown.Parsing.Models;
using Beskar.Markdown.Rendering.Interfaces;
using Beskar.Memory.Buffers;
using Beskar.Memory.Writers;

namespace Beskar.Markdown.Rendering.Html.Blocks;

public sealed class HtmlListItemRenderer : INodeRenderer
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
      var hasBlockChildren = current.FirstChildIndex != -1
         && (nodes[current.FirstChildIndex].Type != NodeType.Paragraph
             || nodes[current.FirstChildIndex].ParagraphIsWrapped == 1);

      writer.Write("<li>");

      if (current.TaskListStatus != 0)
      {
         if (current.TaskListStatus == 1) // unchecked
         {
            writer.Write("<input disabled=\"\" type=\"checkbox\"> ");
         }
         else // checked
         {
            writer.Write("<input checked=\"\" disabled=\"\" type=\"checkbox\"> ");
         }
      }

      if (hasBlockChildren && options.AddBlockNewLines)
      {
         writer.WriteLine();
      }

      current.RenderChildren(context, rawText, nodes, ref writer, options);

      if (options.AddBlockNewLines)
      {
         writer.WriteLine("</li>");
      }
      else
      {
         writer.Write("</li>");
      }
   }
}
