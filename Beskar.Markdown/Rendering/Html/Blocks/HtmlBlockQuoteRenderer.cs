using Beskar.Markdown.Extensions;
using Beskar.Markdown.Parsing.Models;
using Beskar.Markdown.Rendering.Interfaces;
using Me.Memory.Buffers;

namespace Beskar.Markdown.Rendering.Html.Blocks;

public sealed class HtmlBlockQuoteRenderer : INodeRenderer
{
   public int TargetTypeValue => (int)NodeType.BlockQuote;

   public void Render<TData>(
      MarkdownContext<TData> context,
      ReadOnlySpan<char> rawText, 
      ref TextWriterIndentSlim writer, 
      in MarkdownNode current, 
      ReadOnlySpan<MarkdownNode> nodes,
      RenderOptions options)
   {
      writer.Write("<blockquote>");
      current.RenderChildren(rawText, nodes, ref writer, options);
      writer.Write("</blockquote>");
   }
}