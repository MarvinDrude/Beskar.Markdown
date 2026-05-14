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
      writer.WriteLine("<blockquote>");
      current.RenderChildren(context, rawText, nodes, ref writer, options);

      if (options.AddBlockNewLines)
      {
         writer.WriteLine("</blockquote>");
      }
      else
      {
         writer.Write("</blockquote>");
      }
   }
}