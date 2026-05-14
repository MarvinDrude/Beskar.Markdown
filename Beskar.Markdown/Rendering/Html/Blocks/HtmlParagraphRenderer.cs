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
      writer.Write("<p>");
      current.RenderChildren(context, rawText, nodes, ref writer, options);
      
      if (options.AddBlockNewLines)
      {
         writer.WriteLine("</p>");
      }
      else
      {
         writer.Write("</p>");
      }
   }
}