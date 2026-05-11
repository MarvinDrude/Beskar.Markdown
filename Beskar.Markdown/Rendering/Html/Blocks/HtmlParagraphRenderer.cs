using Beskar.Markdown.Extensions;
using Beskar.Markdown.Parsing.Models;
using Beskar.Markdown.Rendering.Interfaces;
using Me.Memory.Buffers;

namespace Beskar.Markdown.Rendering.Html.Blocks;

public sealed class HtmlParagraphRenderer : INodeRenderer
{
   public int TargetTypeValue => (int)NodeType.Paragraph;

   public void Render<TData>(
      MarkdownContext<TData> context,
      ReadOnlySpan<char> rawText, 
      ref TextWriterIndentSlim writer, 
      in MarkdownNode current, 
      ReadOnlySpan<MarkdownNode> nodes,
      RenderOptions options)
   {
      if (current.IsInsideListItem != 1 || current.ParagraphIsWrapped == 1)
      {
         writer.Write("<p>");
      }
      
      current.RenderChildren(context, rawText, nodes, ref writer, options);
      
      if (current.IsInsideListItem != 1 || current.ParagraphIsWrapped == 1)
      {
         writer.Write("</p>");
      }
   }
}