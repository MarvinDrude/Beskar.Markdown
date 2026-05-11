using Beskar.Markdown.Extensions;
using Beskar.Markdown.Parsing.Models;
using Beskar.Markdown.Rendering.Interfaces;
using Me.Memory.Buffers;

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
      
      current.RenderChildren(context, rawText, nodes, ref writer, options);
      writer.Write("</li>");
   }
}