using Beskar.Markdown.Extensions;
using Beskar.Markdown.Parsing.Models;
using Beskar.Markdown.Rendering.Interfaces;
using Beskar.Memory.Buffers;
using Beskar.Memory.Writers;

namespace Beskar.Markdown.Rendering.Html.Inlines;

public sealed class HtmlStrikeThroughRenderer : INodeRenderer
{
   public int TargetTypeValue => (int)NodeType.StrikeThrough;

   public void Render<TData>(
      MarkdownContext<TData> context,
      ReadOnlySpan<char> rawText, 
      ref TextWriterIndentSlim writer, 
      in MarkdownNode current, 
      ReadOnlySpan<MarkdownNode> nodes,
      RenderOptions options)
   {
      writer.Write("<del>");
      
      current.RenderChildren(context, rawText, nodes, ref writer, options);
      
      writer.Write("</del>");
   }
}